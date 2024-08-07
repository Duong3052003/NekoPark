using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Spawner : NetworkBehaviour
{
    [SerializeField] protected Transform holder;
    [SerializeField] protected GameObject prefab;

    [SerializeField] protected List<GameObject> poolObjs;

    protected virtual GameObject ObjIsSpawned()
    {
        if (prefab == null) {
            Debug.Log("Miss prefab");
            return null;
        }

        GameObject newPrefab = this.GetObjFromPool(prefab);

        if (newPrefab.GetComponent<NetworkObject>() != null && !newPrefab.GetComponent<NetworkObject>().IsSpawned)
        {
            newPrefab.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            newPrefab.SetActive(true);
        }

        return newPrefab;
    }

    protected virtual GameObject GetObjFromPool(GameObject _prefab)
    {
        foreach (GameObject obj in poolObjs)
        {
            if (obj.name == _prefab.name)
            {
                this.poolObjs.Remove(obj);
                obj.gameObject.SetActive(true);
                return obj;
            }
        }

        GameObject newPrefab = Instantiate(prefab);
        newPrefab.name = _prefab.name;

        return newPrefab;
    }

    public virtual void DeSpawn(GameObject prefab)
    {
        this.poolObjs.Add(prefab);
        prefab.gameObject.SetActive(false);
    }
}
