using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class DeSpawnByHp : DeSpawn
{
    protected NetworkVariable<float> hpCurrent = new NetworkVariable<float>(
       0,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Owner);

    protected override bool CanDespawn()
    {
        return hpCurrent.Value <= 0;
    }

    protected abstract override void Despawn();
}
