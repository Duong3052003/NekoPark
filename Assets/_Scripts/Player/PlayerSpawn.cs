using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawn : NetworkBehaviour
{
    public GameObject cameraFocus;
    public CinemachineTargetGroup targetGroup;

    public override void OnNetworkSpawn()
    {
        PlayerManager.Instance.players.Add(this.gameObject);
        FindAndAddTargetGroup();

        //if (!IsHost) return;
        /*if (IsHost && SceneManager.GetActiveScene().name != "SampleScene")
        {
            ulong _idOwner = 0;
            for (int i = 0; i < PlayerManager.Instance.playerControl.Count; i++)
            {
                Vector3 newPos = PlayerManager.Instance.playerControl[(int)_idOwner].transform.position;
                PlayerManager.Instance.SetPositionPlayersClientRpc((int)_idOwner, newPos);
                _idOwner++;
            }
        }*/

        //PlayerManager.Instance.ResetPositionAllPlayers();
    }

    private void OnEnable()
    {
        if (targetGroup == null) return;
        AddPlayersToTargetGroup();
    }

    private void OnDisable()
    {
        targetGroup=null;
    }

    private void FindAndAddTargetGroup()
    {
        try
        {
            targetGroup = GameObject.Find("TargetGroupPlayer").GetComponent<CinemachineTargetGroup>();
        }
        catch
        {
            Debug.Log("Khong co targetGroupPlayer trong Scene nay");
        }

        if (targetGroup == null) return;
        Debug.Log("Phat hien co targetGroupPlayer trong Scene nay");
        AddPlayersToTargetGroup();
    }

    private void AddPlayersToTargetGroup()
    {
        foreach (GameObject player in PlayerManager.Instance.players)
        {
            GameObject _cameraFocus = player.GetComponent<PlayerCtrl>().playerSpawn.cameraFocus;
            targetGroup.AddMember(_cameraFocus.transform, 5f, 5f);
        }
    }
}
