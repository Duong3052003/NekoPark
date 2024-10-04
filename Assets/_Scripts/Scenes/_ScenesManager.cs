using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Netcode.NetworkSceneManager;

public class _ScenesManager : NetworkBehaviour
{
    public static _ScenesManager Instance { get; private set; }
    public string startScene = "SampleScene";

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

        PlayerManager.Instance.SetupPlayersClientRPC();
        //PlayerManager.Instance.RefreshPlayersClientRpc();

        NetworkTimer.Instance.OnPauseServerRpc(5);
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
}
