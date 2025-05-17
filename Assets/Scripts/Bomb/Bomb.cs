using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class Bomb : NetworkBehaviour
{
    [SerializeField] private ParticleSystem explosionVFX;
    private float timeToExplode;

    [Networked] public TickTimer ExplodeTimer { get; private set; }

    // Nuevo: propiedad para saber si la bomba est� activa
    [Networked] public bool IsActive { get; private set; }

    public override void Spawned()
    {
        // Solo activa si hay due�o y est� en juego
        if (Object.HasStateAuthority)
        {
            ActivateBomb();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && IsActive && ExplodeTimer.Expired(Runner))
        {
            RPC_Explode();
        }
    }

    // M�todo para activar la bomba correctamente tras asignarla a un jugador
    public void ActivateBomb()
    {
        timeToExplode = Random.Range(15f, 25f);
        ExplodeTimer = TickTimer.CreateFromSeconds(Runner, timeToExplode);
        IsActive = true;
    }

    // M�todo para desactivar la bomba (opcional, por si quieres pausar)
    public void DeactivateBomb()
    {
        IsActive = false;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_Explode()
    {
        explosionVFX?.Play();

        var pc = GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            pc.RpcEliminated(); // Notifica la eliminaci�n al GameManager a trav�s del PlayerController
        }

        if (Object.HasStateAuthority)
        {
            DeactivateBomb(); // Desactiva la bomba hasta que sea reasignada

            // Llama al GameManager para procesar la explosi�n (reasignaci�n, fin de partida, etc.)
            GameManager.Instance.OnBombExploded(this);
        }
    }

    // Cuando el GameManager elige un nuevo due�o, reasigna la bomba y la reactiva
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Reassign(PlayerRef newOwner)
    {
        DeactivateBomb(); // Importante: desactivar antes de moverla

        transform.SetParent(null, worldPositionStays: true);

        if (Runner.TryGetPlayerObject(newOwner, out var netObj))
        {
            var nextPc = netObj.GetComponent<PlayerController>();
            if (nextPc != null && nextPc.bombSlot != null)
            {
                transform.SetParent(nextPc.bombSlot, worldPositionStays: false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;

                if (Object.HasStateAuthority)
                {
                    ActivateBomb(); // Reactiva la bomba al asignar nuevo due�o
                }
            }
        }
    }
}
