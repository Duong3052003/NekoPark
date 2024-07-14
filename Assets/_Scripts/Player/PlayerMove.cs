using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerMove : NetworkBehaviour,IPlayerMovement
{
    private PlayerCtrl playerCtrl;
    private ClientNetworkTransformCus clientNetworkTransformCus;

    [Header("Physics")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float jumpPower = 18f;
    [SerializeField] private float gravitymin = 5f;
    [SerializeField] private float gravitymax = 7f;

    [Header("Others")]
    private bool rightCheck = true;
    
    /*[Header("Netcode")]
    [SerializeField] float reconciliationCooldownTime = 1f;
    [SerializeField] float reconciliationThreshold = 50f;
    [SerializeField] float extrapolationLimit = 0.5f;
    [SerializeField] float extrapolationMultiplier = 1.2f;
    CountdownTimer reconciliationTimer;
    CountdownTimer extrapolationTimer;
    StatePayLoad extrapolationState;*/

    // Netcode general
    const float k_serverTickRate = 60f; // 60 FPS
    const int k_bufferSize = 1024;

    //Netcode Client
    /*CircularBuffer<StatePayLoad> clientStateBuffer;
    CircularBuffer<InputPayLoad> clientInputBuffer;*/
    /*StatePayLoad lastServerState;
    StatePayLoad lastProcessState;*/

    //Netcode Server
   /* CircularBuffer<StatePayLoad> serverStateBuffer;
    Queue<InputPayLoad> serverInputQueue;*/

    //network variable
    public NetworkVariable<Vector3> nPosition = new NetworkVariable<Vector3>(new Vector3(0,0,0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> nRotation = new NetworkVariable<Quaternion>(new Quaternion(0,0,0,0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] float reconciliationThreshold = 50f;

    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();

       /* clientNetworkTransformCus = GetComponent<ClientNetworkTransformCus>();

        networkTimer = new NetworkTimer(k_serverTickRate);
        clientStateBuffer = new CircularBuffer<StatePayLoad>(k_bufferSize);
        clientInputBuffer = new CircularBuffer<InputPayLoad>(k_bufferSize);

        serverStateBuffer = new CircularBuffer<StatePayLoad>(k_bufferSize);
        serverInputQueue = new Queue<InputPayLoad>();

        reconciliationTimer = new CountdownTimer(reconciliationCooldownTime);
        extrapolationTimer = new CountdownTimer(extrapolationLimit);

        reconciliationTimer.OnTimerStart += () => {
            extrapolationTimer.Stop();
        };

        extrapolationTimer.OnTimerStart += () => {
            reconciliationTimer.Stop();
            SwitchAuthorityMode(AuthorityMode.Server);
        };
        extrapolationTimer.OnTimerStop += () => {
            extrapolationState = default;
            SwitchAuthorityMode(AuthorityMode.Client);
        };*/
    }

    public override void OnNetworkSpawn()
    {
        UpdateNetworkVariables();
    }

    /*void SwitchAuthorityMode(AuthorityMode mode)
    {
        clientNetworkTransformCus.authorityMode = mode;
        bool shouldSync = mode == AuthorityMode.Client;
        clientNetworkTransformCus.SyncPositionX = shouldSync;
        clientNetworkTransformCus.SyncPositionY = shouldSync;
        clientNetworkTransformCus.SyncPositionZ = shouldSync;
    }*/

    private void Start()
    {
        NetworkTimer.Instance.CurrentTick.OnValueChanged += (oldValue, newValue) => HandleClientTick();
    }

    private void Update()
    {
        if (!IsOwner || playerCtrl.rb.bodyType == RigidbodyType2D.Static) return;
        MoveInput();

        /*Move(new Vector2(MoveInput().x, playerCtrl.rb.velocity.y));
        Jump(new Vector2(playerCtrl.rb.velocity.x, MoveInput().y));*/
    }

    private Vector2 MoveInput()
    {
        return new Vector2(InputManager.Instance.InputHorizon(), InputManager.Instance.InputVertical());
    }

    void HandleClientTick()
    {
        UpdateNetworkVariables();

        if (!IsOwner) return;
        var currentTick = NetworkTimer.Instance.CurrentTick.Value;
        var bufferIndex = currentTick % k_bufferSize;

        InputPayLoad inputPayload = new InputPayLoad()
        {
            tick = currentTick,
            timestamp = DateTime.Now,
            networkObjID = NetworkObjectId,
            inputVector = MoveInput(),
            position = transform.position
        };

        //clientInputBuffer.Add(inputPayload, bufferIndex);
        Server.Instance.OnClientInput(inputPayload);
        //StatePayLoad statePayload = ProcessMovement(inputPayload);
        //clientStateBuffer.Add(statePayload, bufferIndex);

        //HandleServerReconciliation();
    }

    StatePayLoad ProcessMovement(InputPayLoad inputPayLoad)
    {
        Move(inputPayLoad.inputVector);
        Jump(inputPayLoad.inputVector);

        return new StatePayLoad()
        {
            tick = inputPayLoad.tick,
            networkObjID = inputPayLoad.networkObjID,
            position = inputPayLoad.position,
            rotation = transform.rotation,
            velocity = playerCtrl.rb.velocity,
            angularVelocity = playerCtrl.rb.angularVelocity
        };
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

        if (inputVector.y == 1.5)
        {
            playerCtrl.rb.velocity = new Vector2(playerCtrl.rb.velocity.x, playerCtrl.rb.velocity.y * 0.5f);
        }
    }

    private void Flip()
    {
        rightCheck = !rightCheck;

        if (rightCheck == true)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        }
    }

    private void GravityChanged()
    {
        if (playerCtrl.rb.velocity.y < 0)
        {
            playerCtrl.rb.gravityScale = gravitymax;
        }
        else
        {
            playerCtrl.rb.gravityScale = gravitymin;
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

            transform.position = nPosition.Value;
            transform.rotation = nRotation.Value;
        }
    }



    /* static float CalculateLatencyInMillis(InputPayLoad inputPayLoad)
     {
         return (DateTime.Now - inputPayLoad.timestamp).Milliseconds / 1000f;
     }

     bool ShouldReconcile()
     {
         bool isNewServerState = !lastServerState.Equals(default);
         bool isLastStateUndefinedOrDifferent = lastProcessState.Equals(default)
                                                || !lastProcessState.Equals(lastServerState);

         return isNewServerState && isLastStateUndefinedOrDifferent && !reconciliationTimer.IsRunning && !extrapolationTimer.IsRunning;
     }

     void HandleServerReconciliation()
     {
         if (!ShouldReconcile()) return;

         float positionError;
         int bufferIndex;

         bufferIndex = lastServerState.tick % k_bufferSize;
         if (bufferIndex - 1 < 0) return; // Not enough information to reconcile

         StatePayLoad rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState; // Host RPCs execute immediately, so we can use the last server state
         StatePayLoad clientState = IsHost ? clientStateBuffer.Get(bufferIndex - 1) : clientStateBuffer.Get(bufferIndex);
         positionError = Vector3.Distance(rewindState.position, clientState.position);

         if (positionError > reconciliationThreshold)
         {
             ReconcileState(rewindState);
         }

         lastProcessState = rewindState;
     }

     void ReconcileState(StatePayLoad rewindState)
     {
         transform.position = rewindState.position;
         transform.rotation = rewindState.rotation;
         playerCtrl.rb.velocity = rewindState.velocity;

         if (!rewindState.Equals(lastServerState)) return;

         clientStateBuffer.Add(rewindState, rewindState.tick % k_bufferSize);

         // Replay all inputs from the rewind state to the current state
         int tickToReplay = lastServerState.tick;

         while (tickToReplay < networkTimer.CurrentTick)
         {
             int bufferIndex = tickToReplay % k_bufferSize;
             StatePayLoad statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
             clientStateBuffer.Add(statePayload, bufferIndex);
             tickToReplay++;
         }
     }

     void Extraplolate()
     {
         if (IsServer && extrapolationTimer.IsRunning)
         {
             transform.position += extrapolationState.position.With(y: 0);
         }
     }

     void HandleExtrapolation(StatePayLoad latest, float latency)
     {
         if (ShouldExtrapolate(latency))
         {
             if (extrapolationState.position != default)
             {
                 latest = extrapolationState;
             }

             // Update position and rotation based on extrapolation
             var posAdjustment = latest.velocity * (1 + latency * extrapolationMultiplier);
             extrapolationState.position = posAdjustment;
             extrapolationState.rotation = transform.rotation;
             extrapolationState.velocity = latest.velocity;
             extrapolationTimer.Start();
         }
         else
         {
             extrapolationTimer.Stop();
         }
     }

     bool ShouldExtrapolate(float latency) => latency < extrapolationLimit && latency > Time.fixedDeltaTime;


     [ServerRpc]
     void SendToServerRPC(InputPayLoad input)
     {
         serverInputQueue.Enqueue(input);
     }

     void HandleServerTick()
     {
         if (!IsServer) return;

         var bufferIndex = -1;
         InputPayLoad inputPayload = default;
         while (serverInputQueue.Count > 0)
         {
             inputPayload = serverInputQueue.Dequeue();

             bufferIndex = inputPayload.tick % k_bufferSize;

             StatePayLoad statePayload = ProcessMovement(inputPayload);
             serverStateBuffer.Add(statePayload, bufferIndex);
         }

         if (bufferIndex == -1) return;
         SendToClientRpc(serverStateBuffer.Get(bufferIndex));
         HandleExtrapolation(serverStateBuffer.Get(bufferIndex), CalculateLatencyInMillis(inputPayload));
     }

     [ClientRpc]
     void SendToClientRpc(StatePayLoad statePayload)
     {
         transform.position = statePayload.position;
         if (!IsOwner) return;
         lastServerState = statePayload;
     }

     void HandleClientTick()
     {
         if (!IsClient || !IsOwner) return;

         var currentTick = networkTimer.CurrentTick;
         var bufferIndex = currentTick % k_bufferSize;

         InputPayLoad inputPayload = new InputPayLoad()
         {
             tick = currentTick,
             timestamp = DateTime.Now,
             networkObjID = NetworkObjectId,
             inputVector = MoveInput(),
             position = transform.position
         };

         clientInputBuffer.Add(inputPayload, bufferIndex);
         SendToServerRPC(inputPayload);

         StatePayLoad statePayload = ProcessMovement(inputPayload);
         clientStateBuffer.Add(statePayload, bufferIndex);

         HandleServerReconciliation();
     }

     StatePayLoad ProcessMovement(InputPayLoad inputPayLoad)
     {
         Move(inputPayLoad.inputVector);
         Jump(inputPayLoad.inputVector);

         return new StatePayLoad()
         {
             tick = inputPayLoad.tick,
             networkObjID = inputPayLoad.networkObjID,
             position = inputPayLoad.position,
             rotation = transform.rotation,
             velocity = playerCtrl.rb.velocity,
             angularVelocity = playerCtrl.rb.angularVelocity
         };
     }*/
}

