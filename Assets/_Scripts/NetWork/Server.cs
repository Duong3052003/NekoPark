using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Server : NetworkBehaviour
{
    public static Server Instance { get; private set; }

    // Netcode general
    const int k_bufferSize = 1024;

    //Netcode Server
    CircularBuffer<StatePayLoad> serverStateBuffer;
    Queue<InputPayLoad> serverInputQueue;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        serverStateBuffer = new CircularBuffer<StatePayLoad>(k_bufferSize);
        serverInputQueue = new Queue<InputPayLoad>();
    }

    private void Start()
    {
        if(this==null) return;
        NetworkTimer.Instance.CurrentTick.OnValueChanged += (oldValue, newValue) => HandleTick();
    }

    public void OnClientInput(InputPayLoad inputPayLoad)
    {
        serverInputQueue.Enqueue(inputPayLoad);
        Debug.Log("id gui" +inputPayLoad.networkObjID);
    }

    void HandleTick()
    {
        var bufferIndex = -1;
        InputPayLoad inputPayload = default;
        while (serverInputQueue.Count > 0)
        {
            inputPayload = serverInputQueue.Dequeue();
            Debug.Log("id gui2" +inputPayload.networkObjID);

            if (IsHost)
            {
                SimulateInputClientRpc(inputPayload);
            }
            else
            {
                SimulateInputServerRpc(inputPayload);
            }

            //PlayerManager.Instance.players[0].GetComponent<PlayerMove>().SimulateInputClientRpc(inputPayload);

            //bufferIndex = inputPayload.tick % k_bufferSize;

            //StatePayLoad statePayload = ProcessMovement(inputPayload);
            //serverStateBuffer.Add(statePayload, bufferIndex);
        }

        if (bufferIndex == -1) return;
        //SendToClientRpc(serverStateBuffer.Get(bufferIndex));
        //HandleExtrapolation(serverStateBuffer.Get(bufferIndex), CalculateLatencyInMillis(inputPayload));
    }

    [ServerRpc(RequireOwnership = false)]
    public void SimulateInputServerRpc(InputPayLoad inputPayLoad)
    {
        SimulateInputClientRpc(inputPayLoad);
    }

    [ClientRpc]
    public void SimulateInputClientRpc(InputPayLoad inputPayLoad)
    {
        //if (!IsHost || inputPayLoad.networkObjID != NetworkObjectId) return;
        Debug.Log("id gui3" + inputPayLoad.networkObjID);
        var networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[inputPayLoad.networkObjID];
        if (networkObject != null)
        {
            var playerMovement = networkObject.GetComponent<IPlayerMovement>();

            if (playerMovement != null)
            {
                playerMovement.Move(inputPayLoad.inputVector);
                playerMovement.Jump(inputPayLoad.inputVector);

               /* if (inputPayLoad.inputVector.y > 0)
                {
                    playerMovement.Jump(inputPayLoad.inputVector);
                }*/
            }
        }
    }





    /*public static Server Instance { get; private set; }

    private float timer;
    private int currentTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 30f;
    private const int BUFFER_SIZE = 1024;

    private StatePayLoad[] stateBuffer;
    private Queue<InputPayLoad> inputQueue;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        stateBuffer = new StatePayLoad[BUFFER_SIZE];
        inputQueue = new Queue<InputPayLoad>();
    }

    private void Update()
    {
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
        int bufferIndex = -1;
        while (inputQueue.Count > 0)
        {
            InputPayLoad inputPayLoad = inputQueue.Dequeue();

            bufferIndex = inputPayLoad.tick % BUFFER_SIZE;

            StatePayLoad statePayLoad = ProcessMovement(inputPayLoad);
            stateBuffer[bufferIndex] = statePayLoad;
        }

        if(bufferIndex != -1)
        {
            StartCoroutine(SendToClient(stateBuffer[bufferIndex]));
        }
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

    public void OnClientInput(InputPayLoad inputPayLoad)
    {
        inputQueue.Enqueue(inputPayLoad);
    }

    IEnumerator SendToClient(StatePayLoad statePayLoad)
    {
        yield return new WaitForSeconds(0.02f);

        Client.Instance.OnServerMovementState(statePayLoad);
    }*/
}
