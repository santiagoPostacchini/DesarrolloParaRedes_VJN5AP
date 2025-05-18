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

        losePanel?.SetActive(false);

        winPanel?.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    [Rpc]
    public void RPC_DisableSkinSelectionUI()
    {
        if (skinSelectionPanel != null)
            skinSelectionPanel.SetActive(false);
    }

    public void SetSkinNameOnUI(string name)
    {
        if (skinName != null)
            skinName.text = name;
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