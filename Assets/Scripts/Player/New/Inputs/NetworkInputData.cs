using Fusion;
using UnityEngine;

namespace Player.New.Inputs
{
    public struct NetworkInputData : INetworkInput
    {
        public NetworkBool IsHitPressed;
        public NetworkBool IsMovePressed;
        public Vector2 MouseScreenPosition; 

        public NetworkButtons NetworkButtons;
    }

    enum MyButtons
    {
        Jump = 0,
        Hit = 1
    }
}