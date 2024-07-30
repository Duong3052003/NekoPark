using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class DeSpawn : NetworkBehaviour
{
    protected abstract bool CanDespawn();
    protected abstract void Despawn();
}
