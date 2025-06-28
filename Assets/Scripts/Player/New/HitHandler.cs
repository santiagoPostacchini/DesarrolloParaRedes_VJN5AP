using System;
using Fusion;
using UnityEngine;

namespace Player.New
{
    public class HitHandler : NetworkBehaviour
    {
        [SerializeField] private Transform bombSlotTransform;
    
        public event Action OnHit = delegate { };
    
        public void Hit()
        {
            if (!HasStateAuthority) return;

            RayHit();

            OnHit();
        }
    
        void RayHit()
        {
            /*
            if (_isStunned || _hitTimer > 0f) return;

            Vector3 dir = skinRoot.transform.forward;
            Vector3 origin = transform.position + skinRoot.transform.forward * 0.2f;

            if (Physics.SphereCast(origin, hitRadius, dir, out var hit, hitRange, hitLayer))
            {
                if (hit.collider.TryGetComponent<OldPlayerController>(out var other))
                {
                    other.RPC_TakeHit();
                    
                    var bomb = GameManager.Instance.GetCurrentBomb();
                    if (bomb)
                    {
                        if (bomb.OwnerRef == Object.InputAuthority)
                        {
                            bomb.RPC_RequestPassBomb(other.Object.InputAuthority);
                        }
                    }
                }
            }
            _hitTimer = hitCooldown;
            */
            Debug.DrawLine(transform.position, transform.position + transform.forward * 2, Color.magenta, 2);
        
            Runner.LagCompensation.Raycast(origin: transform.position, 
                direction: transform.forward, 
                length: 100, 
                player: Object.InputAuthority, 
                hit: out var hitInfo);

            if (!hitInfo.Hitbox) return;
            
            if (!hitInfo.Hitbox.transform.root.TryGetComponent(out LifeHandler player)) return;
            
            player.TakeHit();
        }
    }
}
