using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BrickDespawn : DeSpawnByHp, ITakeDamaged
{
    [SerializeField] private int hpMax=5;
    [SerializeField] private TextMeshProUGUI hpText;

    public override void OnNetworkSpawn()
    {
        hpCurrent.Value = hpMax;
        hpText.text = hpCurrent.Value.ToString();
        hpCurrent.OnValueChanged += (oldValue, newValue) => Breaked(newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamagedServerRpc(int damage)
    {
        hpCurrent.Value = hpCurrent.Value - damage;
    }

    private void Breaked(int _hpCurrent)
    {
        hpText.text = hpCurrent.Value.ToString();
        if (!CanDespawn()) return;
        Debug.Log("Breaked");
        Despawn();
    }

    protected override void Despawn()
    {
        DespawnServerRpc();
    }

    [ServerRpc]
    protected virtual void DespawnServerRpc()
    {
        LevelGenerator.Instance.DeSpawn(this.gameObject);
    }
}
