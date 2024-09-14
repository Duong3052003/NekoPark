using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEngine;

public class ItemDrop : Spawner
{
    [SerializeField] protected List<GameObject> listObj;
    [SerializeField] protected float rateDrop;

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
        if(!CheckRateDropItem()) return;
        prefab = RandomGameObjectFromList.GetRandomGameObject(listObj);
    }

    bool CheckRateDropItem()
    {
        int randomValue = Random.Range(0, 100);

        if (randomValue < rateDrop)
        {
            return true;
        }

        return false;
    }
}
