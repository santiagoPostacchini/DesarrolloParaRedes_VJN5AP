using System;
using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    [Header("Skin")]
    public Transform skinRoot;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float clickRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Teleport")]
    [SerializeField] private float teleportDistance = 5f;
    [SerializeField] private float teleportCooldown = 20f;

    [Header("Melee Hit")]
    [SerializeField] private float hitRadius = 0.5f;
    [SerializeField] private float hitRange = 0.3f;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private float hitCooldown = 1.0f;

    public int SkinIndex { get; set; }
    private int _lastSkin = -1;

    [Networked, OnChangedRender(nameof(OnStunChanged))]
    private TickTimer stunTimer { get; set; }
    private int _lastStunTick = -1;

    // ——— Internals ——————————————————————————————————————————
    CharacterController _cc;
    Camera _cam;
    NetworkMecanimAnimator _netAnim;

    Vector3 _clickDir;
    Vector3 _velocity;
    float _verticalVel;

    bool _mouseHeld, _isMoving, _jumpReq, _isStunned, _hitRequested, _canTeleport = true;
    float _hitTimer, _teleportTimer;

    public override void Spawned()
    {
        _cc = GetComponent<CharacterController>();
        _cam = Camera.main;
        _netAnim = GetComponentInChildren<NetworkMecanimAnimator>();

        _velocity = Vector3.zero;
        _verticalVel = 0f;
        _hitTimer = _teleportTimer = 0f;

        stunTimer = TickTimer.None;
        _isStunned = false;

        RPC_SetSkin();
    }

    void Update()
    {
        if (!HasInputAuthority) return;
        ReadInput();

        if (_mouseHeld && !_isStunned)
            RotateTowards(_clickDir);

        UpdateAnimationFlags();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority)
        {
            if (_velocity.sqrMagnitude > 0.001f && !_isStunned)
                RotateTowards(_velocity.normalized);
            return;
        }

        ProcessTeleport();
        ProcessJumpGravity();
        ProcessMovement();

        if (_hitRequested)
            ProcessHit();

        ProcessRotationAfterMove();
    }

    static void OnStunChanged(PlayerController c)
    {
        if (!c.stunTimer.Expired(c.Runner))
            c.StartCoroutine(c.StunCoroutine());
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetSkin()
    {
        foreach (Transform t in skinRoot) Destroy(t.gameObject);
        var go = Instantiate(SkinSelection.instance.skins[SkinIndex], skinRoot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
    }

    IEnumerator StunCoroutine()
    {
        _isStunned = true;
        _isMoving = false;
        _netAnim.Animator?.SetTrigger("Stunned");
        yield return new WaitForSeconds(hitCooldown);
        _isStunned = false;
        _netAnim.Animator?.SetTrigger("Recover");
    }

    // ——— Input Phase ———————————————————————————————————————
    void ReadInput()
    {
        if (Input.GetMouseButtonDown(0)) _mouseHeld = true;
        if (Input.GetMouseButtonUp(0)) { _mouseHeld = _isMoving = false; }

        // dragging → world hit
        if (_mouseHeld && !_isStunned)
        {
            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var h, Mathf.Infinity, groundLayer))
            {
                var flat = h.point - transform.position;
                flat.y = 0;
                _clickDir = (flat.sqrMagnitude > 0.001f) ? flat.normalized : transform.forward;
                _isMoving = flat.magnitude > clickRadius;
            }
        }

        if (_cc.isGrounded && Input.GetButtonDown("Jump"))
        {
            _jumpReq = true;
            _netAnim.Animator?.SetTrigger("Jump");
        }

        if (Input.GetMouseButtonDown(1))
        {
            _hitRequested = true;
            _netAnim.Animator?.SetTrigger("Throw");
        }
    }

    // ——— Physics Phase —————————————————————————————————————
    void ProcessMovement()
    {
        if (_isStunned) return;
        var target = _isMoving ? _clickDir * moveSpeed : Vector3.zero;
        var rate = _isMoving ? acceleration : deceleration;
        _velocity = Vector3.MoveTowards(_velocity, target, rate * Runner.DeltaTime);

        var m = _velocity; m.y = _verticalVel;
        _cc.Move(m * Runner.DeltaTime);
    }

    void ProcessJumpGravity()
    {
        if (_cc.isGrounded && _verticalVel < 0f)
            _verticalVel = 0f;

        if (_jumpReq)
        {
            _verticalVel = Mathf.Sqrt(2f * jumpHeight * -gravity);
            _jumpReq = false;
        }

        _verticalVel += gravity * Runner.DeltaTime;
    }

    void ProcessTeleport()
    {
        if (_mouseHeld && _isMoving && _canTeleport && Input.GetKeyDown(KeyCode.Q))
        {
            transform.position += _clickDir * teleportDistance;
            _canTeleport = false;
            _teleportTimer = teleportCooldown;
        }

        if (!_canTeleport)
        {
            _teleportTimer -= Runner.DeltaTime;
            if (_teleportTimer <= 0f) _canTeleport = true;
        }
    }

    void ProcessHit()
    {
        _hitRequested = false;
        if (_hitTimer > 0f || _isStunned) return;

        Vector3 origin = transform.position + transform.forward * 0.2f + Vector3.up * 0.5f;
        Debug.DrawLine(origin, origin + transform.forward * hitRange, Color.red, 1f);

        if (Physics.SphereCast(origin, hitRadius, transform.forward, out var h, hitRange, hitLayer)
            && h.collider.TryGetComponent<PlayerController>(out var other))
        {
            other.RPC_RequestStun();
            _hitTimer = hitCooldown;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_RequestStun()
    {
        if (HasStateAuthority)
            stunTimer = TickTimer.CreateFromSeconds(Runner, hitCooldown);
    }

    void ProcessRotationAfterMove()
    {
        if (_mouseHeld) return;
        if (_velocity.sqrMagnitude <= 0.001f || _isStunned) return;
        RotateTowards(_velocity.normalized);
    }

    void RotateTowards(Vector3 dir)
    {
        var desired = Quaternion.LookRotation(dir, Vector3.up);
        skinRoot.rotation = Quaternion.RotateTowards(skinRoot.rotation, desired, rotationSpeed * Time.deltaTime);
    }

    void UpdateAnimationFlags()
    {
        if (_netAnim == null) return;
        _netAnim.Animator?.SetBool("isRunning", _isMoving);
        _netAnim.Animator?.SetBool("isGrounded", _cc.isGrounded);
    }

}
