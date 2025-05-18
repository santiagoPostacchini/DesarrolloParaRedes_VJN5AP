using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class Bomb : NetworkBehaviour
{
    [SerializeField] private ParticleSystem explosionVFX;
    private float timeToExplode;
    private bool exploded = false;

    [Networked] public TickTimer ExplodeTimer { get; private set; }
    [Networked] public bool IsActive { get; private set; }
    [Networked] public PlayerRef OwnerRef { get; set; }

    public override void FixedUpdateNetwork()
    {
        // Seguir el bombSlot del OwnerRef
        if (GameManager.Instance.TryGetPlayer(OwnerRef, out var pc) && pc.bombSlot != null)
        {
            transform.position = pc.bombSlot.position;
            transform.rotation = pc.bombSlot.rotation;
        }

        // Explosión SOLO por el host
        if (IsActive && ExplodeTimer.Expired(Runner) && !exploded)
        {
            if (Object.HasStateAuthority)
            {
                exploded = true;
                Debug.Log($"[Bomb] Explode! OwnerRef: {OwnerRef}, InputAuthority: {Object.InputAuthority}");
                RPC_Explode();
            }
        }
    }

    public void ActivateBomb(PlayerRef owner)
    {
        exploded = false;
        OwnerRef = owner;
        if (Object.HasStateAuthority)
            Object.AssignInputAuthority(owner); // El host siempre asigna autoridad
        timeToExplode = Random.Range(8f, 12f); // Tiempo corto para debug
        ExplodeTimer = TickTimer.CreateFromSeconds(Runner, timeToExplode);
        IsActive = true;
        Debug.Log($"[Bomb] ACTIVATED! OwnerRef: {owner}, InputAuthority: {Object.InputAuthority}");
    }

    public void DeactivateBomb()
    {
        IsActive = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Explode()
    {
        Debug.Log($"[Bomb] EXPLODE! OwnerRef: {OwnerRef}, InputAuthority: {Object.InputAuthority}");
        GameManager.Instance.OnBombExploded(this);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestPassBomb(PlayerRef target)
    {
        // Solo el host (StateAuthority) y SOLO si OwnerRef == quien lo pide
        if (OwnerRef == Object.InputAuthority && Object.HasStateAuthority)
        {
            Debug.Log($"[Bomb] Host transfiere bomba de {OwnerRef} a {target}");
            RPC_Reassign(target);
        }
        else
        {
            Debug.LogWarning($"[Bomb] RPC_RequestPassBomb ignorado. OwnerRef: {OwnerRef}, InputAuthority: {Object.InputAuthority}, StateAuthority: {Object.HasStateAuthority}");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Reassign(PlayerRef newOwner)
    {
        DeactivateBomb();

        if (Object.HasStateAuthority)
        {
            OwnerRef = newOwner;
            Object.AssignInputAuthority(newOwner);
            Debug.Log($"[Bomb] Reassigned: Ahora OwnerRef/InputAuthority es {newOwner}");
        }

        // Mover visual para todos
        if (GameManager.Instance.TryGetPlayerController(newOwner, out var nextPc) && nextPc.bombSlot != null)
        {
            transform.position = nextPc.bombSlot.position;
            transform.rotation = nextPc.bombSlot.rotation;
        }

        // Reactiva SOLO si soy el nuevo dueño
        if (Object.InputAuthority == newOwner)
        {
            ActivateBomb(newOwner);
        }
    }
}
