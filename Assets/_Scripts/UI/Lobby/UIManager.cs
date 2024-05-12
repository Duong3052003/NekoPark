using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;
using static Cinemachine.DocumentationSortingAttribute;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Lobby joinedLobby;

    private float heartBeatTimer;
    private float lobbyUpdatetimer;

    private string playerName;


    //Screen
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject listLobbyScreen;
    [SerializeField] private GameObject hostScreen;
    [SerializeField] private GameObject lobbyScreen;

    //List Lobby
    [SerializeField] private GameObject lobbyGameObj;
    [SerializeField] private Transform listLobby;

    //Host Lobby
    [SerializeField] private TextMeshProUGUI isPrivateTxt;

    //Lobby
    [SerializeField] private GameObject informationPlayer;
    [SerializeField] private Transform listPlayer;

    //Others
    [SerializeField] private GameObject backGround;
    [SerializeField] private TextMeshProUGUI gameModeTxt;
    [SerializeField] private TMP_InputField lobbyCodeTF;
    [SerializeField] private TMP_InputField playerNameTF;
    [SerializeField] private TMP_InputField IDLobbyTF;
    private GameObject gameStarted;
    public bool alStarted=false;

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

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Sign in: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        playerName = "Player" + UnityEngine.Random.Range(10, 100);
        playerNameTF.text = playerName;

        Debug.Log(playerName);
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdate();
        GameStarted();
    }

    #region Lobby
    private async void HandleLobbyHeartBeat()
    {
        if (IsLobbyHost())
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer < 0)
            {
                heartBeatTimer = 15;

                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdate()
    {
        if (joinedLobby != null)
        {
            lobbyUpdatetimer -= Time.deltaTime;
            if (lobbyUpdatetimer < 0)
            {
                float lobbyUpdatetimermax = 1.1f;
                lobbyUpdatetimer = lobbyUpdatetimermax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;

                if (!IsPlayerInLobby())
                {
                    Debug.Log("Kicked from lobby");
                    joinedLobby = null;
                }

                if (alStarted) return;

                UpdatePlayerLobby();

                if (joinedLobby.Data["StartGame"].Value != "0")
                {
                    StartScene();
                }
            }
        }
    }

    public async void CreateLobby()
    {
        try
        {
            bool _isPrivate;

            if (isPrivateTxt.text.Equals("Private"))
            {
                _isPrivate = true;
            }
            else
            {
                _isPrivate = false;
            }

            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = _isPrivate,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode",new DataObject(DataObject.VisibilityOptions.Public,gameModeTxt.text) },
                    {"StartGame",new DataObject(DataObject.VisibilityOptions.Member,"0") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(playerNameTF.text, maxPlayers, createLobbyOptions);

            joinedLobby = lobby;

            Debug.Log("Created Lobby! " + playerNameTF.text + " " + maxPlayers + " " + lobby.Data["GameMode"].Value + " " + lobby.IsPrivate + " " + lobby.Id + " " + lobby.LobbyCode);
            PrintPlayers(joinedLobby);
            LobbyScreen();
            GetIDLobby(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    public async void ListLobby()
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

            int index = 0;

            foreach (Transform childTransform in listLobby)
            {
                GameObject child = childTransform.gameObject;
                Destroy(child);
            }

            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.Players.Count + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);

                if (lobbyGameObj != null && listLobby != null)
                {
                    GameObject newLobbyObj = Instantiate(lobbyGameObj, listLobby);

                    if (newLobbyObj != null)
                    {
                        LobbyGameObj lobbyGameComponent = newLobbyObj.GetComponent<LobbyGameObj>();

                        lobbyGameComponent.index = index;
                        index++;

                        if (lobbyGameComponent != null)
                        {
                            lobbyGameComponent.SetInformation(lobby.Name, lobby.Players.Count, lobby.MaxPlayers, lobby.Data["GameMode"].Value);
                        }
                        else
                        {
                            Debug.LogError("LobbyGameObj component is missing on instantiated object.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to instantiate lobbyGameObj.");
                    }
                }
                else
                {
                    Debug.LogError("lobbyGameObj or listLobby is not initialized.");
                }
            }

        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby(int index)
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            JoinLobbyByIdOptions joinLobbyIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };

            Lobby newlobby = await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[index].Id, joinLobbyIdOptions);
            joinedLobby = newlobby;

            PrintPlayers(joinedLobby);
            LobbyScreen();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode()
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCodeTF.text, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            Debug.Log("Join Lobby with Code" + lobbyCodeTF.text);
            LobbyScreen();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            await Lobbies.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdateGameMode()
    {
        if (!IsLobbyHost()) return;
        try
        {
            joinedLobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public,gameModeTxt.text) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            LobbyScreen();
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby= null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void KickPlayer(int index)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[index].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void MigrateLobbyHost(int index)
    {
        try
        {
            joinedLobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[index].Id,
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,playerName) },
                        {"PlayerColor",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,"Brown") },
                        {"PlayerModel",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,"Human") }

                        //{"PlayerAlReady",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,"false") }
                    }
        };
    }

    public void NewPlayerName()
    {
        if (playerNameTF.text.Length == 0) return;
        playerNameTF.text = playerNameTF.text.Substring(0, (int)Mathf.Min(10, playerNameTF.text.Length));
        playerName = playerNameTF.text;
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in "+lobby.Name);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " "+ player.Data["PlayerName"].Value);
        }
    }

/*    public async void PlayerReady()
    {
        try
        {
            UpdatePlayerOptions options = new UpdatePlayerOptions();

            options.Data = new Dictionary<string, PlayerDataObject>()
            {
                  {"PlayerAlReady",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,"true") }
            };

            string playerId = AuthenticationService.Instance.PlayerId;

            joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }*/

    public async void UpdateColorPlayer(string _playerColor)
    {
        try
        {
            UpdatePlayerOptions options = new UpdatePlayerOptions();

            options.Data = new Dictionary<string, PlayerDataObject>()
            {
                  {"PlayerColor",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,_playerColor) }
            };

            string playerId = AuthenticationService.Instance.PlayerId;

            joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, options);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public int GetIndexColor(string _playerColor)
    {
        switch (_playerColor)
        {
            case "Brown":
                return 0;
            case "Green":
                return 1;
            case "Pink":
                return 2;
            case "Blue":
                return 3;
            default:
                return 0;
        }
    }

    public void UpdatePlayerLobby()
    {
        int index = 0;

        foreach (Transform childTransform in listPlayer)
        {
            GameObject child = childTransform.gameObject;
            Destroy(child);
        }

        foreach (Player player in joinedLobby.Players)
        {
            GameObject newInforPlayer = Instantiate(informationPlayer, listPlayer);

            InforPlayer inforPlayer = newInforPlayer.GetComponent<InforPlayer>();

            inforPlayer.index = index;
            index++;

            int color = GetIndexColor(player.Data["PlayerColor"].Value);

            inforPlayer.SetInformation(player.Data["PlayerName"].Value, color);
        }
    }

    private void GetIDLobby(Lobby lobby)
    {
        IDLobbyTF.text = lobby.Id;
    }

    private bool IsLobbyHost()
    {
        return joinedLobby !=null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private bool IsPlayerInLobby()
    {
        if (joinedLobby !=null && joinedLobby.Players != null)
        {
            foreach(Player player in joinedLobby.Players)
            {
                if(player.Id == AuthenticationService.Instance.PlayerId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public async void StartGame()
    {
        if (!IsLobbyHost())return;
        try
        {
            Debug.Log(("Start Game"));

            string relayCode = await TestRelay.Instance.CreateRelay();

            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {"StartGame",new DataObject(DataObject.VisibilityOptions.Member,relayCode) }
                }
            });

            joinedLobby = lobby;
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    /*private void PreStart()
    {
        if (joinedLobby == null) return;
        foreach (Player player in joinedLobby.Players)
        {
            if (!player.Data["PlayerAlReady"].Value.Equals("true")) return;
        }
        if (IsLobbyHost())
        {
            NetworkManager.Singleton.StartHost();
        }
        if (IsPlayerInLobby())
        {
            NetworkManager.Singleton.StartClient();
        }
    }*/

    #endregion

    public void ListLobbyScreen()
    {
        if (mainMenuScreen.activeInHierarchy)
        {
            mainMenuScreen.SetActive(false);
            listLobbyScreen.SetActive(true);
            ListLobby();
        }
        else
        {
            mainMenuScreen.SetActive(true);
            listLobbyScreen.SetActive(false);
        }
    }

    public void HostScreen()
    {
        hostScreen.SetActive(true);
    }

    public void LobbyScreen()
    {
        if (listLobbyScreen.activeInHierarchy)
        {
            lobbyScreen.SetActive(true);
            hostScreen.SetActive(false);
            listLobbyScreen.SetActive(false);
            UpdatePlayerLobby();

        }
        else
        {
            lobbyScreen.SetActive(false);
            hostScreen.SetActive(true);
            listLobbyScreen.SetActive(true);
        }
    }

    private void GameStarted()
    {
        gameStarted = GameObject.FindGameObjectWithTag("GameStarted");
        if( gameStarted == null || alStarted==true) return;
        if (IsLobbyHost())
        {
            NetworkManager.Singleton.StartHost();
        }
        if (IsPlayerInLobby()&& !IsLobbyHost())
        {
            TestRelay.Instance.JoinRelay(joinedLobby.Data["StartGame"].Value);
        }
        alStarted = true;
    }

    private void StartScene()
    {
        SceneManager.LoadScene("Scenes/SampleScene");
        lobbyScreen.SetActive(false);
        backGround.SetActive(false);
    }

}
