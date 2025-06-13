using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private NetworkRunnerHandler _networkRunnerHandler;

    [Header("Panels")]
    [SerializeField] private GameObject _initialPanel;
    [SerializeField] private GameObject _statusPanel;
    [SerializeField] private GameObject _sessionBrowserPanel;
    [SerializeField] private GameObject _hostGamePanel;

    [Header("Buttons")]
    [SerializeField] private Button _joinLobbyBTN;
    [SerializeField] private Button _hostPanelBTN;
    [SerializeField] private Button _hostGameBTN;

    [Header("InputFields")] 
    [SerializeField] private TMP_InputField _sessionName;
    [SerializeField] private TMP_InputField _nicknameField;

    [Header("Texts")]
    [SerializeField] private TMP_Text _statusText;
    
    void Start()
    {
        _joinLobbyBTN.onClick.AddListener(Btn_JoinLobby);
        _hostPanelBTN.onClick.AddListener(Btn_ShowHostPanel);
        _hostGameBTN.onClick.AddListener(Btn_CreateGameSession);

        _networkRunnerHandler.OnJoinedLobby += () =>
        {
            _statusPanel.SetActive(false);
            _sessionBrowserPanel.SetActive(true);
        };
    }

    void Btn_JoinLobby()
    {
        _networkRunnerHandler.JoinLobby();
        
        PlayerPrefs.SetString("Nickname", _nicknameField.text);
        
        _initialPanel.SetActive(false);
        _statusPanel.SetActive(true);

        _statusText.text = "Joining Lobby...";
    }

    void Btn_ShowHostPanel()
    {
        _sessionBrowserPanel.SetActive(false);
        _hostGamePanel.SetActive(true);
    }
    
    void Btn_CreateGameSession()
    {
        _hostGameBTN.interactable = false;
        
        _networkRunnerHandler.CreateGame(_sessionName.text, "Game");
    }
}
