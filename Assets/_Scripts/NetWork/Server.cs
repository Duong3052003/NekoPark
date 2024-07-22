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
    }

    void HandleTick()
    {
        InputPayLoad inputPayload = default;
        while (serverInputQueue.Count > 0)
        {
            inputPayload = serverInputQueue.Dequeue();

            if (IsHost)
            {
                SimulateInputClientRpc(inputPayload);
            }
            else
            {
                SimulateInputServerRpc(inputPayload);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SimulateInputServerRpc(InputPayLoad inputPayLoad)
    {
        SimulateInputClientRpc(inputPayLoad);
    }

    [ClientRpc]
    public void SimulateInputClientRpc(InputPayLoad inputPayLoad)
    {
        var networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[inputPayLoad.networkObjID];

        if (networkObject != null)
        {
            var playerMovement = networkObject.GetComponent<IPlayerMovement>();

            if (playerMovement != null)
            {
                playerMovement.Move(inputPayLoad.inputVector);
                playerMovement.Jump(inputPayLoad.inputVector);
            }

            var objectMovement = networkObject.GetComponent<IObjectMovement>();

            if (objectMovement != null)
            {
                objectMovement.Movement(inputPayLoad.inputVector);
            }
        }
    }
}
