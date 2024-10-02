using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEngine;

public class SpawmObjByTime : Spawner
{
    protected bool canSpawn;
    [SerializeField] protected float spawnCD = 2f;
    [SerializeField] protected List<Transform> objSpawnePorts;

    public virtual void AddPorts(Transform portObj)
    {
        objSpawnePorts.Add(portObj);
    }

    public virtual void RemovePort(Transform portObj)
    {
        objSpawnePorts.Remove(portObj);
    }

    [ClientRpc]
    protected virtual void SpawnObjClientRpc(Vector2 position, Vector2 newTarget)
    {
        GameObject objSpawned = ObjIsSpawned();
    }

    protected IEnumerator SpawnObject()
    {
        while (objSpawnePorts.Count >= 1)
        {
            for (int k = 0; k < objSpawnePorts.Count; k++)
            {
                if (canSpawn)
                {
                    Transform headTrans = objSpawnePorts[k].GetComponent<PortGun>().headTransform;
                    Transform positionTrans = objSpawnePorts[k].GetComponent<PortGun>().positionTransform;
                    SpawnObjClientRpc(positionTrans.position, headTrans.position);
                }
                yield return new WaitForSeconds(spawnCD);
            }
        }
    }
}
