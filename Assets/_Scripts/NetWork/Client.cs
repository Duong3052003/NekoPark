using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public struct InputPayLoad : INetworkSerializable
{
    public int tick;
    public DateTime timestamp;
    public ulong networkObjID;
    public Vector3 inputVector;
    public Vector3 position;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref timestamp);
        serializer.SerializeValue(ref networkObjID);
        serializer.SerializeValue(ref inputVector);
        serializer.SerializeValue(ref position);
    }
}

public struct StatePayLoad : INetworkSerializable
{
    public int tick;
    public ulong networkObjID;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public float angularVelocity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref networkObjID);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref velocity);
    }
}

public class Client : MonoBehaviour
{
   /* public static Client Instance { get; private set; }

    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 30f;
    private const int BUFFER_SIZE = 1024;

    private StatePayLoad[] stateBuffer;
    private InputPayLoad[] inputBuffer;
    private StatePayLoad lastestServerState;
    private StatePayLoad lastProcessedState;
    private float horizontalInput;
    private float verticalInput;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        stateBuffer = new StatePayLoad[BUFFER_SIZE];
        inputBuffer = new InputPayLoad[BUFFER_SIZE];
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        timer += Time.deltaTime;

        while (timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }
    }

    void HandleTick()
    {
        if(!lastestServerState.Equals(default(StatePayLoad)) &&
            (lastProcessedState.Equals(default(StatePayLoad)))||
            !lastestServerState.Equals(lastProcessedState))
        {
            HandleServerReconciliation();
        }

        int bufferIndex = currentTick % BUFFER_SIZE;

        InputPayLoad inputPayLoad = new InputPayLoad();
        inputPayLoad.tick = currentTick;
        inputPayLoad.inputVector = new Vector3(horizontalInput, verticalInput, 0);
        inputBuffer[bufferIndex] = inputPayLoad;

        stateBuffer[bufferIndex] = ProcessMovement(inputPayLoad);

        StartCoroutine(SendToServer(inputPayLoad));
    }

    IEnumerator SendToServer(InputPayLoad inputPayLoad)
    {
        yield return new WaitForSeconds(0.02f);
        //Server.Instance.OnClientInput(inputPayLoad);
    }

    StatePayLoad ProcessMovement(InputPayLoad input)
    {
        transform.position += input.inputVector * 7f * minTimeBetweenTicks;

        return new StatePayLoad()
        {
            tick = input.tick,
            position = transform.position,
        };
    }

    public void OnServerMovementState(StatePayLoad serverState)
    {
        lastestServerState = serverState;
    }

    void HandleServerReconciliation()
    {
        lastProcessedState = lastestServerState;

        int serverStateBufferIndex = lastestServerState.tick % BUFFER_SIZE;
        float positionError = Vector3.Distance(lastestServerState.position, stateBuffer[serverStateBufferIndex].position);

        if(positionError > 0.001)
        {
            Debug.Log("Chung ta can chua lanh");

            transform.position = lastestServerState.position;

            stateBuffer[serverStateBufferIndex] = lastestServerState ;

            int tickToProcess = lastestServerState.tick + 1;

            while(tickToProcess < currentTick)
            {
                StatePayLoad statePayLoad = ProcessMovement(inputBuffer[tickToProcess]);

                int bufferIndex = tickToProcess % BUFFER_SIZE;
                stateBuffer[bufferIndex] = statePayLoad ;

                tickToProcess++;
            }
        }
    }*/
}
