using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerGetModel : NetworkBehaviour
{
    private GameObject playerModel;

    public override void OnNetworkSpawn()
    {
        playerModel = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().playerModel;
        if (!IsOwner) return;
        GetModelServerRpc();
    }

    [ServerRpc]
    private void GetModelServerRpc(ServerRpcParams rpcParams = default)
    {
        GameObject playerInstance = Instantiate(playerModel);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        //playerInstance.transform.SetParent(this.transform);
    }
}
