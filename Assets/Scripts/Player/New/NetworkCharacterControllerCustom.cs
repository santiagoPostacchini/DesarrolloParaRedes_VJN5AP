using System;
using Fusion;
using UnityEngine;

namespace Player.New
{
    public class NetworkCharacterControllerCustom : NetworkCharacterController
    {
        public event Action<bool> OnMoving = delegate { };
        public event Action OnJump = delegate { };
        
        [SerializeField] private float maxVerticalVelocity = 8f;

        public override void Move(Vector3 direction)
        {
            var deltaTime    = Runner.DeltaTime;
            var previousPos  = transform.position;
            var moveVelocity = Velocity;

            direction = direction.normalized;

            if (Grounded && moveVelocity.y < 0) {
                moveVelocity.y = 0f;
            }

            moveVelocity.y += gravity * Runner.DeltaTime;
            
            moveVelocity.y = Mathf.Clamp(moveVelocity.y, -maxVerticalVelocity, maxVerticalVelocity);
            
            var horizontalVel = new Vector3(moveVelocity.x, 0f, moveVelocity.z);

            if (direction == default) 
            {
                horizontalVel = Vector3.Lerp(horizontalVel, Vector3.zero, braking * deltaTime);
            } 
            else 
            {
                Vector3 horizontalDirection = new Vector3(direction.x, 0f, direction.z);
                horizontalVel = Vector3.ClampMagnitude(horizontalVel + horizontalDirection * acceleration * deltaTime, maxSpeed);
                
                if (horizontalDirection.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(horizontalDirection);
                }
            }   
            
            moveVelocity.x = horizontalVel.x;
            moveVelocity.z = horizontalVel.z;

            Controller.Move(moveVelocity * deltaTime);

            Velocity = (transform.position - previousPos) * Runner.TickRate;
            Grounded = Controller.isGrounded;
            
            OnMoving(direction.sqrMagnitude > 0);
        }
        
        public override void Jump(bool ignoreGrounded = false, float? overrideImpulse = null)
        {
            base.Jump(ignoreGrounded, overrideImpulse);
            OnJump();
        }
    }
}