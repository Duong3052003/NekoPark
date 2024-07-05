using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.TextCore.Text;
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

    public override void OnNetworkSpawn()
    {
        if (!IsHost) return;

        SetActivePlayersServerRPC();
        GenerateBrickServerRPC();

        /*generateMapBtn.onClick.AddListener(() =>
        {
            GenerateMapServerRpc();
        });*/
    }

    [ServerRpc(RequireOwnership =false)]
    private void GenerateBrickServerRPC()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                GameObject brick = ObjIsSpawned();
                
                if (brick.GetComponent<NetworkObject>() != null && !brick.GetComponent<NetworkObject>().IsSpawned)
                {
                    brick.GetComponent<NetworkObject>().Spawn();
                }
                else
                {
                    brick.SetActive(true);
                }

                brick.transform.position = transform.position + new Vector3((float)((size.x - 1) * 0.5f - i) * offset.x, j * offset.y, 0);
                brick.GetComponent<SpriteRenderer>().color = gradient.Evaluate((float)j / (size.y - 1));
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetActivePlayersServerRPC()
    {
        GameObject[] newItems = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().items;
        GameObject[] newItems2 = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().items2;
        float[] newItemsTransformX = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().itemsTransformX;
        float[] newItemsTransformY = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().itemsTransformY;
        ulong indexId=0;

        for (int i = 0; i < PlayerManager.Instance.players.Count; i++)
        {
            indexId++;
            PlayerManager.Instance.SetPositionPlayersServerRpc(i, transformPlayers[i].position);

            if (newItems[i] == null) return;
            GameObject newItem = Instantiate(newItems[i]);
            newItem.transform.position = new Vector2(transformPlayers[i].position.x + newItemsTransformX[i], transformPlayers[i].position.y + newItemsTransformY[i]);
            newItem.GetComponent<NetworkObject>().SpawnWithOwnership(indexId);
            Debug.Log("Quyen so huu ball " + newItem.GetComponent<NetworkObject>().OwnerClientId);

            if (newItems2[i] == null) return;
            GameObject newItem2 = Instantiate(newItems2[i]);
            newItem2.transform.position = new Vector2(transformPlayers[i].position.x, transformPlayers[i].position.y + 1);
            newItem2.GetComponent<NetworkObject>().SpawnWithOwnership(indexId);
            newItem2.transform.SetParent(PlayerManager.Instance.players[i].transform);
        }
    }

    [ServerRpc]
    public void GenerateMapServerRpc()
    {
        SetActivePlayersServerRPC();
        GenerateBrickServerRPC();
    }
}
