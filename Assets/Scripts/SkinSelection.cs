using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class SkinSelection : MonoBehaviour
{
    [SerializeField] MainMenuHandler _mainMenuHandler;
    public static SkinSelection Instance;
    public List<GameObject> skins;
    private int _selectedSkin = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void NextCharacter()
    {
        skins[_selectedSkin].SetActive(false);
        _selectedSkin = (_selectedSkin + 1) % skins.Count;
        skins[_selectedSkin].SetActive(true);
        _mainMenuHandler.UpdateSkinName(skins[_selectedSkin].name);
    }
    
    public void PrevCharacter()
    {
        skins[_selectedSkin].SetActive(false);
        _selectedSkin--;
        if(_selectedSkin < 0)
        {
            _selectedSkin += skins.Count;
        }
        skins[_selectedSkin].SetActive(true);
        _mainMenuHandler.UpdateSkinName(skins[_selectedSkin].name);
    }

    public int GetCurrentIndex()
    {
        return _selectedSkin;
    }
}