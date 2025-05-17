using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [Header("Skins: Prefabs por índice")]
    [SerializeField] private NetworkPrefabRef[] skinPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("UI")]
    [SerializeField] private GameObject startButtonUI;

    private bool _gameStarted = false;
    private int _spawnedPlayers = 0;

    public void PlayerJoined(PlayerRef player)
    {
        Debug.Log($"[Spawner] PlayerJoined: {player}");

        // Spawn local
        if (player == Runner.LocalPlayer)
        {
            SpawnLocalPlayer(player);
        }

        // Mostrar botón solo al host (MasterClient) cuando haya >=2
        if (!Runner.IsSharedModeMasterClient)
            return;

        if (Runner.SessionInfo.PlayerCount >= 2 && !_gameStarted)
        {
            Debug.Log("[Spawner] Soy MasterClient y ya hay 2+ jugadores: muestro StartButton");
            startButtonUI?.SetActive(true);
        }
    }

    public void StartGame()
    {
        // Evita doble click
        if (_gameStarted) return;

        // Solo el host puede iniciar
        if (!Runner.IsSharedModeMasterClient)
        {
            Debug.LogWarning("[Spawner] Ignorando StartGame: no soy el host");
            return;
        }

        // Verifica conteo justo antes de iniciar
        if (Runner.SessionInfo.PlayerCount < 2)
        {
            Debug.LogWarning("[Spawner] Ignorando StartGame: menos de 2 jugadores");
            return;
        }

        Debug.Log("[Spawner] Todos ok, llamando a GameManager.StartGame()");
        _gameStarted = true;
        startButtonUI?.SetActive(false);

        GameManager.Instance.StartGame();
    }

    private void SpawnLocalPlayer(PlayerRef player)
    {
        int skinIndex = SkinSelection.instance.GetCurrentIndex();
        if (skinIndex < 0 || skinIndex >= skinPrefabs.Length)
        {
            Debug.LogWarning($"[Spawner] Skin index inválido ({skinIndex}), uso 0");
            skinIndex = 0;
        }

        var prefab = skinPrefabs[skinIndex];
        var sp = (_spawnedPlayers < spawnPoints.Length)
            ? spawnPoints[_spawnedPlayers]
            : null;

        Vector3 pos = sp != null ? sp.position : Vector3.up * 2f;
        Quaternion rot = sp != null ? sp.rotation : Quaternion.identity;

        Runner.Spawn(prefab, pos, rot, player);
        Debug.Log($"[Spawner] Spawned jugador {player} en skin #{skinIndex}");
        _spawnedPlayers++;
    }
}
