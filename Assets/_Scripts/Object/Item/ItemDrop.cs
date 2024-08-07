using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEngine;

public class ItemDrop : Spawner
{
    [SerializeField] protected List<GameObject> listObj;

    public virtual void DropItem()
    {
        if(!IsHost) return;
        GetRandomItem();
        DropItemServerRPC();
    }

    [ServerRpc(RequireOwnership =false)]
    private void DropItemServerRPC()
    {
        GameObject newItem = ObjIsSpawned();
        newItem.transform.position = this.transform.position;
    }

    protected virtual void GetRandomItem()
    {
        prefab = RandomGameObjectFromList.GetRandomGameObject(listObj);
    }
}
