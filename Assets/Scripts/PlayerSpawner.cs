using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [Header("Skins: Prefabs por índice")]
    [SerializeField] private NetworkPrefabRef[] skinPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private bool _gameStarted;
    private int _spawnedPlayers;

    private NetworkRunner _cachedRunner;

    public void PlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Spawner] PlayerJoined: {player}");

        if (runner.IsServer)
        {
            SpawnLocalPlayer(player);
        }

        if (!_cachedRunner.IsSharedModeMasterClient)
            return;

        if (_cachedRunner.SessionInfo.PlayerCount >= 2 && !_gameStarted)
        {
            Debug.Log("[Spawner] Soy MasterClient y ya hay 2+ jugadores: muestro StartButton");
        }
    }

    public void StartGame()
    {
        if (_gameStarted) return;

        NetworkRunner runner = Runner;
        if (!runner)
            runner = FindObjectOfType<NetworkRunner>();

        if (!runner)
        {
            Debug.LogError("[Spawner] Runner is NULL in StartGame. (¿Apretaste el botón muy temprano? ¿Está inicializado el Runner en la escena?)");
            return;
        }

        if (!runner.IsSharedModeMasterClient)
        {
            Debug.LogWarning("[Spawner] Ignorando StartGame: no soy el host");
            return;
        }

        if (runner.SessionInfo == null || runner.SessionInfo.PlayerCount < 2)
        {
            Debug.LogWarning("[Spawner] Ignorando StartGame: menos de 2 jugadores");
            return;
        }

        Debug.Log("[Spawner] Todos ok, llamando a GameManager.StartGame()");
        _gameStarted = true;
        UIController.Instance.RPC_DisableSkinSelectionUI();
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

        Vector3 pos = sp ? sp.position : Vector3.up * 2f;
        Quaternion rot = sp ? sp.rotation : Quaternion.identity;
        Runner.Spawn(prefab, pos, rot, player);
        Debug.Log($"[Spawner] Spawned jugador {player} en skin #{skinIndex}");
        _spawnedPlayers++;
    }
}
