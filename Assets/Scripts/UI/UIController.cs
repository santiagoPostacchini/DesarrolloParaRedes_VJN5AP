using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] private GameObject skinSelectionPanel;
    [SerializeField] private TextMeshProUGUI skinName;

    private void Awake()
    {
        // Singleton robusto
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Validar referencias
        if (skinSelectionPanel == null) Debug.LogError("Debe asignar skinSelectionPanel en inspector");
        if (skinName == null) Debug.LogError("Debe asignar skinName en inspector");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void DisableSkinSelectionUI()
    {
        // Mejor con CanvasGroup si el panel es muy complejo
        skinSelectionPanel.SetActive(false);
    }

    public void SetSkinNameOnUI(string name)
    {
        if (skinName != null)
            skinName.text = name;
    }
}
