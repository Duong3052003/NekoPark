using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class LevelGenerator : Spawner,ISceneObserver
{
    public static LevelGenerator levelGenerator { get; private set; }

    [SerializeField] protected GameObject[] objSpawnedHolders;

    [SerializeField] protected Vector2Int size;
    [SerializeField] protected Vector2 offset;

    [SerializeField] protected List<Transform> transformPlayers;
    [SerializeField] protected Button generateMapBtn;

    [SerializeField] protected bool mupltiHP=true;
    [SerializeField] protected int hpObj;

    protected GameObject objSpawned;

    protected virtual void Awake()
    {
        if (levelGenerator == null)
        {
            levelGenerator = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost) return;

        SetActivePlayerControlServerRPC();

        /*generateMapBtn.onClick.AddListener(() =>
        {
            GenerateMapServerRpc();
        });*/
    }

    protected virtual void GenerateObj(int numRow)
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
    protected virtual void GenerateObjsServerRPC(int k, int i, int j)
    {
        objSpawned = ObjIsSpawned();

        objSpawned.transform.position = objSpawnedHolders[k].transform.position + new Vector3((float)((size.x - 1) * 0.5f - i) * offset.x, j * offset.y, 0);
        
        objSpawned.transform.SetParent(objSpawnedHolders[k].transform);

        objSpawned.GetComponent<ObjDeSpawnByHp>().SettingObjIfAlreadyInScene((float)j / (size.y - 1));

        if(mupltiHP== true)
        {
            hpObj = 3 * (j + 1);
        }
        objSpawned.GetComponent<ObjDeSpawnByHp>().SetHp(hpObj);
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SetActivePlayerControlServerRPC()
    {
        PlayerManager.Instance.SetPlayerControlClientRpc(true);
        ulong _idOwner = 0;
        for (int i = 0; i < PlayerManager.Instance.playerControl.Count; i++)
        {
            PlayerManager.Instance.SetPositionPlayersControlClientRpc(i, transformPlayers[i].position);
            PlayerManager.Instance.playerControl[i].GetComponent<PlayerGetModel>().GetModelServerRpc(_idOwner);
            _idOwner++;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SetActivePlayersServerRPC()
    {
        for (int i = 0; i < PlayerManager.Instance.players.Count; i++)
        {
            PlayerManager.Instance.SetPositionPlayersClientRpc(i, transformPlayers[i].position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected virtual void SetUpLevelServerRPC()
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
    public virtual void SpawnObjServerRpc(int indexItem,ulong idOwner, Vector2 posSpawn, Vector2 velocityVector)
    {
        LevelStorage levelStorage = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>();
        if (levelStorage.items[indexItem] == null) return;
        GameObject newItem = Instantiate(levelStorage.items[indexItem].gameObject);
        newItem.GetComponent<NetworkObject>().SpawnWithOwnership(idOwner);
        newItem.GetComponent<IObjectServerSpawn>().Spawn(posSpawn, velocityVector);
    }

    [ServerRpc]
    public virtual void GenerateMapServerRpc()
    {
        SetActivePlayerControlServerRPC();
        GenerateObj(size.y);
    }

    protected virtual void OnEnable()
    {
        AddListObserver(this);
    }

    protected virtual void OnDisable()
    {
        RemoveListObserver(this);
    }

    public virtual void AddListObserver(ISceneObserver observer)
    {
        _ScenesManager.Instance.AddListObserver(observer);
    }

    public virtual void RemoveListObserver(ISceneObserver observer)
    {
        _ScenesManager.Instance.RemoveListObserver(observer);
    }

    public virtual void OnPause(int time)
    {
        
    }

    public virtual void OnResume()
    {
        GenerateObj(size.y);
        if (!IsHost) return;
        SetUpLevelServerRPC();
        PlayerManager.Instance.GetSettingStatusPlayer();
    }

    public void OnLoadDone()
    {
        if (!IsHost) return;
        SetActivePlayersServerRPC();
    }
}
