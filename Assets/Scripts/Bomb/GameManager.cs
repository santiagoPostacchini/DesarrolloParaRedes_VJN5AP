using System.Linq;
using Fusion;
using UnityEngine;

namespace Bomb
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private NetworkPrefabRef bombPrefab;
        public BombBehaviour bombInstance;

        public GameObject winScreen;
        public GameObject loseScreen;

        public override void Spawned()
        {
            Debug.Log($"[BombManager] Spawned! Authority: {HasStateAuthority}");

            // Asegurarse de que las pantallas estén ocultas al inicio
            if (winScreen) winScreen.SetActive(false);
            if (loseScreen) loseScreen.SetActive(false);
        }

        public bool HasBomb(PlayerRef p) =>
            bombInstance && bombInstance.Holder == p;

        public void TransferBomb(PlayerRef newHolder)
        {
            if (!HasStateAuthority || !bombInstance) return;
            bombInstance.PickUp(newHolder);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_SpawnBombOnRandomPlayer()
        {
            Debug.Log("[BombManager] SpawnBombOnRandomPlayer called");

            // Obtener jugadores que realmente tienen GameObjects válidos
            var validPlayers = GetValidPlayers();

            if (!validPlayers.Any())
            {
                Debug.LogWarning("[BombManager] No valid players found!");
                return;
            }

            var rnd = validPlayers[Random.Range(0, validPlayers.Count)];
            Debug.Log($"[BombManager] Selected random player: {rnd}");

            var playerObj = Runner.GetPlayerObject(rnd);
            Debug.Log($"[BombManager] Player object found: {playerObj.name}");

            var spawnPos = playerObj.transform.position + Vector3.up * 1.5f;
            Debug.Log($"[BombManager] Spawning bomb at {spawnPos}");

            var netObj = Runner.Spawn(bombPrefab, spawnPos, Quaternion.identity, rnd);

            if (!netObj)
            {
                Debug.LogError("[BombManager] Runner.Spawn returned null — check bombPrefab is spawnable!");
                return;
            }

            Debug.Log("[BombManager] Bomb spawned successfully");

            bombInstance = netObj.GetComponent<BombBehaviour>();
            if (!bombInstance)
            {
                Debug.LogError("[BombManager] Spawned object has no BombBehaviour component!");
                return;
            }

            bombInstance.PickUp(rnd);
            bombInstance.OnExplode += HandleBombExplode;
            Debug.Log($"[BombManager] Bomb picked up by {rnd}");
        }

        private System.Collections.Generic.List<PlayerRef> GetValidPlayers()
        {
            var validPlayers = new System.Collections.Generic.List<PlayerRef>();

            foreach (var player in Runner.ActivePlayers)
            {
                if (Runner.TryGetPlayerObject(player, out var playerObj) && playerObj != null)
                {
                    Debug.Log($"[BombManager] Valid player found: {player}");
                    validPlayers.Add(player);
                }
                else
                {
                    Debug.Log($"[BombManager] Player {player} has no valid GameObject, skipping");
                }
            }

            Debug.Log($"[BombManager] Valid players count: {validPlayers.Count}");
            return validPlayers;
        }

        private void HandleBombExplode(PlayerRef holder)
        {
            if (!HasStateAuthority) return;
            
            Debug.Log("Explota la bomba!");
            
            // Eliminar el player
            if (Runner.TryGetPlayerObject(holder, out var playerObj))
            {
                Debug.Log($"[BombManager] Eliminando jugador: {holder}");
                Runner.Despawn(playerObj);
            }
            
            // Limpiar la referencia de la bomba
            bombInstance = null;
            
            // Obtener jugadores válidos después de la eliminación
            var validPlayers = GetValidPlayers();
            Debug.Log($"[BombManager] Jugadores válidos después de la explosión: {validPlayers.Count}");
            
            if (validPlayers.Count >= 2)
            {
                // Si hay 2 o más jugadores válidos, continúa el juego
                Debug.Log("[BombManager] Continuando el juego, spawneando nueva bomba...");
                RPC_SpawnBombOnRandomPlayer();
            }
            else if (validPlayers.Count == 1)
            {
                // Si solo queda 1 jugador válido, es el ganador
                var winner = validPlayers[0];
                Debug.Log($"[BombManager] ¡Juego terminado! Ganador: {winner}");
                RPC_GameOver(winner);
            }
            else
            {
                // Si no quedan jugadores válidos (empate)
                Debug.Log("[BombManager] ¡Empate! No quedan jugadores válidos.");
                RPC_GameOver(PlayerRef.None);
            }
        }

        private void ShowDefeatScreen()
        {
            if (loseScreen)
            {
                loseScreen.SetActive(true);
                Debug.Log("[BombManager] Pantalla de derrota mostrada");
            }
            else
            {
                Debug.LogWarning("[BombManager] loseScreen no está asignado!");
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_GameOver(PlayerRef winner)
        {
            Debug.Log($"[BombManager] Game Over! Winner: {winner}");

            if (winner == PlayerRef.None)
            {
                Debug.Log("¡Empate!");
                // Mostrar pantalla de empate (puedes usar loseScreen o crear una específica)
                ShowDefeatScreen();
            }
            else if (Runner.LocalPlayer == winner)
            {
                Debug.Log($"¡El jugador {winner} ha ganado!");
                // Mostrar pantalla de victoria para el ganador
                ShowVictoryScreen();
            }
            else
            {
                Debug.Log($"El jugador {winner} ha ganado, pero no soy yo");
                // Mostrar pantalla de derrota para los demás
                ShowDefeatScreen();
            }
        }

        private void ShowVictoryScreen()
        {
            if (winScreen)
            {
                winScreen.SetActive(true);
                Debug.Log("[BombManager] Pantalla de victoria mostrada");
            }
            else
            {
                Debug.LogWarning("[BombManager] winScreen no está asignado!");
            }
        }
        
        // Métodos públicos para botones en las pantallas UI
        public void RestartGame()
        {
            Debug.Log("[BombManager] Reiniciando juego...");

            // Ocultar pantallas
            if (winScreen) winScreen.SetActive(false);
            if (loseScreen) loseScreen.SetActive(false);

            // Aquí puedes agregar lógica para reiniciar el juego
            // Por ejemplo: recargar la escena, respawnear jugadores, etc.
        }

        public void GoToMainMenu()
        {
            Debug.Log("[BombManager] Volviendo al menú principal...");

            // Ocultar pantallas
            if (winScreen) winScreen.SetActive(false);
            if (loseScreen) loseScreen.SetActive(false);

            // Aquí puedes agregar lógica para volver al menú
            // Por ejemplo: cargar escena del menú principal
        }
    }
}