using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Player;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Spawns & Prefabs")]
    [SerializeField] private Transform[] playerSpawnPoints;
    [SerializeField] private NetworkPrefabRef bombPrefab;

    private readonly Dictionary<PlayerRef, PlayerController> _clients = new Dictionary<PlayerRef, PlayerController>();
    private Bomb _currentBomb;
    private NetworkRunner runner;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private NetworkRunner GetRunner()
    {
        if (runner == null)
        {
            runner = GetComponent<NetworkRunner>();
            if (runner == null)
                runner = FindObjectOfType<NetworkRunner>();
            if (runner == null)
                Debug.LogError("[GameManager] Runner is null (could not find runner in scene)");
        }
        return runner;
    }

    public void AddToList(PlayerController player)
    {
        var playerRef = player.Object.InputAuthority;
        if (_clients.ContainsKey(playerRef)) return;
        _clients.Add(playerRef, player);
        Debug.Log($"[GameManager] Added player {playerRef}. Total = {_clients.Count}");
    }

    public void RemoveFromList(PlayerRef client)
    {
        _clients.Remove(client);
        Debug.Log($"[GameManager] Removed player {client}. Total = {_clients.Count}");
    }

    public PlayerController GetPlayerController(PlayerRef pr)
    {
        _clients.TryGetValue(pr, out var pc);
        return pc;
    }

    public bool TryGetPlayerController(PlayerRef pr, out PlayerController pc) => _clients.TryGetValue(pr, out pc);
    public Bomb GetCurrentBomb() => _currentBomb;

    public void StartGame()
    {
        Debug.Log($"[GameManager] StartGame() with {_clients.Count} players");

        int index = 0;
        foreach (var kvp in _clients)
        {
            if (index >= playerSpawnPoints.Length)
                break;
            var pc = kvp.Value;
            var spawn = playerSpawnPoints[index];
            if (pc == null)
            {
                Debug.LogError($"[GameManager] PlayerController for {kvp.Key} is null!");
                index++;
                continue;
            }
            pc.transform.SetPositionAndRotation(
                spawn.position,
                spawn.rotation
            );
            Debug.Log($"[GameManager] Positioned {kvp.Key} at spawn {index}");
            index++;
        }

        if (_currentBomb == null && _clients.Count > 0)
        {
            var keys = new List<PlayerRef>(_clients.Keys);
            var first = keys[Random.Range(0, keys.Count)];

            var usedRunner = GetRunner();
            if (usedRunner == null)
            {
                Debug.LogError("[GameManager] Runner is null when spawning bomb! Aborting bomb spawn.");
                return;
            }

            if (!_clients.TryGetValue(first, out var pc))
            {
                Debug.LogError($"[GameManager] No PlayerController for {first}");
                return;
            }

            if (bombPrefab == null)
            {
                Debug.LogError("[GameManager] bombPrefab is not assigned!");
                return;
            }

            var bombObj = usedRunner.Spawn(
                bombPrefab,
                pc.bombSlot.position,
                Quaternion.identity,
                first
            );

            if (bombObj == null)
            {
                Debug.LogError("[GameManager] Runner.Spawn returned null! Is the prefab registered?");
                return;
            }

            _currentBomb = bombObj.GetComponent<Bomb>();
            if (_currentBomb == null)
            {
                Debug.LogError("[GameManager] Spawned bomb does not have a Bomb component!");
                return;
            }

            _currentBomb.OwnerRef = first;
            if (_currentBomb.Object.HasStateAuthority)
                _currentBomb.Object.AssignInputAuthority(first);

            _currentBomb.ActivateBomb(first);
            Debug.Log($"[GameManager] Bomb spawned on {first} (OwnerRef set, authority assigned)");
        }
    }

    public void OnBombExploded(Bomb bomb)
    {
        var eliminated = bomb.OwnerRef;
        Debug.Log($"[GameManager] Bomb exploded on {eliminated}");
        RPC_Defeat(eliminated);

        if (_clients.Count > 1)
        {
            var keys = new List<PlayerRef>(_clients.Keys);
            var next = keys[Random.Range(0, keys.Count)];
            Debug.Log($"[GameManager] Reassigning bomb to {next}");
            bomb.RPC_Reassign(next);
        }
    }

    [Rpc]
    public void RPC_Defeat(PlayerRef client)
    {
        Debug.Log($"[GameManager] RPC_Defeat: {client}");

        if (client == Runner.LocalPlayer)
        {
            UIController.Instance.ShowEliminated();
        }

        RemoveFromList(client);

        if (_clients.Count == 1 && HasStateAuthority)
        {
            var winner = _clients.Keys.First();
            RPC_Win(winner);
        }
    }

    [Rpc]
    public void RPC_Win([RpcTarget] PlayerRef client)
    {
        Debug.Log($"[GameManager] RPC_Win: {client}");
        UIController.Instance.ShowWin();
    }

    public bool TryGetPlayer(PlayerRef pr, out PlayerController pc) => _clients.TryGetValue(pr, out pc);
}
