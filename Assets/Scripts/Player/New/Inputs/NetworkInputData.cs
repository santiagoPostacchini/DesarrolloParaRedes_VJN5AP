using Fusion;
using UnityEngine;

namespace Player.New.Inputs
{
    public struct NetworkInputData : INetworkInput
    {
        public const byte MOUSEBUTTON0 = 1;
        public const byte SPACE = 2;
        
        public NetworkButtons Buttons;
        public Vector3 Direction;
        
        public bool IsHitPressed => Buttons.IsSet(MOUSEBUTTON0);
        public bool IsJumpPressed => Buttons.IsSet(SPACE);
    }
}