using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
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

    public void ResetPositionAllPlayers(Vector3 pos)
    {
        foreach (GameObject player in players)
        {
            player.transform.position = pos;
        }
    }

    [ClientRpc]
    public void SetPositionPlayersClientRpc(int numberPlayer,Vector3 pos)
    {
        players[numberPlayer].transform.position = pos;
    }
}
