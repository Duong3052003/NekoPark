using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn : NetworkBehaviour
{
    [SerializeField] private float posRanged = 5f;
    private CinemachineTargetGroup targetGroup;

    private void Awake()
    {
        targetGroup = GameObject.Find("TargetGroupPlayer").GetComponent<CinemachineTargetGroup>();
    }

    private void Start()
    {
        AddPlayersToTargetGroup();
    }

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

    void AddPlayersToTargetGroup()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            targetGroup.AddMember(player.transform, 5f, 5f);
        }
    }
}
