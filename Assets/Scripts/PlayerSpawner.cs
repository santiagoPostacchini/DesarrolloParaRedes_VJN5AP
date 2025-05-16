using Fusion;
using UnityEngine;

public class  PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [Header("Skins: Prefabs por �ndice")]
    [SerializeField] private NetworkPrefabRef[] skinPrefabs;

    [Header("Spawn Point")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("UI")]
    [SerializeField] private GameObject startButtonUI;

    private bool _gameStarted = false;
    private int _spawnedPlayers = 0;

    public void PlayerJoined(PlayerRef player)
    {
        Debug.Log($"[Spawner] Se conect� {player}");

        // Spawn inmediato para quien se conecta
        if (player == Runner.LocalPlayer)
        {
            SpawnLocalPlayer(player);
        }

        // Mostrar bot�n de inicio solo al primer jugador local si hay 2 o m�s conectados
        if (Runner.LocalPlayer.PlayerId == 1 &&
            Runner.SessionInfo.PlayerCount >= 2 &&
            startButtonUI != null && !_gameStarted)
        {
            startButtonUI.SetActive(true);
        }
    }

    public void StartGame()
    {
        if (_gameStarted) return;

        if (Runner.SessionInfo.PlayerCount < 2)
        {
            Debug.LogWarning("[Spawner] No se puede iniciar con menos de 2 jugadores");
            return;
        }

        _gameStarted = true;

        if (startButtonUI != null)
            startButtonUI.SetActive(false);

        Debug.Log("[Spawner] Partida iniciada");
        // Pod�s poner aqu� l�gica adicional si necesit�s, como cerrar selecci�n de skin o deshabilitar UI
    }

    private void SpawnLocalPlayer(PlayerRef player)
    {
        int skinIndex = SkinSelection.instance.GetCurrentIndex();

        if (skinIndex < 0 || skinIndex >= skinPrefabs.Length)
        {
            Debug.LogWarning($"[Spawner] �ndice de skin inv�lido: {skinIndex}, usando 0");
            skinIndex = 0;
        }

        var prefab = skinPrefabs[skinIndex];

        Vector3 position = spawnPoints.Length > _spawnedPlayers
            ? spawnPoints[_spawnedPlayers].position
            : Vector3.up * 2f;

        Quaternion rotation = spawnPoints.Length > _spawnedPlayers
            ? spawnPoints[_spawnedPlayers].rotation
            : Quaternion.identity;

        Runner.Spawn(prefab, position, rotation, player);
        Debug.Log($"[Spawner] Spawned {player} with skin #{skinIndex}");

        _spawnedPlayers++;
    }
}
