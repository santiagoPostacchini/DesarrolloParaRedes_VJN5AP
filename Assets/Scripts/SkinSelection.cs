using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinSelection : MonoBehaviour
{
    public static SkinSelection instance;
    public List<GameObject> skins;
    private int selectedSkin = 0;

    private void Awake()
    {
        instance = this;
    }

    public void NextCharacter()
    {
        skins[selectedSkin].SetActive(false);
        selectedSkin = (selectedSkin + 1) % skins.Count;
        skins[selectedSkin].SetActive(true);
        UIController.Instance.SetSkinNameOnUI(skins[selectedSkin].name);
    }
    
    public void PrevCharacter()
    {
        skins[selectedSkin].SetActive(false);
        selectedSkin--;
        if(selectedSkin < 0)
        {
            selectedSkin += skins.Count;
        }
        skins[selectedSkin].SetActive(true);
        UIController.Instance.SetSkinNameOnUI(skins[selectedSkin].name);
    }

    public GameObject GetCurrentSelection()
    {
        return skins[selectedSkin];
    }

    public int GetCurrentIndex()
    {
        return selectedSkin;
    }
}