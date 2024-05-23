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
            Destroy(Instance);
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
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log("LoadScene");
        PlayerManager.Instance.SetActiveAllPlayers(false);
        StartCoroutine(LoadLevel(sceneName));
        
    }
    private IEnumerator LoadLevel(string sceneName)
    {
        //transition
        yield return new WaitForSeconds(2);
        if (IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        //transition
    }

    public void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log("LoadSceneCompleted");

        if (IsHost && SceneManager.GetActiveScene().name == sceneName)
        {
            PlayerManager.Instance.SetActiveAllPlayers(true);
            PlayerManager.Instance.SetPositionAllPlayers(new Vector3(0,2,0));
        }
    }

    public override void OnDestroy()
    {
        if (IsHost)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
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
