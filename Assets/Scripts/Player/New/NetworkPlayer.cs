using System;
using Fusion;
using Nicknames;
using Player.New.Inputs;
using UnityEngine;

namespace Player.New
{
    [RequireComponent(typeof(LocalInputs))]
    public class NetworkPlayer : NetworkBehaviour
    {
        public static NetworkPlayer Local { get; private set; }
        public LocalInputs LocalInputs { get; private set; }

        private NicknameItem _myNickname;
    
        [Networked]
        private NetworkString<_16> Nickname { get; set; }

        private ChangeDetector _changeDetector;
    
        public event Action OnLeft = delegate { };

        public override void Spawned()
        {
            LocalInputs = GetComponent<LocalInputs>();

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        
            _myNickname = NicknameHandler.Instance.AddNickname(this);
        
            if (Object.HasInputAuthority)
            {
                Local = this;
                LocalInputs.enabled = true;

                NetworkString<_16> loadedNick;

                if (PlayerPrefs.HasKey("Nickname"))
                {
                    loadedNick = PlayerPrefs.GetString("Nickname");
                }
                else
                {
                    loadedNick = $"Player {Runner.LocalPlayer.PlayerId}";
                }
            
                //loadedNick = PlayerPrefs.HasKey("Nickname") ? PlayerPrefs.GetString("Nickname") : "Player";

                RPC_SetNickname(loadedNick);
            }
            else
            {
                LocalInputs.enabled = false;
                UpdateNickname();
            }
        }

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(Nickname):
                        UpdateNickname();
                        break;
                }
            }
        }
    
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        void RPC_SetNickname(NetworkString<_16> newNickname)
        {
            Nickname = newNickname;
        }

        void UpdateNickname()
        {
            _myNickname.UpdateText(Nickname.Value);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            OnLeft();
        }
    }
}
