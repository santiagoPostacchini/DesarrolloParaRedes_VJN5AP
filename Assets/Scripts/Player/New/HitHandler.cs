using System;
using Bomb;
using Fusion;
using UnityEngine;

namespace Player.New
{
    public class HitHandler : NetworkBehaviour
    {
        [SerializeField] private float hitCooldown = 1f;
        [SerializeField] private float hitRange = 0.8f;
        [SerializeField] private int raycastCount = 6;
        [SerializeField] private float raycastAngle = 180f;
        [SerializeField] private LayerMask hitboxLayer;

        [Networked] private TickTimer HitCooldownTimer { get; set; }

        public event Action OnTryHit = delegate { };

        public void Hit()
        {
            if (!HasStateAuthority || !HitCooldownTimer.ExpiredOrNotRunning(Runner)) return;

            PerformHit();

            HitCooldownTimer = TickTimer.CreateFromSeconds(Runner, hitCooldown);

            OnTryHit();
        }

        void PerformHit()
        {
            float angleStep = raycastAngle / (raycastCount - 1);
            float startAngle = -raycastAngle / 2;

            for (int i = 0; i < raycastCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
                Vector3 direction = rotation * transform.forward;

                Debug.DrawRay(transform.position, direction * hitRange, Color.yellow, 1);

                if (Runner.LagCompensation.Raycast(transform.position, direction, hitRange, Object.InputAuthority, out var hit, hitboxLayer, HitOptions.IgnoreInputAuthority | HitOptions.SubtickAccuracy))
                {
                    if (hit.Hitbox && hit.Hitbox.Root.TryGetComponent(out LifeHandler player))
                    {
                        player.TakeHit();
                        RPC_TriggerHitEffects(player.Object);
                        var attacker = Object.InputAuthority;
                        var victim = hit.Hitbox.Root.GetComponent<NetworkObject>().InputAuthority;

                        var bm = FindObjectOfType<Bomb.GameManager>();
                        if (bm && bm.HasBomb(attacker) && attacker != victim)
                        {
                            bm.TransferBomb(victim);
                        }
                        break;
                    }
                }
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_TriggerHitEffects(NetworkObject playerHit)
        {
            if (playerHit.TryGetComponent(out PlayerView view))
            {
                view.TriggerGetHitParticles();
            }
        }
    }
}