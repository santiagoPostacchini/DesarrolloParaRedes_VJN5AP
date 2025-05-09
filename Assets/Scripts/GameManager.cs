using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class GameManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static GameManager Instance { get; private set; }

    // store each player’s chosen skin index
    Dictionary<PlayerRef, int> _skinChoices = new();

    void Awake()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;
    }

    // called by each PlayerController in Spawned()
    public void RegisterLocalSkin(PlayerRef me, int skinIndex)
    {
        _skinChoices[me] = skinIndex;
        Debug.Log($"[GameManager] Registered skin {skinIndex} for {me}");
    }

    // ── INetworkRunnerCallbacks ─────────────────────────────────────────────────
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // look up the just-spawned NetworkObject
        var netObj = runner.GetPlayerObject(player);
        if (netObj == null) return;

        var pc = netObj.GetComponent<PlayerController>();
        if (pc == null) return;

        // if we have a registered choice for that player, apply it
        if (_skinChoices.TryGetValue(player, out var idx))
        {
            Debug.Log($"[GameManager] Applying skin {idx} for {player}");
            pc.RPC_SetSkin(idx);
        }
        else
        {
            Debug.LogWarning($"[GameManager] No skin registered for {player}");
        }
    }

    // ── other INetworkRunnerCallbacks (no-ops) ─────────────────────────────────
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, Fusion.NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, Fusion.NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, Fusion.ShutdownReason reason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, Fusion.Sockets.NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, Fusion.NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, Fusion.Sockets.NetAddress remoteAddress, Fusion.Sockets.NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, Fusion.NetworkRunnerCallbackArgs.ConnectRequest message) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<Fusion.SessionInfo> sessionList) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, Fusion.Allocator buffer) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnHostMigration(NetworkRunner runner, Fusion.HostMigrationToken hostMigrationToken) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }
}
