using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTakeDame : PlayerHp
{
    [SerializeField] private bool IMMORTAL = false;
    [SerializeField] private bool canRevive = false;
    [SerializeField] private GameObject reviveObj;

    public override void TakeDamaged(int damage)
    {
        if (!IsOwner || IMMORTAL) return;
        if (hpCurrent.Value==0 || hpCurrent.Value - damage > hpMax) return;
        hpCurrent.Value = hpCurrent.Value - damage;
    }

    protected override void Despawn()
    {
        playerCtrl.SetActivePlayer(false);
        foreach(Transform child in this.transform)
        {
            child.gameObject.SetActive(false);
        }
        if (IsHost)
        {
            SpawnReviveObjServerRpc();
        }
        playerCtrl.playerAnimator.Desappear();
    }

    [ClientRpc]
    public void ReviveClientRpc()
    {
        Debug.Log("Revive");
        this.gameObject.SetActive(true);
;       playerCtrl.SetActivePlayer(true);
        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(true);
        }

        if (!IsOwner) return;
        hpCurrent.Value = 1;
    }

    protected override void UpdateHpBar(float oldValue, float newValue)
    {
        if(newValue < oldValue)
        {
            StartCoroutine(playerCtrl.playerAnimator.Invisible(3f));
        }

        if (hpBar == null) return;
        hpBar.value = hpCurrent.Value / hpMax;

        if (hpCurrent.Value > 0) return;
        Despawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnReviveObjServerRpc()
    {
        Debug.Log("Spawn Revive");
        if (!canRevive) return;
        GameObject newObj = Instantiate(reviveObj);
        newObj.GetComponent<ReviveObj>().SetPlayerObj(this.gameObject);
        newObj.GetComponent<NetworkObject>().SpawnWithOwnership(0);
        newObj.transform.position = this.transform.position;
    }
}
