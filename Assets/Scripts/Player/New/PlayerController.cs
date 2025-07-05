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

        [Header("Layers")] [SerializeField] private LayerMask groundLayer;
        
        public override void Spawned()
        {
            _cc = GetComponent<NetworkCharacterControllerCustom>();
            _hitHandler = GetComponent<HitHandler>();
            
            if (!TryGetBehaviour(out LifeHandler lifeHandler)) return;

            lifeHandler.OnDeadChanged += b => { enabled = !b; };

            lifeHandler.OnRespawn += () => { _cc.Teleport(transform.position + Vector3.up * 3); };
        }

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out NetworkInputData inputs)) return;

            _cc.Move(inputs.direction.normalized);

            if (inputs.IsJumpPressed && _cc.Grounded)
            {
                _cc.Jump();
            }
            
            if (inputs.IsHitPressed)
            {
                _hitHandler.Hit();
            }
        }
    }
}