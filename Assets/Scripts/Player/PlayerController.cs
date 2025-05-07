// PlayerController.cs
using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    [Header("Skin")]
    [Tooltip("Child into which we instantiate the chosen skin prefab")]
    public Transform skinRoot;

    [Header("Ground")]
    [Tooltip("Layers considered ground for raycasts")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Move")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 20f;
    public float clickRadius = 0.3f;

    [Header("Rotate")]
    public float rotationSpeed = 360f;

    [Header("Jump/Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Teleport")]
    public float teleportDistance = 5f;
    public float teleportCooldown = 20f;

    // 1) Networked skin index
    [Networked] public int SkinIndex { get; set; }

    // internals
    CharacterController _controller;
    Camera _mainCamera;
    Vector3 _clickPoint;
    Vector3 _clickDir;
    bool _mouseHeld;
    bool _isMoving;
    Vector3 _currentVel;
    float _verticalVel;
    bool _jumpReq;
    bool _canTeleport = true;
    float _teleportTimer;

    // track last to know when to reapply
    int _lastSkinIndex = -1;

    public override void Spawned()
    {
        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        _currentVel = Vector3.zero;
        _verticalVel = 0f;
        _mouseHeld = false;
        _isMoving = false;

        // 2) Only the StateAuthority picks the networked skin
        if (HasStateAuthority)
        {
            int idx = SkinSelection.instance.GetCurrentIndex();
            SkinIndex = Mathf.Clamp(idx, 0, SkinSelection.instance.skins.Count - 1);
        }
    }

    void Update()
    {
        if (!HasInputAuthority) return;

        HandleMouseInput();
        HandleJumpInput();

        // Local preview: skin faces the click while dragging
        if (_mouseHeld)
            RotateSkinTo(_clickDir);
    }

    public override void FixedUpdateNetwork()
    {
        // 3) Skin‐sync on *all* clients
        if (SkinIndex != _lastSkinIndex)
        {
            ApplySkin(SkinIndex);
            _lastSkinIndex = SkinIndex;
        }

        // 4) Movement+teleport only on the owner
        if (!HasInputAuthority)
        {
            // But even remotes should see the skin face run‐direction:
            if (_currentVel.sqrMagnitude > 0.001f)
                RotateSkinTo(_currentVel.normalized);
            return;
        }

        HandleTeleport();
        ApplyJumpGravity();
        HandleMovement();

        // After movement, owner sees skin face run‐dir too
        if (_currentVel.sqrMagnitude > 0.001f)
            RotateSkinTo(_currentVel.normalized);

        HandleTeleportCooldown();
    }

    // — Input & click handling —
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            _mouseHeld = true;

        if (_mouseHeld)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer))
            {
                _clickPoint = hit.point;
                Vector3 flat = _clickPoint - transform.position;
                flat.y = 0;
                _clickDir = flat.sqrMagnitude > 0.001f
                    ? flat.normalized
                    : transform.forward;
                _isMoving = flat.magnitude > clickRadius;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _mouseHeld = false;
                _isMoving = false;
            }
        }
    }

    // — Skin rotation helper —
    void RotateSkinTo(Vector3 dir)
    {
        if (skinRoot == null) return;
        Quaternion desired = Quaternion.LookRotation(dir, Vector3.up);
        skinRoot.rotation = Quaternion.RotateTowards(
            skinRoot.rotation,
            desired,
            rotationSpeed * Time.deltaTime
        );
    }

    // — Jump & gravity —
    void HandleJumpInput()
    {
        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
            _jumpReq = true;
    }
    void ApplyJumpGravity()
    {
        if (_controller.isGrounded && _verticalVel < 0f)
            _verticalVel = 0f;
        if (_jumpReq)
        {
            _verticalVel = Mathf.Sqrt(2f * jumpHeight * -gravity);
            _jumpReq = false;
        }
        _verticalVel += gravity * Runner.DeltaTime;
    }

    // — Movement with acceleration/friction —
    void HandleMovement()
    {
        Vector3 targetVel = _isMoving ? _clickDir * moveSpeed : Vector3.zero;
        float rate = _isMoving ? acceleration : deceleration;
        _currentVel = Vector3.MoveTowards(
            _currentVel,
            targetVel,
            rate * Runner.DeltaTime
        );
        Vector3 m = _currentVel;
        m.y = _verticalVel;
        _controller.Move(m * Runner.DeltaTime);
    }

    // — Teleport with Q & cooldown —
    void HandleTeleport()
    {
        if (_mouseHeld && _isMoving && _canTeleport && Input.GetKeyDown(KeyCode.Q))
        {
            transform.position += _clickDir * teleportDistance;
            _canTeleport = false;
            _teleportTimer = teleportCooldown;
        }
    }
    void HandleTeleportCooldown()
    {
        if (!_canTeleport)
        {
            _teleportTimer -= Runner.DeltaTime;
            if (_teleportTimer <= 0f)
                _canTeleport = true;
        }
    }

    // — Instantiate the chosen skin under skinRoot —
    void ApplySkin(int index)
    {
        foreach (Transform t in skinRoot) Destroy(t.gameObject);
        var prefab = SkinSelection.instance.skins[index];
        var inst = Instantiate(prefab, skinRoot);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;
    }
}