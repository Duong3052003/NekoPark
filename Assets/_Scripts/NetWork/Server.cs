using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System.Linq;
using UnityEngine;

public class Server : NetworkBehaviour
{
    public static Server Instance { get; private set; }

    // Netcode general
    const int k_bufferSize = 1024;

    //Netcode Server
    Queue<InputPayLoad> serverInputQueue;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        serverInputQueue = new Queue<InputPayLoad>(k_bufferSize);
    }

    public override void OnNetworkSpawn()
    {
        NetworkTimer.Instance.CurrentTick.OnValueChanged += (oldValue, newValue) => HandleTick();
    }

    public void OnClientInput(InputPayLoad inputPayLoad)
    {
        serverInputQueue.Enqueue(inputPayLoad);
    }

    void HandleTick()
    {
        while (serverInputQueue.TryDequeue(out var inputPayload))
        {
            SimulateInputServerRpc(inputPayload, inputPayload.OwnerObjID);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SimulateInputServerRpc(InputPayLoad inputPayLoad, ulong excepClientID)
    {   
        List<ulong> targetClients = NetworkManager.Singleton.ConnectedClientsList
            .Where(client => client.ClientId != excepClientID)
            .Select(client => client.ClientId)
            .ToList();

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClients
            }
        };

        SimulateInputClientRpc(inputPayLoad, clientRpcParams);
    }

    [ClientRpc]
    public void SimulateInputClientRpc(InputPayLoad inputPayLoad, ClientRpcParams clientRpcParams = default)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(inputPayLoad.NetworkObjID, out var networkObject))
        {
            //Debug.LogWarning($"NetworkObject with ID {inputPayLoad.OwnerObjID} not found.");
            return;
        }

        networkObject.GetComponent<IObjectServerMovement>()?.Movement(inputPayLoad.inputVector);
    }
}
