using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public List<GameObject> players;

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
    
    public void SetActiveAllPlayers(bool boollen)
    {
        foreach (GameObject player in players)
        {
            player.SetActive(boollen);
        }
    }

    public void SetPositionAllPlayers(Vector3 pos)
    {
        foreach (GameObject player in players)
        {
            player.transform.position = pos;
        }
    }
}
