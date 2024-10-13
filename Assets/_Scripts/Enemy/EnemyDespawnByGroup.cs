using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyDespawnByGroup : EnemyDespawn
{
    public override void TakeDamaged(int damage)
    {
        if (!IsOwner) return;
        TakeDamagedServerRpc(damage);
        if(!IsHost) return;
        SendDameGroupServerRpc(-damage);
    }

    [ServerRpc]
    private void SendDameGroupServerRpc(float dame)
    {
        EnemySnake.Instance.ChangeHpServerRpc(dame);
    }

    public override void CallDespawn()
    {
        if (beingDetroyed == true) return;
        SendDameGroupServerRpc(-hpCurrent.Value);
        beingDetroyed = true;
        col.enabled = false;
        animator.SetTrigger("expl");
    }
}
