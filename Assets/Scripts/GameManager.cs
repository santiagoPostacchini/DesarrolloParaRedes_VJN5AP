using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) 
            Destroy(gameObject);
        else 
        { 
            Instance = this; DontDestroyOnLoad(gameObject);
        }
    }

    public void StartGamemode()
    {

    }

    public GameObject GetPlayerSkin()
    {
        return SkinSelection.instance.GetCurrentSelection();
    }
}