using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class LevelGenerator : Spawner,IObserver
{
    public static LevelGenerator Instance { get; private set; }
    
    [SerializeField] private GameObject[] objSpawnedHolders;

    [SerializeField] private Vector2Int size;
    [SerializeField] private Vector2 offset;

    [SerializeField] private List<Transform> transformPlayers;
    [SerializeField] private Button generateMapBtn;

    private GameObject objSpawned;
    private int cdRespam;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost) return;

        SetActivePlayersServerRPC();

        /*generateMapBtn.onClick.AddListener(() =>
        {
            GenerateMapServerRpc();
        });*/
    }

    private void GenerateObj(int numRow)
    {
        for (int k = 0; k < objSpawnedHolders.Length; k++)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < numRow; j++)
                {
                    GenerateObjsServerRPC(k,i,j);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void GenerateObjsServerRPC(int k, int i, int j)
    {
        objSpawned = ObjIsSpawned();

        objSpawned.transform.position = objSpawnedHolders[k].transform.position + new Vector3((float)((size.x - 1) * 0.5f - i) * offset.x, j * offset.y, 0);
        
        objSpawned.transform.SetParent(objSpawnedHolders[k].transform);

        objSpawned.GetComponent<ObjDeSpawnByHp>().SettingObjIfAlreadyInScene((float)j / (size.y - 1));
        objSpawned.GetComponent<ObjDeSpawnByHp>().SetHp(3*(j+1));
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetActivePlayersServerRPC()
    {
        for (int i = 0; i < PlayerManager.Instance.playerControl.Count; i++)
        {
            PlayerManager.Instance.SetPositionPlayersControlClientRpc(i, transformPlayers[i].position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetUpLevelServerRPC()
    {
        LevelStorage levelStorage = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>();

        ulong indexId = 0;

        for (int i = 0; i < PlayerManager.Instance.players.Count; i++)
        {
            for (int j = 0; j < levelStorage.items.Length; j++)
            {
                if (levelStorage.items[j] == null) return;
                GameObject newItem = Instantiate(levelStorage.items[j].gameObject);

                if (levelStorage.isHost[j] == true)
                {
                    newItem.GetComponent<NetworkObject>().SpawnWithOwnership(0);
                }
                else
                {
                    newItem.GetComponent<NetworkObject>().SpawnWithOwnership(indexId);
                }

                if (levelStorage.isParent[j] == true)
                {
                    newItem.transform.SetParent(PlayerManager.Instance.players[i].transform);
                }

                newItem.GetComponent<IObjectServerSpawn>().Spawn(new Vector2(transformPlayers[i].position.x + levelStorage.itemsTransformX[j], transformPlayers[i].position.y + levelStorage.itemsTransformY[j]),Vector2.zero);
            }

            indexId++;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnObjServerRpc(int indexItem,ulong idOwner, Vector2 posSpawn, Vector2 velocityVector)
    {
        LevelStorage levelStorage = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>();
        if (levelStorage.items[indexItem] == null) return;
        GameObject newItem = Instantiate(levelStorage.items[indexItem].gameObject);
        newItem.GetComponent<NetworkObject>().SpawnWithOwnership(idOwner);
        newItem.GetComponent<IObjectServerSpawn>().Spawn(posSpawn, velocityVector);
    }

    public List<GameObject> CallAllBallsOfClient(ulong clientId)
    {
        GameObject[] allBalls = GameObject.FindGameObjectsWithTag("Bullet");

        List<GameObject> clientBalls = new List<GameObject>();

        foreach (GameObject ball in allBalls)
        {
            NetworkObject networkObject = ball.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.OwnerClientId == clientId)
            {
                clientBalls.Add(ball);
            }
        }

        return clientBalls;
    }

    [ServerRpc]
    public void GenerateMapServerRpc()
    {
        SetActivePlayersServerRPC();
        GenerateObj(size.y);
    }

    private void OnEnable()
    {
        AddListObserver(this);
    }

    private void OnDisable()
    {
        RemoveListObserver(this);
    }

    public void AddListObserver(IObserver observer)
    {
        NetworkTimer.Instance.AddListObserver(observer);
    }

    public void RemoveListObserver(IObserver observer)
    {
        NetworkTimer.Instance.RemoveListObserver(observer);
    }

    public void OnPause(int time)
    {
    }

    public void OnResume()
    {
        GenerateObj(size.y);
        if (!IsHost) return;
        SetUpLevelServerRPC();
    }
}
