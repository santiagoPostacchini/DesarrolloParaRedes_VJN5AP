using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    [Header("Skin")]
    public Transform skinRoot;

    [Header("Bomb")]
    public Transform bombSlot;

    public ParticleSystem stunStars;

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

    [SerializeField] NetworkMecanimAnimator _netAnim;
    private Animator _childAnim;

    [Networked] public int SkinIndex { get; set; }
    [Networked] public bool NetIsRunning { get; set; }
    [Networked] public bool NetIsGrounded { get; set; }
    [Networked] public Quaternion NetRotation { get; set; }

    private CharacterController _cc;
    private Camera _cam;
    private Vector3 _clickDir;
    private bool _mouseHeld, _isMoving, _jumpReq, _isStunned;
    private bool _canTeleport = true;
    private float _teleportTimer;
    private Vector3 _velocity;
    private float _verticalVel;
    private int _lastSkinIndex = -1;
    private bool _hitRequested;
    private float _hitTimer;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _cam = Camera.main;
    }

    public override void Spawned()
    {
        _velocity = Vector3.zero;
        _verticalVel = 0f;
        _mouseHeld = _isMoving = _isStunned = _hitRequested = false;
        _hitTimer = 0f;

        if (HasInputAuthority)
            SkinIndex = SkinSelection.instance.GetCurrentIndex();

        _lastSkinIndex = SkinIndex;
        _childAnim = GetComponentInChildren<Animator>();
        _netAnim.Animator = _childAnim;
        _netAnim.Animator.Rebind();

        GameManager.Instance.AddToList(this);
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
        if (!HasInputAuthority)
            transform.rotation = Quaternion.Slerp(transform.rotation, NetRotation, Runner.DeltaTime * 10f);

        if (_lastSkinIndex != SkinIndex)
            _lastSkinIndex = SkinIndex;

        ProcessTeleport();
        ProcessJumpAndGravity();
        ProcessMovement();
        UpdateHitTimer();
        ProcessBufferedHit();
        ProcessRotationAfterMove();
        UpdateAnimations();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
            _mouseHeld = true;

        if (Input.GetMouseButtonUp(0))
        {
            _mouseHeld = false;
            _isMoving = false;
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
            _netAnim?.SetTrigger("Jump");
        }
    }

    private void BufferHitRequest()
    {
        if (Input.GetMouseButtonDown(1) && !_hitRequested)
            _hitRequested = true;
    }

    private void ProcessTeleport()
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
            if (_teleportTimer <= 0f)
                _canTeleport = true;
        }
    }

    private void ProcessJumpAndGravity()
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

    private void ProcessMovement()
    {
        if (_isStunned) return;

        _netAnim?.Animator.SetBool("isRunning", _isMoving);
        _netAnim?.Animator.SetBool("isGrounded", _cc.isGrounded);

        Vector3 target = _isMoving ? _clickDir * moveSpeed : Vector3.zero;
        float rate = _isMoving ? acceleration : deceleration;
        _velocity = Vector3.MoveTowards(_velocity, target, rate * Runner.DeltaTime);

        Vector3 move = _velocity;
        move.y = _verticalVel;
        _cc.Move(move * Runner.DeltaTime);
    }

    private void UpdateHitTimer()
    {
        if (_hitTimer > 0f)
            _hitTimer -= Runner.DeltaTime;
    }

    private void ProcessBufferedHit()
    {
        if (!_hitRequested) return;
        _hitRequested = false;

        _netAnim?.SetTrigger("Throw");

        if (_isStunned || _hitTimer > 0f) return;

        Vector3 dir = skinRoot.transform.forward;
        Vector3 origin = transform.position + skinRoot.transform.forward * 0.2f;

        if (Physics.SphereCast(origin, hitRadius, dir, out var hit, hitRange, hitLayer))
        {
            if (hit.collider.TryGetComponent<PlayerController>(out var other))
            {
                other.RPC_TakeHit();

                // --- PASAR BOMBA ---
                var bomb = GameManager.Instance.GetCurrentBomb();
                if (bomb != null)
                {
                    // SOLO si yo tengo la bomba y soy host (StateAuthority)
                    if (bomb.OwnerRef == Object.InputAuthority)
                    {
                        bomb.RPC_RequestPassBomb(other.Object.InputAuthority);
                    }
                }
            }
        }
        _hitTimer = hitCooldown;
    }

    private void PreviewDragRotation()
    {
        if (_mouseHeld && !_isStunned)
            RotateTransformTo(_clickDir);

        UpdateSkinRootRotation();
    }

    private void ProcessRotationAfterMove()
    {
        if (_velocity.sqrMagnitude <= 0.001f || _isStunned) return;
        RotateTransformTo(_velocity.normalized);
    }

    private void RotateTransformTo(Vector3 dir)
    {
        if (dir == Vector3.zero)
            return;

        Quaternion desired = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desired,
            rotationSpeed * Time.deltaTime
        );

        if (HasInputAuthority)
            NetRotation = transform.rotation;
    }

    private void UpdateSkinRootRotation()
    {
        if (_clickDir == Vector3.zero)
            return;

        Quaternion look = Quaternion.LookRotation(_clickDir, Vector3.up);
        skinRoot.rotation = look;
    }

    // CAMBIO: Stun sincronizado a todos
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_TakeHit()
    {
        ApplyLocalStun();
    }

    [Rpc(RpcSources.All, RpcTargets.InputAuthority)]
    public void RpcEliminated()
    {
        UIController.Instance.ShowEliminated();

        StartCoroutine(DelayedDisable());
    }

    private IEnumerator DelayedDisable()
    {
        yield return null;
        gameObject.SetActive(false);
    }


    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PlayStunEffect()
    {
        if (stunStars != null)
            stunStars.Play();
    }

    private void ApplyLocalStun()
    {
        if (_isStunned) return;
        _isStunned = true;
        _isMoving = false;
        RPC_PlayStunEffect();
        StartCoroutine(RecoverFromStun());
    }

    private IEnumerator RecoverFromStun()
    {
        yield return new WaitForSeconds(1f);
        _isStunned = false;
    }

    private void UpdateAnimations()
    {
        if (IsProxy) return;

        NetIsRunning = _isMoving;
        NetIsGrounded = _cc.isGrounded;

        if (_jumpReq)
            _netAnim?.SetTrigger("Jump");
    }
}
