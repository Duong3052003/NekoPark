using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletSpawn : SpawmObjByTime, IObserver
{
    public static BulletSpawn Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    [ClientRpc]
    protected override void SpawnObjClientRpc(Vector2 position, Vector2 newTarget)
    {
        GameObject objSpawned = ObjIsSpawned();
        objSpawned.transform.parent = holder.transform;
        objSpawned.GetComponent<Bullet>().SetTarget(position, newTarget);
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
        canSpawn=false;
    }

    public void OnResume()
    {
        StartCoroutine(SpawnObject());
        canSpawn =true;
    }
}
