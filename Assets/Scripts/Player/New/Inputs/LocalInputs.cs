using UnityEngine;

namespace Player.New.Inputs
{
    public class LocalInputs : MonoBehaviour
    {
        private NetworkInputData _networkInputData;

        private bool _isJumpPressed;
        private bool _isFirePressed;
    
        void Start()
        {
            _networkInputData = new NetworkInputData();
        }

        void Update()
        {
            _networkInputData.MovementInput = Input.GetAxis("Horizontal");

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isFirePressed = true;
            }

            _isFirePressed |= Input.GetKeyDown(KeyCode.Space);
        
            _isJumpPressed |= Input.GetKeyDown(KeyCode.W);
        }

        public NetworkInputData GetLocalInputs()
        {
            _networkInputData.IsHitPressed = _isFirePressed;
            _isFirePressed = false;

            _networkInputData.NetworkButtons.Set(MyButtons.Jump, _isJumpPressed);
            _isJumpPressed = false;
        
            return _networkInputData;
        }
    }
}