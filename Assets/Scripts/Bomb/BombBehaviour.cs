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
            if (playerObj && playerObj.TryGetComponent(out PlayerView view))
            {
                transform.SetParent(view.bombSlot);
                transform.localPosition = Vector3.zero;
        
                Debug.Log("[Bomb] Attached in hierarchy, NetworkTransform will sync parent next tick");
            }
        }

        private void TriggerExplode()
        {
            OnExplode?.Invoke(Holder);
            FuseTimer = TickTimer.None;
        }
    }
}