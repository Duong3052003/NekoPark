using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerGetModel : NetworkBehaviour
{
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject playerInstance;

    private NetworkObject networkObj;

    private void Awake()
    {
        networkObj= GetComponent<NetworkObject>();
    }

    public override void OnNetworkSpawn()
    {
        PlayerManager.Instance.playerControl.Add(this.gameObject);
        GetModelFromStorage();
    }

    private void OnEnable()
    {
        GetModelFromStorage();
    }

    private void OnDisable()
    {
        playerModel= null;

        if (playerInstance == null || !playerInstance.GetComponent<NetworkObject>().IsSpawned) return;
        playerInstance.GetComponent<NetworkObject>().Despawn();
        Destroy(playerInstance);
    }
    
    private void GetModelFromStorage()
    {
        if (networkObj == null) return;
        playerModel = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().playerModel;
        if (!IsOwner) return;
        GetModelServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetModelServerRpc(ServerRpcParams rpcParams = default)
    {
        playerInstance = Instantiate(playerModel);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        //playerInstance.transform.SetParent(this.transform);
    }
}
