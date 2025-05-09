using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        // Only the host (StateAuthority) spawns avatars for everyone
        if (player != Runner.LocalPlayer)
            return;

        // This spawn is automatically propagated to all clients
        Runner.Spawn(
            PlayerPrefab,
            Vector3.up,              // choose your spawn height
            Quaternion.identity,
            player                   // grants that client InputAuthority
        );

        Debug.Log($"[Spawner] Spawned avatar for Player {player}");
    }
}