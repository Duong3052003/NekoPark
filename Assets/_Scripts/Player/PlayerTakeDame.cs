using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTakeDame : PlayerHp
{
    public override void TakeDamaged(int damage)
    {
        if (!IsOwner) return;
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
        playerCtrl.playerAnimator.Desappear();
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
}
