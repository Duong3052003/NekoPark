using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UI;

public class LevelGenerator : Spawner
{
    public static LevelGenerator Instance { get; private set; }

    [SerializeField] private Vector2Int size;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Gradient gradient;


    [SerializeField] private List<Transform> transformPlayers;
    [SerializeField] private Button generateMapBtn;

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

    protected override void Start()
    {
        if(IsOwner)
        {
            GenerateMapServerRpc();
        }
        Debug.Log(1);
        generateMapBtn.onClick.AddListener(() =>
        {
            GenerateMapServerRpc();
        });
    }

    private void GenerateBrick()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                clone = ObjIsSpawned();
                clone.transform.position = transform.position + new Vector3((float)((size.x - 1) * 0.5f - i) * offset.x, j * offset.y, 0);
                clone.GetComponent<SpriteRenderer>().color = gradient.Evaluate((float)j / (size.y - 1));
                InstantiateServerRpc();
            }
        }
    }

    private void SetActivePlayers()
    {
        GameObject[] newItems = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().items;
        float[] newItemsTransformX = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().itemsTransformX;
        float[] newItemsTransformY = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().itemsTransformY;

        for (int k = 0; k < PlayerManager.Instance.players.Count; k++)
        {
            transformPlayers[k].gameObject.SetActive(true);

            if (newItems[k] == null) return;
            GameObject newItem = Instantiate(newItems[k]);
            newItem.transform.position = new Vector2(transformPlayers[k].position.x + newItemsTransformX[k], transformPlayers[k].position.y + newItemsTransformY[k]);
            newItem.GetComponent<NetworkObject>().Spawn();
        }
        PlayerManager.Instance.SetActiveAllPlayers(true);
        PlayerManager.Instance.SetPositionAllPlayers(transformPlayers);
    }

    [ServerRpc]
    public void GenerateMapServerRpc()
    {
        SetActivePlayers();
        GenerateBrick();
    }
}
