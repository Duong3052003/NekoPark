using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using Unity.Services.Lobbies.Models;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public List<GameObject> players;
    public List<GameObject> playerControl;
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //SetColor(0);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    /*public int GetColor()
    {
        return PlayerPrefs.GetInt("Color", 0);
    }

    public void SetColor(int indexColor)
    {
        PlayerPrefs.SetInt("Color", indexColor);
    }*/

    [ClientRpc]
    public void SetActiveAllPlayersClientRpc(bool boollen)
    {
        foreach (GameObject player in players)
        {
            player.SetActive(boollen);
        }
    }

    [ClientRpc]
    public void SetPlayersClientRpc(bool boolen)
    {
        foreach (GameObject player in players)
        {
            player.SetActive(boolen);
        }
    }

    [ClientRpc]
    public void RefreshPlayersClientRpc(bool isPlayerControl)
    {
        if(isPlayerControl == false)
        {
            foreach (GameObject player in players)
            {
                player.SetActive(false);
                player.SetActive(true);
            }
        }
        else
        {
            players.Clear();

            foreach (GameObject player in playerControl)
            {
                if(player != null)
                {
                    player.SetActive(false);
                    player.SetActive(true);
                }
                
            }
        }
    }

    [ClientRpc]
    public void SetPlayerControlClientRpc(bool boolen)
    {
        if(boolen == false)
        {
            players.Clear();
        }

        foreach (GameObject player in playerControl)
        {
            if (player != null)
            {
                player.SetActive(boolen);
            }

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

    public void ResetPositionAllPlayers()
    {
        for(int i = 0;i < playerControl.Count; i++)
        {
            if (i >=players.Count) return;
            if (players[i] == null || playerControl[i] == null) return;
            players[i].transform.position = playerControl[i].transform.position;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetBodyTypeAllPlayersServerRpc(int index)
    {
        foreach (GameObject player in players)
        {
            if (player.GetComponent<Rigidbody2D>() == null) return;
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
        if (players[numberPlayer] == null) return;
        players[numberPlayer].GetComponent<PlayerMove>().SetPositionNetworkVariable(pos);
    }

    [ClientRpc]
    public void SetPositionPlayersControlClientRpc(int numberPlayer, Vector3 pos)
    {
        if (playerControl[numberPlayer] == null) return;
        playerControl[numberPlayer].transform.position = pos;
    }

    [ServerRpc(RequireOwnership =false)]
    public void SetPositionPlayersServerRpc(int numberPlayer, Vector3 pos)
    {
        players[numberPlayer].transform.position = pos;
    }

    public bool CheckGameOver(int number)
    {
        return players.Count(obj => obj.activeSelf) <= number;
    }

    [ServerRpc(RequireOwnership =false)]
    public void DestroyPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        for(int i = 0; i < players.Count; i++)
        {
            if(players[i].gameObject.GetComponent<NetworkObject>().OwnerClientId == rpcParams.Receive.SenderClientId)
            {
                DestroyPlayer(i, rpcParams.Receive.SenderClientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyAllPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        /*ulong idOwner=0;
        for (int i = 0; i < players.Count; i++)
        {
            //DestroyPlayer(i, idOwner);

            Destroy(players[i].gameObject);

            idOwner++;
        }

        players.Clear();*/

        for (int i = 0; i < playerControl.Count; i++)
        {
            if (playerControl[i] != null)
            {
                NetworkObject networkObject = playerControl[i].GetComponent<NetworkObject>();

                if (networkObject != null && networkObject.IsSpawned)
                {
                    networkObject.Despawn(true);
                }

                Destroy(playerControl[i].gameObject);
            }
        }

        ClearListClientRPC();
    }

    [ClientRpc]
    public void ClearListClientRPC()
    {
        playerControl.Clear();
    }
    
    public void DestroyAllOwnedObjects(ulong clientId)
    {
        NetworkObject[] networkObjects = FindObjectsOfType<NetworkObject>();

        foreach (NetworkObject networkObject in networkObjects)
        {
            var objectServerSpawn = networkObject.GetComponent<IObjectServerSpawn>();
            if (networkObject.OwnerClientId == clientId && networkObject.IsSpawned ==true && objectServerSpawn!=null)
            {
                networkObject.Despawn(true);
                Destroy(networkObject.gameObject);
            }
        }
    }

    private void DestroyPlayer(int count, ulong idOwner)
    {
        foreach (Transform child in players[count].gameObject.transform)
        {
            var childNetwork = child.GetComponent<NetworkObject>();
            if (childNetwork != null && childNetwork.IsSpawned)
            {
                childNetwork.Despawn();
                Destroy(child.gameObject);
            }
        }
        DestroyAllOwnedObjects(idOwner);

        Destroy(players[count].gameObject);

        players.Remove(players[count].gameObject);
    }

    public void RemoveAllPlayers()
    {
        players.Clear();
    }

    public void RemoveAllPlayersControl()
    {
        playerControl.Clear();
    }

    [ClientRpc]
    public void SpawnPlayerClientRpc()
    {
        SpawnPlayerServerRpc();

        /*SpawnPlayerServerRpc(OwnerClientId);
        Debug.Log("owner ID" + this.GetComponent<NetworkObject>().OwnerClientId);*/
    }

    /*[ServerRpc]
    public void SpawnPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        GameObject newPlayer = Instantiate(playerPrefab);

        ulong clientId = rpcParams.Receive.SenderClientId;

        newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        Debug.Log("Player spawned for client ID: " + clientId);
    }*/

    [ServerRpc(RequireOwnership =false)]
    private void SpawnPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        GameObject playerInstance = Instantiate(playerPrefab);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

        networkObject.SpawnAsPlayerObject(rpcParams.Receive.SenderClientId);
        Debug.Log("owner ID" + rpcParams.Receive.SenderClientId);
    }

    [ServerRpc]
    public void DesAndSpawnAllPlayerServerRpc()
    {
        DestroyAllPlayerServerRpc();
        SpawnPlayerClientRpc();
    }
}
