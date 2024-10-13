using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public abstract class PlayerHp : DeSpawnByHp, ITakeDamaged, IPlayerStatus
{
    protected PlayerCtrl playerCtrl;

    protected Slider hpBar;

    [SerializeField] protected GameObject hpBarPrefab;
    [SerializeField] protected GameObject CanvasWorldSpacePrefab;

    public int hpMax = 3;

    protected virtual void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            hpCurrent.Value = hpMax;
        }

        hpCurrent.OnValueChanged += (oldValue, newValue) => UpdateHpBar(oldValue, newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnObjectAndSendIdServerRpc(ulong _idPlayer, ServerRpcParams rpcParams = default)
    {
        GameObject canvasWorld = Instantiate(CanvasWorldSpacePrefab, this.gameObject.transform.position, Quaternion.identity);
        canvasWorld.GetComponent<NetworkObject>().SpawnWithOwnership(_idPlayer);
        canvasWorld.transform.SetParent(this.transform);

        GameObject hpBarGameObj = Instantiate(hpBarPrefab);
        NetworkObject networkObject = hpBarGameObj.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(_idPlayer);
        hpBarGameObj.transform.SetParent(canvasWorld.transform);
        hpBarGameObj.transform.localScale = new Vector3(1, 2, 1);
        hpBarGameObj.transform.localPosition = new Vector3(0, -120f, 0);

        SendObjectIdToClientClientRpc(networkObject.NetworkObjectId, default);
        //SelectTargetServerRpc(networkObject.NetworkObjectId, _idPlayer);
    }

    [ServerRpc]
    private void SelectTargetServerRpc(ulong networkObjectId, ulong clientSelected)
    {
        List<ulong> targetClients = NetworkManager.Singleton.ConnectedClientsList
            .Where(client => client.ClientId == clientSelected)
            .Select(client => client.ClientId)
            .ToList();

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClients
                //TargetClientIds = default
            }
        };

        SendObjectIdToClientClientRpc(networkObjectId, clientRpcParams);
    }

    [ClientRpc]
    private void SendObjectIdToClientClientRpc(ulong networkObjectId, ClientRpcParams clientRpcParams = default)
    {
        GetComponentFromSpawnedObject(networkObjectId);
    }

    private void GetComponentFromSpawnedObject(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var networkObject))
        {
            hpBar = networkObject.GetComponent<Slider>();
            hpBar.value = hpCurrent.Value / hpMax;
        }
        else
        {
            Debug.LogWarning("NetworkObject not found.");
        }
    }

    protected virtual void Update()
    {
        if (hpBar == null) return;
        hpBar.gameObject.transform.rotation = Camera.main.transform.rotation;
    }

    public void GetSetting(ulong _idPlayer)
    {
        SpawnObjectAndSendIdServerRpc(_idPlayer);
    }

    protected override abstract void Despawn();
    protected abstract void UpdateHpBar(float oldValue, float newValue);
    public abstract void TakeDamaged(int damage);
}