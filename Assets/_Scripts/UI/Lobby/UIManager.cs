using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;
using static Cinemachine.DocumentationSortingAttribute;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }

    public Lobby joinedLobby;

    private float heartBeatTimer;
    private float lobbyUpdatetimer;

    private string playerName;

    private string nameScene = "Scenes/SampleScene";

    [Header("Menu screens")]
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject backGroundmainMenuScreen;
    [SerializeField] private GameObject listLobbyScreen;
    [SerializeField] private GameObject hostBtnScreen;
    [SerializeField] private GameObject settingLobbyScreen;
    [SerializeField] private GameObject choiceLevelScreen;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject gameOverScreenClient;
    [SerializeField] private GameObject loadingScreen;

    [Header("List Lobby")]
    [SerializeField] private GameObject lobbyGameObj;
    [SerializeField] private Transform listLobby;

    [Header("Host Lobby")]
    [SerializeField] private TextMeshProUGUI isPrivateTxt;

    [Header("Lobby")]
    [SerializeField] private GameObject informationPlayer;
    [SerializeField] private Transform listPlayer;

    [Header("Slider")]
    [SerializeField] private GameObject loadingSliderObj;
    [SerializeField] private Slider loadingSlider;

    [Header("Others")]
    [SerializeField] private GameObject backGround;
    [SerializeField] private TextMeshProUGUI gameModeTxt;
    [SerializeField] private TMP_InputField lobbyCodeTF;
    [SerializeField] private TMP_InputField playerNameTF;
    [SerializeField] private TMP_InputField IDLobbyTF;
    public GameObject inLobby;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        /*if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }*/
    }

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (AuthenticationService.Instance.IsSignedIn) return;

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Sign in: " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            playerName = "Player" + UnityEngine.Random.Range(10, 100);
            playerNameTF.text = playerName;

            Debug.Log(playerName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error: " + ex.Message);

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdate();
        HandleInput();
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
            }
        }
    }

    public async void CreateLobby(string _nameScene)
    {
        try
        {
            nameScene = _nameScene;
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
            string relayCode = await TestRelay.Instance.CreateRelay();


            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = _isPrivate,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode",new DataObject(DataObject.VisibilityOptions.Public,gameModeTxt.text) },
                    {"StartGame",new DataObject(DataObject.VisibilityOptions.Member,relayCode) },
                    {"Scene",new DataObject(DataObject.VisibilityOptions.Member,nameScene) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(playerNameTF.text, maxPlayers, createLobbyOptions);

            joinedLobby = lobby;

            Debug.Log("Created Lobby! " + playerNameTF.text + " " + maxPlayers + " " + lobby.Data["GameMode"].Value + " " + lobby.IsPrivate + " " + lobby.Id + " " + lobby.LobbyCode);

            StartSceneLobby();
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

            StartSceneLobby();
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
            StartSceneLobby();
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

    public Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,playerName) },
                        {"PlayerColor",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,"Brown") },
                        {"PlayerModel",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,"Human") }
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

    public async void UpdateColorPlayer(int _playerColor)
    {
        try
        {
            string colorString = GetStringColor(_playerColor);

            UpdatePlayerOptions options = new UpdatePlayerOptions();

            options.Data = new Dictionary<string, PlayerDataObject>()
            {
                  {"PlayerColor",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,colorString) }
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

    public string GetStringColor(int color)
    {
        switch (color)
        {
            case 0:
                return "Brown";
            case 1:
                return "Green";
            case 2:
                return "Pink";
            case 3:
                return "Blue";
            default:
                return "Brown";
        }
    }

    /*public void UpdatePlayerLobby()
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
            Debug.Log(player.Data["PlayerName"].Value + color);
        }
    }*/

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

    private void StartSceneLobby()
    {
        if (nameScene.Equals("test")) return;
        _ScenesManager.Instance.LoadSceneLocal("_Scenes/SampleScene");

        listLobbyScreen.SetActive(false);
        settingLobbyScreen.SetActive(false);
        backGround.SetActive(false);
    }

    public void StartLevel()
    {
        _ScenesManager.Instance.LoadScene(joinedLobby.Data["Scene"].Value);
    }

    public void StartConnect()
    {
        if (IsLobbyHost())
        {
            NetworkManager.Singleton.StartHost();
        }
        if (IsPlayerInLobby() && !IsLobbyHost())
        {
            TestRelay.Instance.JoinRelay(joinedLobby.Data["StartGame"].Value);
        }
    }
    #endregion

    #region buttonScreen
    public void ListLobbyScreen()
    {
        if (mainMenuScreen.activeInHierarchy)
        {
            backGroundmainMenuScreen.SetActive(false);
            mainMenuScreen.SetActive(false);
            listLobbyScreen.SetActive(true);
            ListLobby();
        }
        else
        {
            backGroundmainMenuScreen.SetActive(true);
            mainMenuScreen.SetActive(true);
            listLobbyScreen.SetActive(false);
        }
    }

    public void HostBtnScreen()
    {
        hostBtnScreen.SetActive(true);
    }

    public void SettingLobbyScreen()
    {
        if (listLobbyScreen.activeInHierarchy)
        {
            settingLobbyScreen.SetActive(true);
            hostBtnScreen.SetActive(false);
            listLobbyScreen.SetActive(false);
        }
        else
        {
            settingLobbyScreen.SetActive(false);
            hostBtnScreen.SetActive(true);
            listLobbyScreen.SetActive(true);
        }
    }

    public void ChoiceLevelScreen()
    {
        if (choiceLevelScreen.activeInHierarchy)
        {
            choiceLevelScreen.SetActive(false);
            settingLobbyScreen.SetActive(true);
        }
        else
        {
            settingLobbyScreen.SetActive(false);
            choiceLevelScreen.SetActive(true);
        }
    }

    public void GameOverScreen()
    {
        if (gameOverScreen.activeInHierarchy) return;
        GameOverScreenServerRpc(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void GameOverScreenServerRpc(bool boolen)
    {
        GameOverScreenClientRpc(boolen);
    }

    [ClientRpc]
    private void GameOverScreenClientRpc(bool boolen)
    {
        if (IsHost)
        {
            gameOverScreen.SetActive(boolen);
        }
        else
        {
            gameOverScreenClient.SetActive(boolen);
        }
    }
    #endregion

    #region SettingMenuGame
    public void Restart()
    {
        _ScenesManager.Instance.LoadScene(SceneManager.GetActiveScene().name);
        PlayerManager.Instance.DesAndSpawnAllPlayerServerRpc();
        foreach(var obj in FindAllGameObjectsWithIObject())
        {
            obj.GetComponent<IObjectServerSpawn>().DeSpawn();
        }

        GameOverScreenServerRpc(false);
    }

    public void Leave()
    {
        LeaveLobby();

        gameOverScreenClient.SetActive(false);
        gameOverScreen.SetActive(false);

        mainMenuScreen.SetActive(true);
        backGround.SetActive(true);

        PlayerManager.Instance.DestroyPlayerServerRpc();
        PlayerManager.Instance.RemoveAllPlayers();
        PlayerManager.Instance.RemoveAllPlayersControl();

        NetworkManager.Singleton.SceneManager.LoadScene("MainScreen", LoadSceneMode.Single);

        TestRelay.Instance.LeaveRelay();
        Destroy(this.gameObject);
    }

    private void HandleInput()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            GameOverScreen();
        }
    }
    #endregion

    #region LoadingScreen
    public void Display()
    {
        loadingScreen.SetActive(true);
        if(loadingSlider == null)
        {
            loadingSlider = loadingSliderObj.GetComponent<Slider>();
        }
    }

    public void UpdateProgress(float progress)
    {
        loadingSlider.value = progress;
    }

    public void Hide()
    {
        HideClientRpc();
    }

    [ClientRpc]
    private void HideClientRpc()
    {
        loadingScreen.SetActive(false);
    }
    #endregion

    public static List<GameObject> FindAllGameObjectsWithIObject()
    {
        List<GameObject> objectsWithIObject = new List<GameObject>();

        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            IObjectServerSpawn iObjectComponent = obj.GetComponent<IObjectServerSpawn>();

            if (iObjectComponent != null)
            {
                objectsWithIObject.Add(obj);
            }
        }

        return objectsWithIObject;
    }
}
