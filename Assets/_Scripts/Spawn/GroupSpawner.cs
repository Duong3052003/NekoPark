using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GroupSpawner : Spawner
{
    public bool canSpawn;

    public GunPort[] gunPorts;

    private int objCount= 0;
    public float spawnCD;

    [SerializeField] private AudioClip shootEffect;

    protected IEnumerator SpawnObjectCD(int limit)
    {
        while (true)
        {
            if (canSpawn && objCount <= limit)
            {
                int i = Random.Range(0, gunPorts.Length);
                SpawnObj(i);
                objCount++;
                yield return new WaitForSeconds(spawnCD);
            }
            else
            {
                StopAllCoroutines();
                objCount = 0;
                yield return null;
            }
        }
    }

    /*[ClientRpc]
    public void SettingGunPortClientRpc(bool boolen)
    {
        gunPort.Setting(boolen);
    }*/

    public void StartCoroutineSpawn(int limit)
    {
        if (IsHost)
        {
            StartCoroutine(this.SpawnObjectCD(limit));
        }
    }

    public void SpawnObj(int i)
    {
        if (!IsHost) return;
        if ((gunPorts[i].positionTransform != null && gunPorts[i].headTransform != null))
        {
            SpawnObjClientRpc(i);
        }
    }

    [ClientRpc]
    protected void SpawnObjClientRpc(int i)
    {
        if (holder == null)
        {
            holder = GameObject.FindGameObjectWithTag("Holder").transform;

        }
        GameObject objSpawned = ObjIsSpawned();
        objSpawned.transform.SetParent(holder.transform, true);

        objSpawned.GetComponent<Bullet>().SetTarget(gunPorts[i].positionTransform.position, gunPorts[i].headTransform.position, this);

        SoundManager.Instance.PlaySound(shootEffect);
    }
}
