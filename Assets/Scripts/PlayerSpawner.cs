using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] GameObject _playerPrefab;

    [SerializeField] Transform[] _spawnPoints;

    bool _initialized;
    int _index;

    //Se ejecuta en CADA cliente conectado cuando uno nuevo entra.
    public void PlayerJoined(PlayerRef player)
    {

        var playerCount = 2; //Runner.SessionInfo.PlayerCount;
        //Es necesario que hayan dos clientes para que se ejecute el juego. El primer cliente inicializa la partida y el segundo la comienza

        if (_initialized && playerCount >= 2)
        {
            CreatePlayer(_index);
            return;
        }

        //Si el cliente que entro, es el mismo que el cliente donde esta ejecutado este codigo, entones:
        if (player == Runner.LocalPlayer)
        {
            if (playerCount < 2)
            {
                if (!_initialized)
                {
                    _initialized = true;
                    _index = playerCount - 1;
                }
            }
            else
            {
                CreatePlayer(playerCount - 1);
            }
        }
    }

    void CreatePlayer(int playerIndex)
    {
        _initialized = false;

        var spawnPoint = _spawnPoints[playerIndex];

        Runner.Spawn(_playerPrefab, spawnPoint.position + Vector3.up, spawnPoint.rotation, Runner.LocalPlayer);
    }
}
