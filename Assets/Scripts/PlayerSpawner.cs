using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Player.New.Inputs;
using UnityEngine;
using NetworkPlayer = Player.New.NetworkPlayer;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Skins: Prefabs por índice")]
    [SerializeField] private NetworkPrefabRef[] skinPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform spawnPointParent;

    private bool _gameStarted;
    private int _spawnedPlayers;

    private NetworkRunner _cachedRunner;


    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Spawner] PlayerJoined: {player}");

        if (runner.IsServer)
        {
            SpawnLocalPlayer(runner, player);
        }
    }

    private LocalInputs _localInputs;
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (!NetworkPlayer.Local) return;

        _localInputs ??= NetworkPlayer.Local.LocalInputs;

        input.Set(_localInputs.GetLocalInputs());
    }
    
    private void SpawnLocalPlayer(NetworkRunner runner, PlayerRef player)
    {
        int skinIndex = SkinSelection.instance.GetCurrentIndex();
        if (skinIndex < 0 || skinIndex >= skinPrefabs.Length)
        {
            Debug.LogWarning($"[Spawner] Skin index inválido ({skinIndex}), uso 0");
            skinIndex = 0;
        }
        
        var spawnPoints = spawnPointParent.GetComponentsInChildren<Transform>();
        
        var prefab = skinPrefabs[skinIndex];
        var sp = (_spawnedPlayers < spawnPoints.Length)
            ? spawnPoints[_spawnedPlayers]
            : null;

        Vector3 pos = sp ? sp.position : Vector3.up * 2f;
        Quaternion rot = sp ? sp.rotation : Quaternion.identity;
        runner.Spawn(prefab, pos, rot, player);
        Debug.Log($"[Spawner] Spawned jugador {player} en skin #{skinIndex}");
        _spawnedPlayers++;
    }
    
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        runner.Shutdown();
    }
    
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    
    /*public void StartGame()
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
    */
}
