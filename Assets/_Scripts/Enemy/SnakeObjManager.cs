using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEngine;

public class SnakeObjManager : Spawner, ISceneObserver
{
    [SerializeField] protected float distanceBetween = 2.5f;
    [SerializeField] protected int size = 10;
    [SerializeField] protected int hpPart = 3;
    [SerializeField] public List<GameObject> bodyParts;

    protected GameObject objSpawned;

    [ServerRpc(RequireOwnership = false)]
    public virtual void GenerateObjsServerRPC()
    {
        for (int i = 0; i < size; i++)
        {
            objSpawned = ObjIsSpawned();
            bodyParts.Add(objSpawned);

            if (i == 0)
            {
                bodyParts[i].GetComponent<EnemySnakeBody>().ChangeTarget(this.gameObject);
                objSpawned.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + distanceBetween);
            }
            else
            {
                bodyParts[i].GetComponent<EnemySnakeBody>().ChangeTarget(bodyParts[i - 1].gameObject);
                objSpawned.transform.position = new Vector3(bodyParts[i - 1].transform.position.x, bodyParts[i - 1].transform.position.y + distanceBetween);
            }


            bodyParts[i].GetComponent<EnemySnakeBody>().distanceBetween = distanceBetween;
            objSpawned.GetComponent<ObjDeSpawnByHp>().SetHp(hpPart);
        }
    }

    private void OnEnable()
    {
        AddListObserver(this);
    }

    private void OnDisable()
    {
        RemoveListObserver(this);
    }

    public void AddListObserver(ISceneObserver observer)
    {
        _ScenesManager.Instance.AddListObserver(observer);
    }

    public void RemoveListObserver(ISceneObserver observer)
    {
        _ScenesManager.Instance.RemoveListObserver(observer);
    }

    public virtual void OnPause(int time)
    {
    }

    public virtual void OnResume()
    {
        GenerateObjsServerRPC();

    }

    public virtual void OnLoadDone()
    {
    }
}
