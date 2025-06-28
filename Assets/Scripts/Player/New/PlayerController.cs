using Fusion;
using Player.New.Inputs;
using UnityEngine;

namespace Player.New
{
    [RequireComponent(typeof(NetworkCharacterControllerCustom))]
    [RequireComponent(typeof(HitHandler))]
    public class PlayerController : NetworkBehaviour
    {
        private NetworkCharacterControllerCustom _characterMovement;
        private HitHandler _hitHandler;
        
        [Header("Layers")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private new Camera camera;
    
        public override void Spawned()
        {
            _characterMovement = GetComponent<NetworkCharacterControllerCustom>();
            _hitHandler = GetComponent<HitHandler>();
            camera = Camera.main;
            
            if (!TryGetBehaviour(out LifeHandler lifeHandler)) return;
        
            lifeHandler.OnDeadChanged += b =>
            {
                enabled = !b;
            };

            lifeHandler.OnRespawn += () =>
            {
                _characterMovement.Teleport(transform.position + Vector3.up * 3);
            };
        }

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out NetworkInputData inputs)) return;
        
            _characterMovement.Move(GetMovementDirection(inputs));
            
            if (inputs.NetworkButtons.IsSet(MyButtons.Jump))
            {
                _characterMovement.Jump();
            }
        
            if (inputs.IsHitPressed)
            {
                _hitHandler.Hit();
            }
        }
    
        Vector3 GetMovementDirection(NetworkInputData input)
        {
            Vector3 direction = Vector3.zero;
            if (input.IsMovePressed)
            {
                Ray ray = camera.ScreenPointToRay(input.MouseScreenPosition);
                if (Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer))
                {
                    Vector3 flat = hit.point - transform.position;
                    flat.y = 0f;
                    direction = flat.sqrMagnitude > 0.001f ? flat.normalized : transform.forward;
                }
            }
            
            if(direction != Vector3.zero)
                Debug.Log($"Player {Object.InputAuthority}: Direction = {direction}");
            
            return direction;
        }
    }
}
