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
    private BulletSpawner spawner;
    private Coroutine coroutine;

    public float distanceBetween;

    public override void OnNetworkSpawn()
    {
        canMove = true;
        if (!IsHost) return;
        AddBodyClientRpc();
    }

    protected override void Awake()
    {
        spawner = GetComponent<BulletSpawner>();
        base.Awake();
    }

    [ClientRpc]
    public void AddBodyClientRpc()
    {
        EnemySnake.Instance.bodyParts.Add(this.gameObject);
        EnemySnake.Instance.GetPosition();
    }

    public void GetGun()
    {
        spawner.gunPort.positionTransform = this.transform;
        spawner.gunPort.headTransform = this.transform;

        spawner.StartCoroutineSpawn();
    }

    public void ChangeTarget(GameObject newTarget)
    {
        target = newTarget;
        SetCanMoveClientRpc(true);
    }

    protected override void Update()
    {
        ReconcileTransform();

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

    protected override void ReconcileTransform()
    {
        if (IsOwner)
        {
            nPosition.Value = transform.position;
        }
        else
        {
            float positionError = Vector3.Distance(nPosition.Value, transform.position);
            transform.position = nPosition.Value;
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

    public void OnSetting(float _distanceBetween, float _speed, bool targetFollow)
    {
        OnSettingClientRpc(_distanceBetween, _speed, targetFollow);
    }

    [ClientRpc]
    private void OnSettingClientRpc(float _distanceBetween, float _speed, bool targetFollow)
    {
        if (targetFollow == true && spawner.gunPort.positionTransform!=null)
        {
            spawner.gunPort.headTransform = Target(PlayerManager.Instance.players).transform;
        }
        distanceBetween = _distanceBetween;
        speed = _speed;
    }

    public GameObject Target(List<GameObject> validTargets)
    {
        if (validTargets.Count == 0)
        {
            Debug.LogWarning("Khong co muc tieu.");
            return null;
        }
        return RandomGameObjectFromList.GetRandomGameObject(validTargets);
    }
    #endregion
}
