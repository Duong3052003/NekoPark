using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static Unity.Networking.Transport.NetworkDriver;

public abstract class EnemyBehaviour : NetworkBehaviour, IObjectServerMovement, IObjectServerSpawn
{
    public Animator animator { get; protected set; }
    public Rigidbody2D rb { get; protected set; }

    [SerializeField] protected float speed = 5f;
    [SerializeField] protected float rotationSpeed = 10f;
    [SerializeField] protected bool canMove=true;

    [SerializeField] protected float velocityX = 0;
    [SerializeField] protected float velocityY = 0;

    // Netcode general
    protected const float k_serverTickRate = 60f; // 60 FPS
    protected const int k_bufferSize = 1024;
    public NetworkVariable<Vector3> nPosition = new NetworkVariable<Vector3>(new Vector3(0, 0, 0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> nRotation = new NetworkVariable<Quaternion>(Quaternion.identity , NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    protected float reconciliationThreshold = 2.5f;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        ReconcileTransform();
        NetworkTimer.Instance.CurrentTick.OnValueChanged += (oldValue, newValue) => SendMovementToServer();
    }

    protected virtual void Update()
    {
        if (!canMove) return;
        Move(velocityX, velocityY);
        RotateShip(GetMovement());
    }

    #region Movement
    protected abstract void Move(float _x, float _y);
    protected abstract void RotateShip(Vector3 targetVector);

    private void SendMovementToServer()
    {
        if(this == null) return;
        ReconcileTransform();

        var currentTick = NetworkTimer.Instance.CurrentTick.Value;
        var bufferIndex = currentTick % k_bufferSize;

        InputPayLoad inputPayload = new InputPayLoad()
        {
            tick = currentTick,
            timestamp = System.DateTime.Now,
            OwnerObjID = OwnerClientId,
            NetworkObjID = NetworkObjectId,
            inputVector = GetMovement(),
            position = transform.position
        };

        Server.Instance.OnClientInput(inputPayload);
    }

    public void Movement(Vector3 direction)
    {
        velocityX = direction.x;
        velocityY = direction.y;
    }

    public Vector3 GetMovement()
    {
        return new Vector3(velocityX, velocityY, 0);
    }

    private void ReconcileTransform()
    {
        if (IsOwner)
        {
            nPosition.Value = transform.position;
            nRotation.Value = transform.rotation;
        }
        else
        {
            float positionError = Vector3.Distance(nPosition.Value, transform.position);
            transform.rotation = nRotation.Value;
            if (positionError < reconciliationThreshold) return;
            transform.position = nPosition.Value;
        }
    }
    #endregion

    [ClientRpc]
    public virtual void SetCanMoveClientRpc(bool boolen)
    {
        canMove = boolen;
    }

    public virtual void Spawn(Vector3 inputVector, Vector2 velocityVector)
    {
        throw new System.NotImplementedException();
    }

    public virtual void DeSpawn()
    {
        StopAllCoroutines();
        NetworkTimer.Instance.CurrentTick.OnValueChanged -= (oldValue, newValue) => SendMovementToServer();
    }
}
