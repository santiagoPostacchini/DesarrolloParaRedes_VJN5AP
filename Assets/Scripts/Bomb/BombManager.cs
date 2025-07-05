using System.Linq;
using Fusion;
using UnityEngine;

namespace Bomb
{
    public class BombManager : NetworkBehaviour
    {
        [SerializeField] private NetworkPrefabRef bombPrefab;
        private BombBehaviour _bombInstance;

        public override void Spawned()
        {
            Debug.Log($"[BombManager] Spawned! Authority: {HasStateAuthority}");
        }

        public bool HasBomb(PlayerRef p) =>
            _bombInstance && _bombInstance.Holder == p;

        public void TransferBomb(PlayerRef newHolder)
        {
            if (!HasStateAuthority || !_bombInstance) return;
            _bombInstance.PickUp(newHolder);
        }

        public void SpawnBombOnRandomPlayer()
        {
            Debug.Log("[BombManager] SpawnBombOnRandomPlayer called");
            Debug.Log($"[BombManager] ActivePlayers count = {Runner.ActivePlayers.Count()}");

            var players = Runner.ActivePlayers.ToList();
            if (!players.Any())
            {
                Debug.LogWarning("[BombManager] No players in Runner.ActivePlayers!");
                return;
            }

            var rnd = players[Random.Range(0, players.Count)];
            Debug.Log($"[BombManager] Selected random player: {rnd}");

            if (!Runner.TryGetPlayerObject(rnd, out var playerObj))
            {
                Debug.LogError($"[BombManager] TryGetPlayerObject failed for player {rnd}");
                return;
            }
            Debug.Log($"[BombManager] Player object found: {playerObj.name}");

            var spawnPos = playerObj.transform.position + Vector3.up * 1.5f;
            Debug.Log($"[BombManager] Spawning bomb at {spawnPos}");

            var netObj = Runner.Spawn(bombPrefab, spawnPos, Quaternion.identity, rnd);
            if (netObj == null)
            {
                Debug.LogError("[BombManager] Runner.Spawn returned null — check bombPrefab is spawnable!");
                return;
            }

            Debug.Log("[BombManager] Bomb spawned successfully");

            _bombInstance = netObj.GetComponent<BombBehaviour>();
            if (_bombInstance == null)
            {
                Debug.LogError("[BombManager] Spawned object has no BombBehaviour component!");
                return;
            }

            _bombInstance.PickUp(rnd);
            _bombInstance.OnExplode += HandleBombExplode;
            Debug.Log($"[BombManager] Bomb picked up by {rnd}");
        }

        private void HandleBombExplode(PlayerRef holder)
        {
            if (!HasStateAuthority) return;
            
            Debug.Log("Explota la bomba!");
            Runner.Invoke(nameof(SpawnBombOnRandomPlayer), 1f);
        }
    }
}