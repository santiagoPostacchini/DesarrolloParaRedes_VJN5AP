using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public GameObject skinSelectionPanel;
    public TextMeshProUGUI skinName;

    private void Awake()
    {
        Instance = this;
    }

    public void DisableSkinSelectionUI()
    {
        skinSelectionPanel.SetActive(false);
    }

    public void SetSkinNameOnUI(string name)
    {
        skinName.text = name;
    }
}
