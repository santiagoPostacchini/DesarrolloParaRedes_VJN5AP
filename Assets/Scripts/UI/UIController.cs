using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] private GameObject skinSelectionPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private TextMeshProUGUI skinName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (skinSelectionPanel == null) Debug.LogError("Debe asignar skinSelectionPanel en inspector");
        if (skinName == null) Debug.LogError("Debe asignar skinName en inspector");
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

    public void ShowVictoryUI()
    {
        winPanel.SetActive(true);
    }

    public void HideVictoryUI()
    {
        winPanel.SetActive(false);
    }
    
    public void ShowDefeatUI()
    {
        losePanel.SetActive(true);
    }

    public void HideDefeatUI()
    {
        losePanel.SetActive(false);
    }

    public void ShowGameUI()
    {
        HideDefeatUI();
        HideVictoryUI();
    }
}
