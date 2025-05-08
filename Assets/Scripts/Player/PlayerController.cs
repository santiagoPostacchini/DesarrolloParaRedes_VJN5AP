using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    [Header("Skin")]
    public Transform skinRoot;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Move")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 20f;
    public float clickRadius = 0.3f;

    [Header("Rotate")]
    public float rotationSpeed = 360f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Teleport")]
    public float teleportDistance = 5f;
    public float teleportCooldown = 20f;

    [Header("Melee Hit")]
    public float hitRadius = 0.5f;
    public float hitRange = 0.3f;
    public LayerMask hitLayer;
    public float hitCooldown = 1.0f;

    // ── Networked State ────────────────────────────────────────
    [Networked] public int SkinIndex { get; set; }

    // ── Private State ──────────────────────────────────────────
    private CharacterController _cc;
    private Camera _cam;
    private Vector3 _clickDir;
    private bool _mouseHeld;
    private bool _isMoving;
    private bool _jumpReq;
    private bool _isStunned;
    private bool _canTeleport = true;
    private float _teleportTimer;
    private Vector3 _velocity;
    private float _verticalVel;
    private int _lastSkinIndex;

    // local hit cooldown
    private bool _hitRequested;
    private float _hitTimer;

    public override void Spawned()
    {
        _cc = GetComponent<CharacterController>();
        _cam = Camera.main;
        _velocity = Vector3.zero;
        _verticalVel = 0f;
        _mouseHeld = false;
        _isMoving = false;
        _isStunned = false;
        _hitRequested = false;
        _hitTimer = 0f;
        _lastSkinIndex = -1;

        Debug.Log($"[Spawn] Player spawned at {transform.position}");
        AssignInitialSkin();
    }

    void Update()
    {
        if (!HasInputAuthority) return;

        HandleMouseInput();
        HandleJumpInput();
        BufferHitRequest();
        PreviewDragRotation();
    }

    public override void FixedUpdateNetwork()
    {
        SyncSkin();
        ProcessTeleport();
        ProcessJumpAndGravity();
        ProcessMovement();
        UpdateHitTimer();         // reduce local timer
        ProcessBufferedHit();     // now uses _hitTimer
        ProcessRotationAfterMove();
    }

    // ────────────────────────────────────────────────────────────
    //   Initialization & Skin
    // ────────────────────────────────────────────────────────────
    private void AssignInitialSkin()
    {
        if (!HasStateAuthority) return;
        int idx = SkinSelection.instance.GetCurrentIndex();
        SkinIndex = Mathf.Clamp(idx, 0, SkinSelection.instance.skins.Count - 1);
        Debug.Log($"[Spawn] Assigned SkinIndex = {SkinIndex}");
    }

    private void SyncSkin()
    {
        if (SkinIndex == _lastSkinIndex) return;
        ApplySkin(SkinIndex);
        _lastSkinIndex = SkinIndex;
    }

    public void ApplySkin(int index)
    {
        foreach (Transform t in skinRoot) Destroy(t.gameObject);
        var go = Instantiate(SkinSelection.instance.skins[index], skinRoot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        Debug.Log($"[Skin] Applied skin #{index}");
    }

    // ────────────────────────────────────────────────────────────
    //   Input Handling
    // ────────────────────────────────────────────────────────────
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _mouseHeld = true;
            Debug.Log("[Input] Drag started");
        }

        if (Input.GetMouseButtonUp(0))
        {
            _mouseHeld = false;
            _isMoving = false;
            Debug.Log("[Input] Drag ended");
        }

        if (!_mouseHeld || _isStunned) return;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer))
        {
            Vector3 flat = hit.point - transform.position;
            flat.y = 0f;
            _clickDir = flat.sqrMagnitude > 0.001f ? flat.normalized : transform.forward;
            _isMoving = flat.magnitude > clickRadius;
        }
    }

    private void HandleJumpInput()
    {
        if (_cc.isGrounded && Input.GetButtonDown("Jump"))
        {
            _jumpReq = true;
            Debug.Log("[Input] Jump requested");
        }
    }

    private void BufferHitRequest()
    {
        if (Input.GetMouseButtonDown(1) && !_hitRequested)
        {
            _hitRequested = true;
            Debug.Log("[Input] Right-click detected");
        }
    }

    // ────────────────────────────────────────────────────────────
    //   Teleport
    // ────────────────────────────────────────────────────────────
    private void ProcessTeleport()
    {
        if (_mouseHeld && _isMoving && _canTeleport && Input.GetKeyDown(KeyCode.Q))
        {
            transform.position += _clickDir * teleportDistance;
            _canTeleport = false;
            _teleportTimer = teleportCooldown;
            Debug.Log($"[Teleport] Teleported to {transform.position}");
        }

        if (!_canTeleport)
        {
            _teleportTimer -= Runner.DeltaTime;
            if (_teleportTimer <= 0f)
            {
                _canTeleport = true;
                Debug.Log("[Teleport] Ready again");
            }
        }
    }

    // ────────────────────────────────────────────────────────────
    //   Jump & Gravity
    // ────────────────────────────────────────────────────────────
    private void ProcessJumpAndGravity()
    {
        if (_cc.isGrounded && _verticalVel < 0f)
            _verticalVel = 0f;

        if (_jumpReq)
        {
            _verticalVel = Mathf.Sqrt(2f * jumpHeight * -gravity);
            _jumpReq = false;
            Debug.Log($"[Jump] Jump applied, verticalVel={_verticalVel}");
        }

        _verticalVel += gravity * Runner.DeltaTime;
    }

    // ────────────────────────────────────────────────────────────
    //   Movement
    // ────────────────────────────────────────────────────────────
    private void ProcessMovement()
    {
        if (_isStunned) return;

        Vector3 target = _isMoving ? _clickDir * moveSpeed : Vector3.zero;
        float rate = _isMoving ? acceleration : deceleration;

        _velocity = Vector3.MoveTowards(_velocity, target, rate * Runner.DeltaTime);
        Vector3 move = _velocity;
        move.y = _verticalVel;
        _cc.Move(move * Runner.DeltaTime);
    }

    // ────────────────────────────────────────────────────────────
    //   Hit Cooldown
    // ────────────────────────────────────────────────────────────
    private void UpdateHitTimer()
    {
        if (_hitTimer > 0f)
            _hitTimer -= Runner.DeltaTime;
    }

    private void ProcessBufferedHit()
    {
        if (!_hitRequested) return;
        _hitRequested = false;

        if (_isStunned)
        {
            Debug.Log("[Hit] Aborted: player is stunned");
            return;
        }

        if (_hitTimer > 0f)
        {
            Debug.Log("[Hit] Aborted: on cooldown");
            return;
        }

        Vector3 dir = _clickDir.sqrMagnitude > 0.001f ? _clickDir : transform.forward;
        Vector3 origin = transform.position + transform.forward * 0.2f;

        Debug.DrawLine(origin, origin + dir * hitRange, Color.red, 1f);
        Debug.Log($"[Hit] SphereCast from {origin} toward {dir}");

        if (Physics.SphereCast(origin, hitRadius, dir, out var hit, hitRange, hitLayer))
        {
            Debug.Log($"[Hit] Spherecast HIT {hit.collider.name} at {hit.point}");
            if (hit.collider.TryGetComponent<PlayerController>(out var other))
            {
                Debug.Log($"[Hit] RPC_TakeHit -> {other.Object.InputAuthority}");
                other.RPC_TakeHit();
            }
        }
        else
        {
            Debug.Log("[Hit] Spherecast missed");
        }

        // start local cooldown
        _hitTimer = hitCooldown;
        Debug.Log($"[Hit] Cooldown started ({hitCooldown}s)");
    }

    // ────────────────────────────────────────────────────────────
    //   Rotation
    // ────────────────────────────────────────────────────────────
    private void PreviewDragRotation()
    {
        if (_mouseHeld && !_isStunned)
            RotateSkinTo(_clickDir);
    }

    private void ProcessRotationAfterMove()
    {
        if (_mouseHeld) return;
        if (_velocity.sqrMagnitude <= 0.001f) return;
        if (_isStunned) return;

        RotateSkinTo(_velocity.normalized);
    }

    private void RotateSkinTo(Vector3 dir)
    {
        if (skinRoot == null) return;
        Quaternion desired = Quaternion.LookRotation(dir, Vector3.up);
        skinRoot.rotation = Quaternion.RotateTowards(
            skinRoot.rotation,
            desired,
            rotationSpeed * Time.deltaTime
        );
    }

    // ────────────────────────────────────────────────────────────
    //   Stun RPC
    // ────────────────────────────────────────────────────────────
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    void RPC_TakeHit()
    {
        if (!Object.HasInputAuthority) return;
        ApplyLocalStun();
    }

    private void ApplyLocalStun()
    {
        if (_isStunned) return;
        _isStunned = true;
        _isMoving = false;
        Debug.Log("[Stun] Player stunned locally");
        StartCoroutine(RecoverFromStun());
    }

    IEnumerator RecoverFromStun()
    {
        yield return new WaitForSeconds(1f);
        _isStunned = false;
        Debug.Log("[Stun] Player recovered locally");
    }
}

// ────────────────────────────────────────────────────────────
// Extension to zero out Y component
// ────────────────────────────────────────────────────────────
static class Vec3Ext
{
    public static Vector3 WithY(this Vector3 v, float y) =>
        new Vector3(v.x, y, v.z);
}
