using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public abstract class ObjDeSpawnByHp : DeSpawnByHp, ITakeDamaged
{
    protected NetworkVariable<int> hpMax = new NetworkVariable<int>(
       0,
       NetworkVariableReadPermission.Everyone,
       NetworkVariableWritePermission.Server);

    [SerializeField] protected TextMeshProUGUI hpText;
    [SerializeField] protected int setHpIfAlreadyInScene;

    public override void OnNetworkSpawn()
    {
        UpdateHpText();
        hpCurrent.OnValueChanged += (oldValue, newValue) => Breaked(newValue);
    }

    void UpdateHpText()
    {
        if (hpText != null)
        {
            hpText.text = hpCurrent.Value.ToString();
        }
    }

    private void Start()
    {
        if (!IsHost) return;
        if (hpMax.Value == 0)
        {
            SetHp(setHpIfAlreadyInScene);
            SettingObjIfAlreadyInScene(1/5f);
            UpdateHpText();
        }
    }

    public void SetHp(int _hp)
    {
        this.hpMax.Value = _hp;
        hpCurrent.Value = hpMax.Value;
    }

    public virtual void TakeDamaged(int damage)
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
        UpdateHpText();
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

    [ServerRpc(RequireOwnership = false)]
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

    public abstract void SettingObjIfAlreadyInScene(float _float);
}