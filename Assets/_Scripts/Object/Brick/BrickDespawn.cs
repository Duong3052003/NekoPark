using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BrickDespawn : DeSpawnByHp, ITakeDamaged
{
    protected NetworkVariable<int> hpMax = new NetworkVariable<int>(
       0,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server);

    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Gradient gradient;

    public override void OnNetworkSpawn()
    {
        hpText.text = hpCurrent.Value.ToString();
        hpCurrent.OnValueChanged += (oldValue, newValue) => Breaked(newValue);
    }

    private void Start()
    {
        if (hpMax.Value == 0)
        {
            SetHp(3);
            SetColorGradientClientRpc(1/5f);
            hpText.text = hpCurrent.Value.ToString();
        }
    }

    public void SetHp(int _hp)
    {
        this.hpMax.Value = _hp;
        hpCurrent.Value = hpMax.Value;
    }

    [ClientRpc]
    public void SetColorGradientClientRpc(float _colorGradient)
    {
        this.GetComponent<SpriteRenderer>().color = gradient.Evaluate(_colorGradient);
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
        DestroyServerRPC();
    }

    [ServerRpc]
    protected virtual void DestroyServerRPC()
    {
        NetworkObject netObj = this.GetComponent<NetworkObject>();
        netObj.Despawn();
        Destroy(netObj.gameObject);
    }

    [ClientRpc]
    protected virtual void DespawnClientRpc()
    {
        LevelGenerator.Instance.DeSpawn(this.gameObject);
    }
}
