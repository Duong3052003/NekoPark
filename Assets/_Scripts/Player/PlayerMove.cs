using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerMove : NetworkBehaviour, IObjectServerMovement, IObserver
{
    private PlayerCtrl playerCtrl;

    [Header("Physics")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float jumpPower = 18f;
    [SerializeField] private float gravitymin = 5f;
    [SerializeField] private float gravitymax = 7f;

    [Header("Others")]
    private bool rightCheck = true;
    private bool CanMove = true;

    // Netcode general
    const float k_serverTickRate = 60f; // 60 FPS
    const int k_bufferSize = 1024;

    //network variable
    private NetworkVariable<Vector3> nPosition = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private NetworkVariable<Quaternion> nRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private readonly float reconciliationThreshold = 2.5f;

    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
    }

    public override void OnNetworkSpawn()
    {
        UpdateNetworkVariables();
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
    private Vector2 MoveInput()
    {
        return new Vector2(InputManager.InputHorizon(), InputManager.InputVertical());
    }

    void HandleClientTick()
    {
        if(this == null) return;
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

    public void Movement(Vector3 inputVector)
    {
        if (playerCtrl.rb.bodyType == RigidbodyType2D.Static) return;
        Move(inputVector);
        Jump(inputVector);
    }

    public void Move(Vector2 inputVector)
    {
        if ((rightCheck == true && inputVector.x < 0f || rightCheck == false && inputVector.x > 0f))
        {
            Flip();
        }

        playerCtrl.rb.velocity = new Vector2(inputVector.x * speed, playerCtrl.rb.velocity.y);
    }

    public void Jump(Vector2 inputVector)
    {
        if (playerCtrl.checkGroundColiision.IsGrounded())
        {
            if (inputVector.y == 1)
            {
                playerCtrl.rb.velocity = new Vector2(playerCtrl.rb.velocity.x, jumpPower);
            }
        }
    }

    private void Flip()
    {
        /*rightCheck = !rightCheck;

        if (rightCheck == true)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        }*/
        rightCheck = !rightCheck;

        transform.rotation = rightCheck ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, -180f, 0f);
    }

    private void GravityChanged()
    {
        /*if (playerCtrl.rb.velocity.y < 0)
        {
            playerCtrl.rb.gravityScale = gravitymax;
        }
        else
        {
            playerCtrl.rb.gravityScale = gravitymin;
        }*/

        playerCtrl.rb.gravityScale = playerCtrl.rb.velocity.y < 0 ? gravitymax : gravitymin;
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
            transform.position = nPosition.Value;
            transform.rotation = nRotation.Value;
        }
    }
    #endregion

    public void SetSpeed(float multiplier)
    {
        this.speed *= multiplier;
    }

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
        if (!IsServer) return;
        SetCanMoveClientRpc(true);
    }

    [ClientRpc]
    private void SetCanMoveClientRpc(bool boolen)
    {
        CanMove = boolen;
    }
}

