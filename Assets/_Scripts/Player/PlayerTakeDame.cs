using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTakeDame : PlayerHp
{
    public override void TakeDamaged(int damage)
    {
        if (!IsOwner) return;
        hpCurrent.Value = hpCurrent.Value - damage;
    }

    protected override void Despawn()
    {
        playerCtrl.SetActivePlayer(false);
        playerCtrl.playerAnimator.Desappear();
    }

    protected override void UpdateHpBar(float _hpCurrent)
    {
        StartCoroutine(playerCtrl.playerAnimator.Invisible(3f));

        hpBar.value = _hpCurrent / hpMax;

        if (hpCurrent.Value > 0) return;
        Despawn();
    }
}
