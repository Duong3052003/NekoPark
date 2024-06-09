using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Brick : NetworkBehaviour,ITakeDamaged
{
    [SerializeField] private int hpMax=5;
    [SerializeField] private TextMeshProUGUI hpText;
    private NetworkVariable<int> hpCurrent = new NetworkVariable<int>(0);

    public override void OnNetworkSpawn()
    {
        hpCurrent.Value = hpMax;
        hpText.text = hpCurrent.Value.ToString();
        hpCurrent.OnValueChanged += (oldValue, newValue) => Breaked(newValue);
    }

    public void TakeDamaged(int damage)
    {
        if(IsHost || IsServer)
        {
            if (!IsSpawned) return;
            hpCurrent.Value = hpCurrent.Value - damage;
            if (!this.GetComponent<NetworkObject>().IsSpawned) return;
            TakeDamageServerRpc();
        }
    }

    private void Breaked(int _hpCurrent)
    {
        if (_hpCurrent > 0) return;
        Debug.Log("Breaked");
        DestroyServerRpc();
    }

    [ServerRpc]
    protected virtual void TakeDamageServerRpc()
    {
        hpText.text = hpCurrent.Value.ToString();
    }

    [ServerRpc]
    protected virtual void DestroyServerRpc()
    {
        this.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
