using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ParentNetwork : NetworkBehaviour
{
   /* public override void OnNetworkDespawn()
    {
        DestroyChildServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]    
    private void DestroyChildServerRpc()
    {
        foreach (Transform child in this.transform)
        {
            var childNetwork = child.GetComponent<NetworkObject>();
            if (childNetwork != null && childNetwork.IsSpawned)
            {
                Debug.Log(child.gameObject);
                childNetwork.transform.SetParent(null);
            }
        }
        base.OnNetworkDespawn();
    }*/
}
