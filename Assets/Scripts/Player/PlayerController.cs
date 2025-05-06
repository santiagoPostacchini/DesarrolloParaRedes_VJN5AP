// PlayerController.cs
using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    [Header("References")]
    [Tooltip("Transform hijo que contiene la skin")]
    public Transform skinRoot;

    [Tooltip("Layer del piso")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 20f;
    public float clickRadius = 1f;

    [Header("Rotation")]
    public float rotationSpeed = 360f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Teleport")]
    public float teleportDistance = 5f;
    public float teleportCooldown = 20f;

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

    public override void Spawned()
    {
        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        _currentVel = Vector3.zero;
        _verticalVel = 0f;
        _mouseHeld = false;
        _isMoving = false;
    }

    void Update()
    {
        if (!HasInputAuthority) return;
        HandleMouseInput();
        HandleJumpInput();
        if (_mouseHeld) RotateSkin();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;
        HandleTeleport();
        ApplyJumpGravity();
        HandleMovement();
        HandleTeleportCooldown();
    }

    void HandleMouseInput()
    {
        Vector3 wp;
        if (Input.GetMouseButtonDown(0) && TryGetClick(out wp))
        {
            _mouseHeld = true;
            _clickPoint = wp;
            _clickDir = (wp - transform.position).WithY(0).normalized;
            _isMoving = Vector3.Distance(transform.position, wp) > clickRadius;
        }
        else if (_mouseHeld && Input.GetMouseButton(0) && TryGetClick(out wp))
        {
            _clickPoint = wp;
            _clickDir = (wp - transform.position).WithY(0).normalized;
            _isMoving = Vector3.Distance(transform.position, wp) > clickRadius;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _mouseHeld = false;
            _isMoving = false;
        }
    }

    bool TryGetClick(out Vector3 pt)
    {
        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var h, Mathf.Infinity, groundLayer))
        {
            pt = h.point;
            return true;
        }
        pt = Vector3.zero;
        return false;
    }

    void RotateSkin()
    {
        if (skinRoot == null) return;
        var target = new Vector3(_clickPoint.x, skinRoot.position.y, _clickPoint.z);
        var desired = Quaternion.LookRotation(target - skinRoot.position);
        skinRoot.rotation = Quaternion.RotateTowards(
            skinRoot.rotation,
            desired,
            rotationSpeed * Time.deltaTime
        );
    }

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

    void HandleMovement()
    {
        var targetVel = _isMoving ? _clickDir * moveSpeed : Vector3.zero;
        var rate = _isMoving ? acceleration : deceleration;
        _currentVel = Vector3.MoveTowards(
            _currentVel,
            targetVel,
            rate * Runner.DeltaTime
        );
        var m = _currentVel;
        m.y = _verticalVel;
        _controller.Move(m * Runner.DeltaTime);
    }

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
            if (_teleportTimer <= 0f) _canTeleport = true;
        }
    }
}

static class Vec3Ext
{
    public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
}
