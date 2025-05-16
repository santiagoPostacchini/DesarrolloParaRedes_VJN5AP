using Fusion;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bomb : NetworkBehaviour
{
    // Para que no procese m�s de una vez
    private bool _pickedUp = false;

    private void OnTriggerEnter(Collider other)
    {
        // S�lo el host debe decidir el pickup
        if (_pickedUp || !Object.HasStateAuthority)
            return;

        // Chequear si choc� con un PlayerController
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            _pickedUp = true;

            // Llamar al RPC de player para adjuntar la bomba
            player.RpcAttachBomb(this.Object.Id);
        }
    }
}
