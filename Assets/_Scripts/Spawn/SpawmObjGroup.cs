using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEngine;

public abstract class SpawmObjGroup : Spawner
{
    public bool canSpawn;
    
    [ClientRpc]
    protected virtual void SpawnObjClientRpc(Vector2 position, Vector2 newTarget)
    {
        GameObject objSpawned = ObjIsSpawned();
    }

    public abstract void SpawnObj();
}
