using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BulletSpawner : Spawner, ISceneObserver
{
    public bool canSpawn;

    public Transform posTransform;
    public Transform targetTranform;
    public float spawnCD;

    public bool isSpin = false;
    public bool isRight = false;
    public bool isSpinRight = false;

    [SerializeField] private bool inScene=true;
    [SerializeField] private AudioClip shootEffect;

    private void Update()
    {
        if(!isSpin)return;
        if (isSpinRight)
        {
            transform.eulerAngles = new Vector3(0f, 0f, transform.eulerAngles.z + 1f);
        }
        else
        {
            transform.eulerAngles = new Vector3(0f, 0f, transform.eulerAngles.z - 1f);
        }
    }

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

    /*[ClientRpc]
    public void SettingGunPortClientRpc(bool boolen)
    {
        gunPort.Setting(boolen);
    }*/

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
        if((posTransform!=null && targetTranform.position != null) || isSpin)
        {
            SpawnObjClientRpc();
        }
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

        if(isSpin)
        {
            objSpawned.GetComponent<Bullet>().SetRotate(this.transform.position, this.transform.rotation, isRight, this);
        }
        else
        {
            objSpawned.GetComponent<Bullet>().SetTarget(posTransform.position, targetTranform.position, this);
        }

        SoundManager.Instance.PlaySound(shootEffect);
    }

    private void OnEnable()
    {
        AddListObserver(this);
        if (IsHost)
        {
            StartCoroutine(this.SpawnObjectCD());
        }
    }

    private void OnDisable()
    {
        RemoveListObserver(this);
        if (IsHost)
        {
            StopCoroutine(this.SpawnObjectCD());
        }
    }

    public void AddListObserver(ISceneObserver observer)
    {
        _ScenesManager.Instance.AddListObserver(observer);
    }

    public void RemoveListObserver(ISceneObserver observer)
    {
        _ScenesManager.Instance.RemoveListObserver(observer);
    }

    public void OnPause(int time)
    {
        
    }

    public void OnResume()
    {
        if (inScene)
        {
            StartCoroutineSpawn();
        }
    }

    public void OnLoadDone()
    {
    }
}
