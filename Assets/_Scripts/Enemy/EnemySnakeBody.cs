using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using static Unity.Networking.Transport.NetworkDriver;

public class EnemySnakeBody : EnemyBehaviour, ISnakeObserver
{
    public GameObject target;
    private Vector3 posCurrent;

    public float distanceBetween;

    public override void OnNetworkSpawn()
    {
        canMove = true;
        if (!IsHost) return;
        AddBodyClientRpc();
    }

    protected override void Awake()
    {
        base.Awake();
    }

    [ClientRpc]
    public void AddBodyClientRpc()
    {
        EnemySnake.Instance.bodyParts.Add(this.gameObject);
        EnemySnake.Instance.GetPosition();
    }

    public void ChangeTarget(GameObject newTarget)
    {
        target = newTarget;
        SetCanMoveClientRpc(true);
    }

    protected override void Update()
    {
        if (!canMove) return;
        if(target == null&&IsHost)
        {
            this.GetComponent<EnemyDespawn>().CallDespawn();
        }
        if (target == null || distanceBetween >= (target.transform.position - transform.position).magnitude) return;
        Move(velocityX, velocityY);
        RotateShip(GetMovement());
        Movement((target.transform.position - transform.position).normalized);
    }

    #region Movement
    protected override void Move(float _x, float _y)
    {
        rb.velocity = new Vector3(_x, _y) * speed;
    }

    protected override void RotateShip(Vector3 targetVector)
    {
        if (targetVector != Vector3.zero)
        {
            float angle = Mathf.Atan2(targetVector.y, targetVector.x) * Mathf.Rad2Deg - 90f;

            rb.rotation = Mathf.LerpAngle(rb.rotation, angle, Time.deltaTime * rotationSpeed);
        }
        else
        {
            rb.rotation = 0;
        }
    }

    private void Start()
    {
        AddListSnakeObserver(this);
    }

    private void OnDisable()
    {
        RemoveListSnakeObserver(this);
    }

    public void AddListSnakeObserver(ISnakeObserver observer)
    {
        EnemySnake.Instance.AddListSnakeObserver(observer);
    }

    public void RemoveListSnakeObserver(ISnakeObserver observer)
    {
        EnemySnake.Instance.RemoveListSnakeObserver(observer);
    }

    public void OnSetting()
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
