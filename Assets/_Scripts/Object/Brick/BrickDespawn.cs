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

    public void TakeDamaged(int damage)
    {
        TakeDamagedServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamagedServerRpc(int damage)
    {
        hpCurrent.Value = hpCurrent.Value - damage;
    }

    private void Breaked(float _hpCurrent)
    {
        hpText.text = hpCurrent.Value.ToString();
        if (!CanDespawn()) return;

        var canDropItem = this.GetComponent<ItemDrop>();

        if (canDropItem != null)
        {
            canDropItem.DropItem();
        }

        Despawn();
    }

    protected override void Despawn()
    {
        DespawnClientRpc();
    }

    [ClientRpc]
    protected virtual void DespawnClientRpc()
    {
        LevelGenerator.Instance.DeSpawn(this.gameObject);
    }
}
