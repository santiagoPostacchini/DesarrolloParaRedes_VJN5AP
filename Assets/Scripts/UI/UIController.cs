using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fusion;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] private GameObject skinSelectionPanel;
    [SerializeField] private TextMeshProUGUI skinName;
    [SerializeField] private Button startButton;

    private NetworkRunner _runner;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _runner = FindObjectOfType<NetworkRunner>();

        if (skinSelectionPanel == null) Debug.LogError("Asignar skinSelectionPanel en inspector");
        if (skinName == null) Debug.LogError("Asignar skinName en inspector");
        if (startButton == null) Debug.LogError("Asignar startButton en inspector");
        else startButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void DisableSkinSelectionUI()
    {
        skinSelectionPanel.SetActive(false);
    }

    public void SetSkinNameOnUI(string name)
    {
        if (skinName != null)
            skinName.text = name;
    }

    public void ShowStartButtonIfHost()
    {
        if (_runner != null && _runner.IsSharedModeMasterClient)
        {
            startButton.gameObject.SetActive(true);
        }
        else
        {
            startButton.gameObject.SetActive(false);
        }
    }

    private void OnStartButtonClicked()
    {
        if (FindObjectOfType<PlayerSpawner>() is PlayerSpawner spawner)
        {
            spawner.StartGame();
        }
        DisableSkinSelectionUI();
        startButton.gameObject.SetActive(false);
    }
}