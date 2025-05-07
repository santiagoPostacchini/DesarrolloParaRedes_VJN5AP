using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        // Only spawn your own avatar
        if (player != Runner.LocalPlayer)
            return;

        Runner.Spawn(
            PlayerPrefab,
            Vector3.up,
            Quaternion.identity,
            player
        );
    }
}