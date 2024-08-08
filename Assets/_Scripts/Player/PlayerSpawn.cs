using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn : NetworkBehaviour
{
    public GameObject cameraFocus;
    [SerializeField] private float posRanged = 5f;
    public CinemachineTargetGroup targetGroup;

    private void OnEnable()
    {
        if (targetGroup == null) return;
        AddPlayersToTargetGroup();
    }

    private void OnDisable()
    {
        targetGroup=null;
    }

    public override void OnNetworkSpawn()
    {
        UpdatePosServerRpc();
        FindAndAddTargetGroup();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePosServerRpc()
    {
        transform.position = new Vector2(Random.Range(-posRanged, posRanged), 1);
        transform.rotation = new Quaternion(0, 0, 0, 0);
    }

    private void FindAndAddTargetGroup()
    {
        try
        {
            Debug.Log("Phat hien co targetGroupPlayer trong Scene nay");
            targetGroup = GameObject.Find("TargetGroupPlayer").GetComponent<CinemachineTargetGroup>();
        }
        catch
        {
            Debug.Log("Khong co targetGroupPlayer trong Scene nay");
        }

        if (targetGroup == null) return;
        AddPlayersToTargetGroup();
    }

    void AddPlayersToTargetGroup()
    {
        foreach (GameObject player in PlayerManager.Instance.players)
        {
            GameObject _cameraFocus = player.GetComponent<PlayerCtrl>().playerSpawn.cameraFocus;
            targetGroup.AddMember(_cameraFocus.transform, 5f, 5f);
        }
    }
}
