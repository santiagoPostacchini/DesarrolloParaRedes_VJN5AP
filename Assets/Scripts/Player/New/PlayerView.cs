using System.Collections;
using Fusion;
using UnityEngine;

namespace Player.New
{
    public class PlayerView : NetworkBehaviour
    {
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsJumping = Animator.StringToHash("IsJumping");
        private static readonly int Throw = Animator.StringToHash("Throw");
        [SerializeField] private ParticleSystem hitParticles;
        [SerializeField] private GameObject playerVisual;
        [Header("Bomb")]
        public Transform bombSlot;

        private NetworkMecanimAnimator _mecanimAnimator;
        private Animator _childAnim;

        public void FixedUpdate()
        {
            var lifeComponent = GetComponentInParent<LifeHandler>();
            var hitComponent = GetComponentInParent<HitHandler>();

            if (lifeComponent)
            {
                lifeComponent.OnDeadChanged += EnableMeshRender;
                lifeComponent.OnGetHit += TriggerGetHitParticles;
            }

            if (hitComponent)
            {
                hitComponent.OnTryHit += HitAnimation;
            }

            _mecanimAnimator = GetComponent<NetworkMecanimAnimator>();

            if (_mecanimAnimator)
            {
                var movementComponent = GetComponentInParent<NetworkCharacterControllerCustom>();

                if (movementComponent)
                {
                    movementComponent.OnMoving += MoveAnimation;
                }

                if (movementComponent)
                {
                    movementComponent.OnJump += JumpAnimationCoroutine;
                }
            }
        }

        void MoveAnimation(bool isRunning)
        {
            _mecanimAnimator.Animator.SetBool(IsRunning, isRunning);
        }

        void JumpAnimationCoroutine()
        {
            StartCoroutine(JumpAnimation());
        }

        IEnumerator JumpAnimation()
        {
            _mecanimAnimator.Animator.SetBool(IsJumping, true);

            yield return new WaitForSeconds(0.1f);

            _mecanimAnimator.Animator.SetBool(IsJumping, false);
        }

        void HitAnimation()
        {
            _mecanimAnimator.Animator.SetTrigger(Throw);
        }

        public void TriggerGetHitParticles()
        {
            hitParticles.Play();
        }

        void EnableMeshRender(bool e)
        {
            playerVisual.SetActive(!e);
        }
    }
}