using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Player.New;
using Player.New.Inputs;
using UnityEngine;
using NetworkPlayer = Player.New.NetworkPlayer;

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Skins: Prefabs por índice")]
    [SerializeField] private NetworkPrefabRef[] skinPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform spawnPointParent;

    private Bomb.GameManager _gameManager;

    private bool _gameStarted;
    private int _spawnedPlayers;
    private int _connectedPlayers;
    private readonly int _maxPlayers = 5;
    private readonly int _minPlayers = 2;

    private NetworkRunner _cachedRunner;

    private readonly Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        _connectedPlayers++;

        if (runner.IsServer)
        {
            if (_spawnedPlayers < _maxPlayers)
            {
                SpawnLocalPlayer(runner, player);
            }

            if (_connectedPlayers == _minPlayers && _spawnedPlayers == _minPlayers)
            {
                _gameManager = FindObjectOfType<Bomb.GameManager>();
                Debug.Log("[Spawner] Ready to spawn bomb — all players spawned");
                _gameManager.RPC_SpawnBombOnRandomPlayer();
            }
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
        int skinIndex = SkinSelection.Instance.GetCurrentIndex();
        skinIndex = Mathf.Clamp(skinIndex, 0, skinPrefabs.Length - 1);

        var spawnPoints = spawnPointParent.GetComponentsInChildren<Transform>();
        var prefab = skinPrefabs[skinIndex];
        var sp = spawnPoints[_spawnedPlayers % spawnPoints.Length];

        Vector3 pos = sp.position + Vector3.up * 0.1f;
        Quaternion rot = sp.rotation;

        var netObj = runner.Spawn(prefab, pos, rot, player);
        Debug.Log($"[Spawner] Runner.Spawn returned null: {!netObj}");
        
        runner.SetPlayerObject(player, netObj);
        Debug.Log($"[Spawner] Runner.SetPlayerObject for {player}");

        var cc = netObj.GetComponent<NetworkCharacterControllerCustom>();
        cc.Controller.enabled = false;
        netObj.transform.position = pos;
        cc.Controller.enabled = true;
        cc.Velocity = Vector3.zero;

        _spawnedCharacters.Add(player, netObj);
        _spawnedPlayers++;
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
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
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}