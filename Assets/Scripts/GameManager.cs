using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Fusion.Sockets;

public class GameManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static GameManager Instance { get; private set; }

    [Header("Spawns")]
    [SerializeField] private Transform[] playerSpawnPoints;

    [Header("Settings")]
    [SerializeField] private int minPlayersToStart = 2;

    [Header("Lobby UI")]
    [SerializeField] private GameObject startButtonObj;

    [Header("Prefabs")]
    [SerializeField] private NetworkPrefabRef bombPrefab;

    private NetworkRunner _runner;
    private readonly List<PlayerRef> _alive = new List<PlayerRef>();
    private Bomb _currentBomb;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else { Instance = this; DontDestroyOnLoad(gameObject); }
    }

    private void Start()
    {
        _runner = FindObjectOfType<NetworkRunner>();
        if (_runner == null)
        {
            Debug.LogError("[GameManager] No NetworkRunner found!");
            return;
        }
        _runner.AddCallbacks(this);

        _alive.Clear();

        // NUEVO: Pre-popular con jugadores ya activos, SOLO si eres host
        if (_runner.IsSharedModeMasterClient)
        {
            foreach (var pr in _runner.ActivePlayers)
            {
                if (!_alive.Contains(pr))
                {
                    _alive.Add(pr);
                    Debug.Log($"[GameManager] Pre-populated alive with: {pr}");
                }
            }
        }

        UpdateStartButton();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!_runner.IsSharedModeMasterClient) return;
        if (!_alive.Contains(player))
        {
            _alive.Add(player);
            Debug.Log($"[GameManager] Player joined: {player}, alive: {_alive.Count}");
            UpdateStartButton();
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!_runner.IsSharedModeMasterClient) return;
        if (_alive.Remove(player))
        {
            Debug.Log($"[GameManager] Player left: {player}, alive: {_alive.Count}");
            UpdateStartButton();
        }
    }

    // --- Nuevo: Sólo permite arrancar cuando todos los PlayerObjects existen ---
    private bool AllPlayerObjectsExist()
    {
        foreach (var pr in _alive)
        {
            var netObj = _runner.GetPlayerObject(pr);
            if (netObj == null)
            {
                Debug.LogWarning($"[GameManager] Esperando PlayerObject de {pr}");
                return false;
            }
        }
        return true;
    }

    private void UpdateStartButton()
    {
        bool ready = _alive.Count >= minPlayersToStart &&
                     _runner.IsSharedModeMasterClient &&
                     AllPlayerObjectsExist();

        if (startButtonObj != null)
            startButtonObj.SetActive(ready);
    }

    // --- Llama este método desde el botón de Start de la UI ---
    public void OnStartButtonPressed()
    {
        StartGame();
    }

    public void StartGame()
    {
        Debug.Log($"[GameManager] Starting game with {_alive.Count} players");
        if (!AllPlayerObjectsExist())
        {
            Debug.LogWarning("[GameManager] No todos los PlayerObjects existen. Esperando...");
            UpdateStartButton();
            return;
        }

        // Posiciona todos los jugadores
        for (int i = 0; i < _alive.Count && i < playerSpawnPoints.Length; i++)
        {
            var pr = _alive[i];
            var netObj = _runner.GetPlayerObject(pr);
            if (netObj == null)
            {
                Debug.LogError($"[GameManager] No PlayerObject for {pr}! No se puede posicionar.");
                continue;
            }
            netObj.transform.position = playerSpawnPoints[i].position;
            netObj.transform.rotation = playerSpawnPoints[i].rotation;
        }

        // Spawn de bomba inicial
        if (_currentBomb == null && _alive.Count > 0)
        {
            var randomIndex = Random.Range(0, _alive.Count);
            SpawnBombOn(_alive[randomIndex]);
        }
        // Aquí podés desactivar la UI del lobby si lo necesitás
    }

    private void SpawnBombOn(PlayerRef pr)
    {
        var netObj = _runner.GetPlayerObject(pr);
        if (netObj == null)
        {
            Debug.LogError($"[GameManager] No PlayerObject for {pr} al intentar asignar bomba.");
            return;
        }

        var pc = netObj.GetComponent<PlayerController>();
        if (pc?.bombSlot == null)
        {
            Debug.LogError($"[GameManager] Player {pr} no tiene bombSlot asignado.");
            return;
        }

        var bombObj = _runner.Spawn(
            bombPrefab, // Prefab asignado por Inspector
            pc.bombSlot.position,
            Quaternion.identity,
            pr
        );
        _currentBomb = bombObj.GetComponent<Bomb>();
        bombObj.transform.SetParent(pc.bombSlot, worldPositionStays: false);

        _currentBomb.ActivateBomb();

        Debug.Log($"[GameManager] Bomb spawned & parented to {pr}");
    }

    public void OnBombExploded(Bomb bomb)
    {
        Debug.Log("[GameManager] 💥 OnBombExploded()");

        // Si solo queda un jugador, fin de la partida
        if (_alive.Count <= 1)
        {
            Debug.Log("[GameManager] Only one left, game over.");
            RPC_GameOver(_alive.First());
            return;
        }

        // Escoge un nuevo dueño aleatorio de los vivos
        var next = _alive[Random.Range(0, _alive.Count)];
        bomb.RPC_Reassign(next);
    }

    public void OnPlayerEliminated(PlayerRef pr)
    {
        if (_alive.Remove(pr))
            Debug.Log($"[GameManager] Player eliminated: {pr}, alive now {_alive.Count}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_GameOver(PlayerRef winner)
    {
        Debug.Log($"🏆 Game Over – Winner: {winner}");
        // Aquí lógica visual/UI de fin de partida
    }

    // --- Callbacks vacíos de INetworkRunnerCallbacks ---
    public void OnInput(NetworkRunner r, NetworkInput i) { }
    public void OnInputMissing(NetworkRunner r, PlayerRef p, NetworkInput i) { }
    public void OnShutdown(NetworkRunner r, ShutdownReason s) { }
    public void OnConnectedToServer(NetworkRunner r) { }
    public void OnDisconnectedFromServer(NetworkRunner r, NetDisconnectReason d) { }
    public void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest req, byte[] token) { }
    public void OnConnectFailed(NetworkRunner r, NetAddress addr, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest msg) { }
    public void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> list) { }
    public void OnReliableDataReceived(NetworkRunner r, PlayerRef p, Allocator buf) { }
    public void OnSceneLoadDone(NetworkRunner r) { }
    public void OnSceneLoadStart(NetworkRunner r) { }
    public void OnHostMigration(NetworkRunner r, HostMigrationToken t) { }
    public void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) { }
    public void OnObjectExitAOI(NetworkRunner r, NetworkObject obj, PlayerRef p) { }
    public void OnObjectEnterAOI(NetworkRunner r, NetworkObject obj, PlayerRef p) { }
    public void OnUserSimulationMessage(NetworkRunner r, SimulationMessagePtr msg) { }
    public void OnReliableDataProgress(NetworkRunner r, PlayerRef p, ReliableKey key, float prog) { }
    public void OnReliableDataReceived(NetworkRunner r, PlayerRef p, ReliableKey key, System.ArraySegment<byte> data) { }
}
