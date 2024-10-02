using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BrickDespawn : ObjDeSpawnByHp
{
    [SerializeField] private Gradient gradient;

    [ClientRpc]
    public void SetColorGradientClientRpc(float _colorGradient)
    {
        this.GetComponent<SpriteRenderer>().color = gradient.Evaluate(_colorGradient);
    }

    public override void SettingObjIfAlreadyInScene(float _float)
    {
        SetColorGradientClientRpc(_float);
    }
}
