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
    
    [SerializeField] private GameObject[] brickHolders;

    [SerializeField] private Vector2Int size;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Gradient gradient;

    [SerializeField] private List<Transform> transformPlayers;
    [SerializeField] private Button generateMapBtn;

    private GameObject brick;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
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

    private void GenerateBricks()
    {
        for (int k = 0; k < brickHolders.Length; k++)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    GenerateBrickServerRPC(k,i,j);

                    brick.GetComponent<SpriteRenderer>().color = gradient.Evaluate((float)j / (size.y - 1));
                }
            }
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void GenerateBrickServerRPC(int k, int i, int j)
    {
        brick = ObjIsSpawned();

        brick.transform.position = brickHolders[k].transform.position + new Vector3((float)((size.x - 1) * 0.5f - i) * offset.x, j * offset.y, 0);

        brick.transform.SetParent(brickHolders[k].transform);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetActivePlayersServerRPC()
    {
        for (int i = 0; i < PlayerManager.Instance.players.Count; i++)
        {
            PlayerManager.Instance.SetPositionPlayersClientRpc(i, transformPlayers[i].position);
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

                newItem.transform.position = new Vector2(transformPlayers[i].position.x + levelStorage.itemsTransformX[j], transformPlayers[i].position.y + levelStorage.itemsTransformY[j]);

            }

            indexId++;
        }
    }

    [ServerRpc]
    public void GenerateMapServerRpc()
    {
        SetActivePlayersServerRPC();
        GenerateBricks();
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
        GenerateBricks();
        if (!IsHost) return;
        SetUpLevelServerRPC();
    }
}
