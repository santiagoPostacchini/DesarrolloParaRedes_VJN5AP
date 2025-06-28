using System;
using Fusion;
using UnityEngine;

namespace Player.New
{
    public class NetworkCharacterControllerCustom : NetworkCharacterController
    {
        public event Action<bool> OnMoving = delegate { };
        public event Action OnJump = delegate { };

        public override void Move(Vector3 direction)
        {
            Vector3 target = direction * maxSpeed;
            
            if (direction.sqrMagnitude > 0)
            {
                Velocity = Vector3.MoveTowards(Velocity, target, acceleration * Runner.DeltaTime);
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
            else
            {
                Velocity = Vector3.MoveTowards(Velocity, Vector3.zero, braking * Runner.DeltaTime);
            }
            
            Vector3 move = Velocity;
            move.y = Velocity.y;
            
            Controller.Move(move * Runner.DeltaTime);
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
