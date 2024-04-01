using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn : NetworkBehaviour
{
    [SerializeField] private float posRanged = 5f;

    public override void OnNetworkSpawn()
    {
        UpdatePosServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePosServerRpc()
    {
        transform.position = new Vector2(Random.Range(-posRanged, posRanged), 1);
        transform.rotation = new Quaternion(0, 180, 0, 0);
    }
}
