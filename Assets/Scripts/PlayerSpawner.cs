using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform[] _spawnPoints;

    public void PlayerJoined(PlayerRef player)
    {
        if (player != Runner.LocalPlayer)
            return;

        CreatePlayer(Runner.LocalPlayer.PlayerId - 1);

        Debug.Log($"[Spawner] Spawned avatar for Player {player}");
    }

    void CreatePlayer(int index)
    {
        var spawnPoint = _spawnPoints[index];
        NetworkObject playerObject =  Runner.Spawn(_playerPrefab, Vector3.up/2, spawnPoint.rotation);
    }
}