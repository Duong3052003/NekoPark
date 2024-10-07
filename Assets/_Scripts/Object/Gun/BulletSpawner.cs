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

    protected IEnumerator SpawnObjectCD()
    {
        while (true)
        {
            if (canSpawn)
            {
                SpawnObj();
                yield return new WaitForSeconds(spawnCD);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void StartCoroutineSpawn()
    {
        if (IsHost)
        {
            StartCoroutine(this.SpawnObjectCD());
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
        if(holder == null)
        {
            holder = GameObject.FindGameObjectWithTag("Holder").transform;

        }
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
        _ScenesManager.Instance.AddListObserver(observer);
    }

    public void RemoveListObserver(IObserver observer)
    {
        _ScenesManager.Instance.RemoveListObserver(observer);
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
        StartCoroutineSpawn();
    }

    public void OnLoadDone()
    {
    }
}
