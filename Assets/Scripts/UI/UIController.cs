using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fusion;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [Header("Skin Selection UI")]
    [SerializeField] private GameObject skinSelectionPanel;
    [SerializeField] private TextMeshProUGUI skinName;
    [SerializeField] private Button startButton;

    [Header("Elimination UI")]
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject winPanel;

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

        losePanel?.SetActive(false);

        winPanel?.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void DisableSkinSelectionUI()
    {
        if (skinSelectionPanel != null)
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

    public void OnStartButtonClicked()
    {
        if (FindObjectOfType<PlayerSpawner>() is PlayerSpawner spawner)
        {
            spawner.StartGame();
        }
        DisableSkinSelectionUI();
        startButton.gameObject.SetActive(false);
    }

    public void ShowEliminated()
    {
        losePanel?.SetActive(true);
    }
    public void ShowWin()
    {
        winPanel?.SetActive(true);
    }
}