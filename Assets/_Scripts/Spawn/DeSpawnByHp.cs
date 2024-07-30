using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeSpawnByHp : DeSpawn
{
    protected NetworkVariable<int> hpCurrent = new NetworkVariable<int>(0);

    protected override bool CanDespawn()
    {
        return hpCurrent.Value <= 0;
    }

    protected override void Despawn()
    {

    }
}
