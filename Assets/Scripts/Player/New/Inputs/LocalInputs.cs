using UnityEngine;

namespace Player.New.Inputs
{
    public class LocalInputs : MonoBehaviour
    {
        private NetworkInputData _networkInputData;

        private bool _isJumpPressed;
        private bool _isHitPressed;
        private bool _isMovePressed;
        private Vector2 _mouseScreenPosition;
    
        void Start()
        {
            _networkInputData = new NetworkInputData();
        }

        void Update()
        {
            _mouseScreenPosition = Input.mousePosition;
            
            _isJumpPressed |= Input.GetKeyDown(KeyCode.Space);
            
            _isMovePressed = Input.GetMouseButton(0);
            
            _isHitPressed |= Input.GetMouseButtonDown(1);
        }

        public NetworkInputData GetLocalInputs()
        {
            _networkInputData.IsMovePressed = _isMovePressed;
            
            _networkInputData.NetworkButtons.Set(MyButtons.Jump, _isJumpPressed);
            _isJumpPressed = false;
            
            _networkInputData.NetworkButtons.Set(MyButtons.Hit, _isHitPressed);
            _isHitPressed = false;
            
            _networkInputData.MouseScreenPosition = _mouseScreenPosition;
            
            return _networkInputData;
        }
    }
}