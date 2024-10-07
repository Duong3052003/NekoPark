using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Netcode.NetworkSceneManager;

public class _ScenesManager : NetworkBehaviour
{
    public static _ScenesManager Instance { get; private set; }
    public string startScene = "SampleScene";
    public NetworkVariable<int> numberPlayer = new NetworkVariable<int>(
       0,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server);

    private List<ISceneObserver> listObserver = new List<ISceneObserver>();

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

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoadedLocal;
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            Debug.Log("dang ki su kien network");
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log("LoadScene");
        StartCoroutine(LoadLevel(sceneName));

        //SetPlayersStatic
        PlayerManager.Instance.SetBodyTypeAllPlayersServerRpc(2);
    }

    private IEnumerator LoadLevel(string sceneName)
    {
        //transition
        SubscribeHandleOnSceneEventClientRpc(true);
        yield return new WaitForSeconds(2);
        if (IsHost)
        {
            PlayerManager.Instance.SetPlayerControlClientRpc(false);
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        //transition
    }

    public void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log("LoadSceneCompleted");
        SubscribeHandleOnSceneEventClientRpc(false);
        PlayerManager.Instance.SetupPlayersClientRPC();
        //PlayerManager.Instance.RefreshPlayersClientRpc();
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            if (IsHost)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            }
        }
        SceneManager.sceneLoaded -= OnSceneLoadedLocal;
    }

    public void LoadSceneLocal(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

    private void OnSceneLoadedLocal(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single && scene.name == startScene)
        {
            Debug.Log("Scene loaded successfully: " + startScene);
            UIManager.Instance.StartConnect();
        }
    }

    /*public void loadLevelBtn(string lvToLoad)
    {
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);

        StartCoroutine(LoadLevelAsync(lvToLoad));
    }*/


    [ClientRpc]
    public void SubscribeHandleOnSceneEventClientRpc(bool boolen)
    {
        if (boolen)
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += HandleOnSceneEvent;
        }
        else
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= HandleOnSceneEvent;
        }
    }

    private async void HandleOnSceneEvent(SceneEvent sceneEvent)
    {
        if (NetworkManager.Singleton.LocalClientId != sceneEvent.ClientId)
        {
            return;
        }

        if (sceneEvent.SceneEventType == SceneEventType.Load)
        {
            UIManager.Instance.Display();
            while (!sceneEvent.AsyncOperation.isDone)
            {
                await Task.Yield();
                UIManager.Instance.UpdateProgress(sceneEvent.AsyncOperation.progress);
            }
        }

        if (sceneEvent.SceneEventType == SceneEventType.LoadComplete)
        {
            StartCoroutine(EndLoadingScreen(3f));
        }
    }

    IEnumerator EndLoadingScreen(float _time)
    {
        yield return new WaitForSeconds(_time);
        PlayerLoadDoneServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerLoadDoneServerRpc(ServerRpcParams serverRpcParams = default)
    {
        numberPlayer.Value++;
        Debug.Log("Player " +serverRpcParams.Receive.SenderClientId+" da load xong | "+ numberPlayer.Value +"/" + NetworkManager.Singleton.ConnectedClientsList.Count);
        if (numberPlayer.Value >= NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            UIManager.Instance.Hide();
            ResetPlayerLoadDoneServerRpc();
            OnLoadDoneServerRpc();
            OnPauseServerRpc(5);
        }
    }

    [ServerRpc]
    private void ResetPlayerLoadDoneServerRpc()
    {
        numberPlayer.Value =0;
    }

    public void AddListObserver(ISceneObserver observer)
    {
        listObserver.Add(observer);
    }

    public void RemoveListObserver(ISceneObserver observer)
    {
        listObserver.Remove(observer);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnLoadDoneServerRpc()
    {
        listObserver.ForEach(observer => observer.OnLoadDone());
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnPauseServerRpc(int time)
    {
        listObserver.ForEach(observer => observer.OnPause(time));

        Invoke(nameof(OnResumeServerRpc), time);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnResumeServerRpc()
    {
        CancelInvoke();
        for (int i = 0; i < listObserver.Count; i++)
        {
            listObserver[i].OnResume();
        }
    }
}
