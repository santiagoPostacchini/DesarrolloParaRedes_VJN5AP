using System;
using Fusion;
using Player.New;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bomb
{
    public class BombBehaviour : NetworkBehaviour
    {
        [Networked] public PlayerRef Holder { get; private set; }
        [Networked] private TickTimer FuseTimer { get; set; }
        
        [Header("Explosion Effect")]
        [SerializeField] private GameObject explosionEffect;
        
        public event Action<PlayerRef> OnExplode;

        private float _fuseTime;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                _fuseTime = Random.Range(10f, 20);
                FuseTimer = TickTimer.CreateFromSeconds(Runner, _fuseTime);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            if (FuseTimer.Expired(Runner))
            {
                TriggerExplode();
            }
        }

        public void PickUp(PlayerRef holder)
        {
            Debug.Log($"[Bomb] Picked up by {holder}");
            Holder = holder;

            var playerObj = Runner.GetPlayerObject(holder);
            if (playerObj && playerObj.GetComponentInChildren<PlayerView>().TryGetComponent(out PlayerView view))
            {
                transform.SetParent(view.bombSlot);
                transform.localPosition = Vector3.zero;
        
                Debug.Log("[Bomb] Attached in hierarchy, NetworkTransform will sync parent next tick");
            }
        }

        private void TriggerExplode()
        {
            RPC_TriggerExplosionEffect();
            
            OnExplode?.Invoke(Holder);
            FuseTimer = TickTimer.None;
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_TriggerExplosionEffect()
        {
            if (explosionEffect && explosionEffect.GetComponentsInChildren<ParticleSystem>().Length > 0)
            {
                var effects = explosionEffect.GetComponentsInChildren<ParticleSystem>();
                foreach (var effect in effects)
                {
                    effect.Play();
                }
            }
        }

    }
}