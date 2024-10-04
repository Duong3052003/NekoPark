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
        if(!IsOwner) return;
        GetModelServerRpc(OwnerClientId);
    }

   /* private void OnEnable()
    {
        GetModelFromStorage();
    }*/

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
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetModelServerRpc(ulong _idOwner)
    {
        GetModelFromStorage();
        playerInstance = Instantiate(playerModel);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(_idOwner);
        playerInstance.transform.SetParent(this.transform);
        //SetPositionModelClientRpc();
    }

    /*[ClientRpc]
    private void SetPositionModelClientRpc()
    {
        if (playerInstance == null) return;
        playerInstance.GetComponent<PlayerMove>().SetPositionNetworkVariable(this.transform.position);
    }*/
}
