using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour, IPlayerJoined
{
    public static LobbyManager instance;
    [Tooltip("Botón Start (solo host)")]
    [SerializeField] private GameObject LobbyPanel;
    [Tooltip("Número mínimo de jugadores para habilitar Start")]
    [SerializeField] private int _minPlayersToStart = 2;
    public PlayerSpawner Spawner;
    public TextMeshProUGUI playercount;

    private List<PlayerRef> Players;

    private void Awake()
    {
        instance = this;
        Players = new List<PlayerRef>();
        LobbyPanel.gameObject.SetActive(false);
    }

    void RemoveFromList(PlayerRef client)
    {
        Players.Remove(client);
    }

    [Rpc]
    public void RPC_Defeat(PlayerRef client)
    {
        if (client == Runner.LocalPlayer)
        {
            UIController.Instance.ShowVictoryUI();
        }

        RemoveFromList(client);

        if (Players.Count == 1 && HasStateAuthority)
        {
            RPC_Win(Players[0]);
        }
    }

    [Rpc]
    void RPC_Win([RpcTarget] PlayerRef client)
    {
        UIController.Instance.ShowVictoryUI();
    }

    public void PlayerJoined(PlayerRef player)
    {
        Players.Add(player);
        if (Runner.IsSharedModeMasterClient)
        {
            Debug.Log("soy el host");
            LobbyPanel.gameObject.SetActive(true);
        }
        playercount.text = Runner.SessionInfo.PlayerCount.ToString();
    }

    public void StartPreparations()
    {
        LobbyPanel.gameObject.SetActive(false);
        UIController.Instance.DisableSkinSelectionUI();
        GameManager.Instance.StartGamemode();
    }
}
