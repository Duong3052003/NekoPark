using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private float heartBeatTimer;
    private string playerName;


    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Sign in: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Player test" + Random.Range(10, 100);
        Debug.Log(playerName);
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
    }

    private async void HandleLobbyHeartBeat()
    {
        if (hostLobby != null)
        {
            heartBeatTimer -= Time.deltaTime;
            if(heartBeatTimer < 0)
            {
                heartBeatTimer = 15;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName) }
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);

            hostLobby = lobby;

            Debug.Log("Created Lobby! " + lobbyName + " " +maxPlayers);
            PrintPlayers(hostLobby);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void ListLobby()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinLobby()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobby Found: "+queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }

            await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in "+lobby.Name);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " "+ player.Data["PlayerName"].Value);
        }
    }
}
