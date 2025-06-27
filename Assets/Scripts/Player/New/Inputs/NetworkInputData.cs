using Fusion;

namespace Player.New.Inputs
{
    public struct NetworkInputData : INetworkInput
    {
        public float MovementInput;
        public NetworkBool IsHitPressed;

        public NetworkButtons NetworkButtons;
    }

    enum MyButtons
    {
        Jump = 0,
    }
}