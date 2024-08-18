using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

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

    public void GetSettingStatusPlayer()
    {
        foreach (GameObject player in players)
        {
            ulong idPlayer = player.GetComponent<NetworkObject>().OwnerClientId;
            player.GetComponent<IPlayerStatus>().GetSetting(idPlayer);
        }
    }

    [ClientRpc]
    public void SetupPlayersClientRPC()
    {
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerSetting>().SetUpPlayer();
        }
    }

    public void ResetPositionAllPlayers(Vector3 pos)
    {
        foreach (GameObject player in players)
        {
            player.transform.position = pos;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetBodyTypeAllPlayersServerRpc(int index)
    {
        foreach (GameObject player in players)
        {
            switch (index)
            {
                case 0:
                    player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                    break;
                case 1:
                    player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                    break;
                case 2:
                    player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                    break ;
                default: break;
            }
        }
    }

    [ClientRpc]
    public void SetPositionPlayersClientRpc(int numberPlayer,Vector3 pos)
    {
        players[numberPlayer].transform.position = pos;
    }

    [ServerRpc(RequireOwnership =false)]
    public void SetPositionPlayersServerRpc(int numberPlayer, Vector3 pos)
    {
        players[numberPlayer].transform.position = pos;
    }

    public bool CheckGameOver()
    {
        return players.Count(obj => obj.activeSelf) == 1;
    }
}
