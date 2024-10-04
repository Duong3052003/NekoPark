using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletSpawner : Spawner, IObserver
{
    public bool canSpawn;

    public Transform headTransform;
    public Transform positionTransform;
    public float spawnCD;

    private void Awake()
    {
        holder = GameObject.FindGameObjectWithTag("Holder").transform;
    }

    protected IEnumerator SpawnObjectCD()
    {
        while (canSpawn)
        {
            SpawnObj();
            yield return new WaitForSeconds(spawnCD);
        }
    }

    public void SpawnObj()
    {
        if (!IsHost) return;
        SpawnObjClientRpc();
    }

    [ClientRpc]
    protected void SpawnObjClientRpc()
    {
        GameObject objSpawned = ObjIsSpawned();
        objSpawned.transform.SetParent(holder.transform, true);
        objSpawned.GetComponent<Bullet>().SetTarget(positionTransform.position, headTransform.position, this);
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
        if (IsHost)
        {
            StopCoroutine(this.SpawnObjectCD());
        }
    }

    public void OnResume()
    {
        if (IsHost)
        {
            StartCoroutine(this.SpawnObjectCD());
        }
    }
}
