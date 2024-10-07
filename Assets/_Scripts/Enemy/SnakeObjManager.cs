using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEngine;

public class SnakeObjManager : Spawner, IObserver
{
    [SerializeField] protected float speed = 30f;
    [SerializeField] protected float distanceBetween = 2.5f;
    [SerializeField] protected int size = 10;
    [SerializeField] protected int hpHead = 5;
    [SerializeField] protected int hpPart = 3;
    [SerializeField] protected List<GameObject> bodyParts;

    [ServerRpc(RequireOwnership = false)]
    protected virtual void GenerateObjsServerRPC()
    {
        for (int i = 0; i < size; i++)
        {
            GameObject objSpawned = ObjIsSpawned();
            bodyParts.Add(objSpawned);

            if (i==0)
            {
                bodyParts[i].GetComponent<EnemySnakeBody>().ChangeTarget(this.gameObject);
                objSpawned.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y - distanceBetween);
                this.gameObject.GetComponent<ObjDeSpawnByHp>().SetHp(hpHead);
            }
            else
            {
                bodyParts[i].GetComponent<EnemySnakeBody>().ChangeTarget(bodyParts[i - 1].gameObject);
                objSpawned.transform.position = new Vector3(bodyParts[i-1].transform.position.x, bodyParts[i - 1].transform.position.y - distanceBetween);
            }
            bodyParts[i].GetComponent<EnemySnakeBody>().distanceBetween = distanceBetween;
            //objSpawned.transform.SetParent(this.gameObject.transform);
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

    public void AddListObserver(IObserver observer)
    {
        _ScenesManager.Instance.AddListObserver(observer);
    }

    public void RemoveListObserver(IObserver observer)
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

    public void OnLoadDone()
    {
    }
}
