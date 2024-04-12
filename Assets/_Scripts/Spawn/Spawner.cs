using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Spawner : MonoBehaviour
{
    [SerializeField] protected Transform holder;
    [SerializeField] protected List<Transform> prefabs;
    [SerializeField] protected List<Transform> poolObjs;


    public virtual Transform SpawnObj(Transform prefab, Vector3 spawnPos, Quaternion rotation)
    {
        if (prefab == null) {
            Debug.Log("Miss prefab");
            return null;
        }

        Transform newPrefab = this.GetObjFromPool(prefab);
        newPrefab.SetPositionAndRotation(spawnPos, rotation);
        newPrefab.parent = this.holder;

        return newPrefab;
    }

    protected virtual Transform GetObjFromPool(Transform prefab)
    {
        foreach (Transform obj in poolObjs)
        {
            if (obj.name == prefab.name)
            {
                this.poolObjs.Remove(obj);
                return obj;
            }
        }

        Transform newPrefab = Instantiate(prefab);
        newPrefab.name = prefab.name;
        return newPrefab;
    }

    protected virtual void DeSpawn(Transform prefab)
    {
        this.poolObjs.Add(prefab);
        prefab.gameObject.SetActive(false);
    }

}
