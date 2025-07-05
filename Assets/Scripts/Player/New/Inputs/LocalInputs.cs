using UnityEngine;

namespace Player.New.Inputs
{
    public class LocalInputs : MonoBehaviour
    {
        private NetworkInputData _data;

        private bool _mouseButton0;
        private bool _space;
        private Vector3 _direction;
    
        void Start()
        {
            _data = new NetworkInputData();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
                _direction += Vector3.forward;

            if (Input.GetKeyDown(KeyCode.S))
                _direction += Vector3.back;

            if (Input.GetKeyDown(KeyCode.A))
                _direction += Vector3.left;

            if (Input.GetKeyDown(KeyCode.D))
                _direction += Vector3.right;
            
            if (Input.GetKeyUp(KeyCode.W))
                _direction -= Vector3.forward;

            if (Input.GetKeyUp(KeyCode.S))
                _direction -= Vector3.back;

            if (Input.GetKeyUp(KeyCode.A))
                _direction -= Vector3.left;

            if (Input.GetKeyUp(KeyCode.D))
                _direction -= Vector3.right;
            
            _mouseButton0 = _mouseButton0 || Input.GetMouseButtonDown(0);
            
            _space = _space || Input.GetKeyDown(KeyCode.Space);
        }

        public NetworkInputData GetLocalInputs()
        {
            _data.Buttons.Set(NetworkInputData.MOUSEBUTTON0, _mouseButton0);
            _mouseButton0 = false;

            _data.Buttons.Set(NetworkInputData.SPACE, _space);
            _space = false;
            
            _data.direction = _direction;
            
            return _data;
        }
    }
}