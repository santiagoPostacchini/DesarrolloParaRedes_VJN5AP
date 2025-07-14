using Fusion;
using Player.New.Inputs;
using UnityEngine;

namespace Player.New
{
    [RequireComponent(typeof(NetworkCharacterControllerCustom))]
    [RequireComponent(typeof(HitHandler))]
    public class PlayerController : NetworkBehaviour
    {
        private NetworkCharacterControllerCustom _cc;
        private HitHandler _hitHandler;
        
        [Header("Stun")]
        [SerializeField] private float stunDuration = 1.5f;
        
        [Networked] private TickTimer StunTimer { get; set; }
        private bool IsStunned => !StunTimer.ExpiredOrNotRunning(Runner);
        
        public override void Spawned()
        {
            _cc = GetComponent<NetworkCharacterControllerCustom>();
            _hitHandler = GetComponent<HitHandler>();
            
            if (!TryGetBehaviour(out LifeHandler lifeHandler)) return;

            lifeHandler.OnDeadChanged += b => { enabled = !b; };

            lifeHandler.OnRespawn += () => { _cc.Teleport(transform.position + Vector3.up * 3); };
            
            lifeHandler.OnGetHit += Stun;
        }

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out NetworkInputData inputs)) return;
            
            if(IsStunned) return;
            
            _cc.Move(inputs.Direction.normalized);
            
            if (inputs.IsJumpPressed && _cc.Grounded)
            {
                _cc.Jump();
            }
            
            if (inputs.IsHitPressed)
            {
                _hitHandler.Hit();
            }
        }

        private void Stun()
        {
            if (HasStateAuthority)
            {
                StunTimer = TickTimer.CreateFromSeconds(Runner, stunDuration);
            }
        }
    }
}