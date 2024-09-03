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
            Destroy(this.gameObject);
        }
    }

    [ClientRpc]
    public void SetActiveAllPlayersClientRpc(bool boollen)
    {
        foreach (GameObject player in players)
        {
            player.SetActive(boollen);
        }
    }

    [ClientRpc]
    public void RefreshPlayersClientRpc()
    {
        foreach (GameObject player in players)
        {
            player.SetActive(false);
            player.SetActive(true);
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
        return players.Count(obj => obj.activeSelf) <= 1;
    }

    [ServerRpc(RequireOwnership =false)]
    public void DestroyPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        for(int i = 0; i < players.Count; i++)
        {
            if(players[i].gameObject.GetComponent<NetworkObject>().OwnerClientId == rpcParams.Receive.SenderClientId)
            {
                foreach (Transform child in players[i].gameObject.transform)
                {
                    var childNetwork = child.GetComponent<NetworkObject>();
                    if (childNetwork != null && childNetwork.IsSpawned)
                    {
                        childNetwork.Despawn();
                        Destroy(child.gameObject);
                    }
                }

                DestroyAllOwnedObjects(rpcParams.Receive.SenderClientId);

                Destroy(players[i].gameObject);
            }
        }
    }

    public void DestroyAllOwnedObjects(ulong clientId)
    {
        NetworkObject[] networkObjects = FindObjectsOfType<NetworkObject>();

        foreach (NetworkObject networkObject in networkObjects)
        {
            if (networkObject.OwnerClientId == clientId)
            {
                networkObject.Despawn(true);
                Destroy(networkObject.gameObject);
            }
        }
    }

    public void RemoveAllPlayers()
    {
        players.Clear();
    }
}
