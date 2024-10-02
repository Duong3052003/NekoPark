using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletSpawn : Spawner, IObserver
{
    [ClientRpc]
    protected void SpawnObjClientRpc(Vector2 position, Vector2 newTarget)
    {
        GameObject objSpawned = ObjIsSpawned();
        objSpawned.transform.parent = holder.transform;
        objSpawned.GetComponent<Bullet>().SetTarget(position, newTarget, this);
    }

    public void SpawnObj(GameObject bullet, Vector2 position, Vector2 head)
    {
        if (!IsHost) return;
        prefab = bullet;
        SpawnObjClientRpc(position, head);
        Debug.Log("Spawn");
    }

    private void OnEnable()
    {
        AddListObserver(this);
    }

    private void OnDisable()
    {
        RemoveListObserver(this);
    }

    public void AddListObserver(IObserver observer)
    {
        NetworkTimer.Instance.AddListObserver(observer);
    }

    public void RemoveListObserver(IObserver observer)
    {
        NetworkTimer.Instance.RemoveListObserver(observer);
    }

    public void OnPause(int time)
    {
        
    }

    public void OnResume()
    {
        
    }
}
