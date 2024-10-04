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
        var newItem = ObjIsSpawned();
        if (newItem == null) return;
        newItem.GetComponent<IObjectServerSpawn>().Spawn(this.transform.position,new Vector2(0,-1));
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
