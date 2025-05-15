using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        // Solo el jugador local solicita su propio spawn
        if (player != Runner.LocalPlayer)
            return;

        Runner.Spawn(
            PlayerPrefab,
            Vector3.up,              // posición de spawn
            Quaternion.identity,
            player                   // asigna autoridad de entrada al jugador
        );

        Debug.Log($"[Spawner] Spawned avatar for Player {player}");
    }
}
