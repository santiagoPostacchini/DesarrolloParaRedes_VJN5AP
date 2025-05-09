//using Fusion;
//using UnityEngine;

//public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
//{
//    public GameObject PlayerPrefab;

//    public void PlayerJoined(PlayerRef player)
//    {
//        // Only the host (StateAuthority) spawns avatars for everyone
//        if (player != Runner.LocalPlayer)
//            return;

//        // This spawn is automatically propagated to all clients
//        Runner.Spawn(
//            PlayerPrefab,
//            Vector3.up,              // choose your spawn height
//            Quaternion.identity,
//            player                   // grants that client InputAuthority
//        );

//        Debug.Log($"[Spawner] Spawned avatar for Player {player}");
//    }
//}

using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [Tooltip("Prefab con NetworkObject y NetworkBehaviour de tu PlayerController")]
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        // Solo el host (StateAuthority) spawnea los avatares para TODOS los clientes
        if (!Runner.IsServer)
            return;

        // Este Spawn se replica automáticamente en todos los clientes
        Runner.Spawn(
            PlayerPrefab,
            Vector3.up,              // altura de spawn
            Quaternion.identity,
            player                   // le da InputAuthority a ese cliente
        );

        Debug.Log($"[Spawner] Spawned avatar for Player {player}");
    }
}
