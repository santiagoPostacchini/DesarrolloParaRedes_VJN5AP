using System;
using Fusion;
using UnityEngine;

namespace Player.New
{
    public class LifeHandler : NetworkBehaviour
    {
        [Networked, OnChangedRender(nameof(DeadStateChanged))] 
        private NetworkBool IsDead { get; set; }
    
        public event Action<bool> OnDeadChanged = delegate { };
        public event Action OnRespawn = delegate { };
        public event Action OnLeft = delegate {  };
        public event Action OnGetHit = delegate { };

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                IsDead = false;
            }
        }

        public void TakeHit()
        {
            OnGetHit();
        }

        public void Server_Resurrect()
        {
            OnRespawn();
            IsDead = false;
        }

        void DeadStateChanged()
        {
            GetComponentInParent<HitboxRoot>().HitboxRootActive = !IsDead;
        
            OnDeadChanged(IsDead);
        }
    
        public void DisconnectPlayer()
        {
            if (!Object.HasInputAuthority)
            {
                Runner.Disconnect(Object.InputAuthority);
            }
        
            Runner.Despawn(Object);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            OnLeft();
        }
    }
}