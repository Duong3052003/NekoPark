using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerPrefs : MonoBehaviour
{
    public static MyPlayerPrefs Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(Instance);
        }
    }

    public void SaveColor(int indexColor)
    {
        PlayerPrefs.SetInt("Color", indexColor);
    }
}
