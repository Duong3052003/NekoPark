using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public abstract class PlayerMove : NetworkBehaviour, IObjectServerMovement, IObserver
{
    protected PlayerCtrl playerCtrl;

    [Header("Physics")]
    [SerializeField] protected float speed = 7f;

    [Header("Others")]
    [SerializeField] protected bool CanMove = true;

    // Netcode general
    const float k_serverTickRate = 60f; // 60 FPS
    const int k_bufferSize = 1024;

    //network variable
    public NetworkVariable<Vector3> nPosition = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<Quaternion> nRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    protected readonly float reconciliationThreshold = 2.5f;

    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
    }

    public void SetPositionNetworkVariable(Vector3 newPos)
    {
        this.transform.position = newPos;
        if (!IsOwner) return;
        nPosition.Value = newPos;
    }

    public override void OnNetworkSpawn()
    {
        NetworkTimer.Instance.CurrentTick.OnValueChanged += (oldValue, newValue) => HandleClientTick();
    }

    private void Update()
    {
        if (!IsOwner || playerCtrl.rb.bodyType == RigidbodyType2D.Static || !CanMove) return;
        MoveInput();
        Movement(MoveInput());
    }

    private void OnEnable()
    {
        AddListObserver(this);
    }

    private void OnDisable()
    {
        RemoveListObserver(this);
    }


    #region Movement
    protected abstract Vector2 MoveInput();
    public abstract void Movement(Vector3 inputVector);

    void HandleClientTick()
    {
        if(this == null) return;
        if (!CanMove)
        {
            UpdateAbsNetworkVariables();
        }
        else
        {
            UpdateNetworkVariables();

            if (!IsOwner) return;
            var currentTick = NetworkTimer.Instance.CurrentTick.Value;
            var bufferIndex = currentTick % k_bufferSize;

            InputPayLoad inputPayload = new InputPayLoad()
            {
                tick = currentTick,
                timestamp = DateTime.Now,
                OwnerObjID = OwnerClientId,
                NetworkObjID = NetworkObjectId,
                inputVector = MoveInput(),
                position = transform.position
            };

            Server.Instance.OnClientInput(inputPayload);
        }
    }

    private void UpdateNetworkVariables()
    {
        if (IsOwner)
        {
            nPosition.Value = transform.position;
            nRotation.Value = transform.rotation;
        }
        else
        {
            float positionError = Vector3.Distance(nPosition.Value, transform.position);

            if (positionError < reconciliationThreshold) return;
            transform.position = Vector3.Lerp(transform.position, nPosition.Value, Time.deltaTime * speed);
            transform.rotation = nRotation.Value;
        }
    }

    private void UpdateAbsNetworkVariables()
    {
        if (IsOwner)
        {
            nPosition.Value = transform.position;
            nRotation.Value = transform.rotation;
        }
        else
        {
            transform.position = nPosition.Value;
            transform.rotation = nRotation.Value;
        }
    }

    public void SetSpeed(float multiplier)
    {
        this.speed *= multiplier;
    }
    #endregion

    public void AddListObserver(IObserver observer)
    {
        NetworkTimer.Instance.AddListObserver(observer);
    }

    public void RemoveListObserver(IObserver observer)
    {
        NetworkTimer.Instance.RemoveListObserver(observer);
    }

    public void OnPause(int time)
    {
        if (!IsServer) return;
        SetCanMoveClientRpc(false);
    }

    public void OnResume()
    {
        UpdateAbsNetworkVariables();
        if (!IsServer) return;
        SetCanMoveClientRpc(true);
    }

    [ClientRpc]
    private void SetCanMoveClientRpc(bool boolen)
    {
        CanMove = boolen;
    }
}

