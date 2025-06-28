using Fusion;
using Player.New;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [SerializeField] private ParticleSystem _hitParticles;
    [SerializeField] private GameObject _playerVisual;
    
    private NetworkMecanimAnimator _mecanimAnimator;
    private Animator _childAnim;
    
    [Networked, OnChangedRender(nameof(TriggerHitParticles))] private NetworkBool Hit { get; set; }
    
    public override void Spawned()
    {
        var hitComponent = GetComponentInParent<HitHandler>();

        if (hitComponent)
        {
            hitComponent.OnHit += ()=> Hit = !Hit;
        }
        
        var lifeComponent = GetComponentInParent<LifeHandler>();

        if (lifeComponent)
        {
            lifeComponent.OnDeadChanged += EnableMeshRender;
        }
        
        _mecanimAnimator = GetComponent<NetworkMecanimAnimator>();

        if (_mecanimAnimator)
        {
            var movementComponent = GetComponentInParent<NetworkCharacterControllerCustom>();

            if (movementComponent)
            {
                movementComponent.OnMoving += MoveAnimation;
            }
        }
    }
    
    void MoveAnimation(bool isRunning)
    {
        _mecanimAnimator.Animator.SetBool("isRunning", isRunning);
    }
    
    void TriggerHitParticles()
    {
        _hitParticles.Play();
    }
    
    void EnableMeshRender(bool e)
    {
        _playerVisual.SetActive(!e);
    }
}
