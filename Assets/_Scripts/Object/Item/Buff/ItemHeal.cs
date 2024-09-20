using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemHeal : ItemBuff
{
    [SerializeField] private int valueHeal = 1;

    public override void Effect(GameObject _gameObject)
    {
        var hpPlayer = _gameObject.GetComponent<ITakeDamaged>();
        hpPlayer.TakeDamaged(valueHeal * -1);
        Debug.Log("gameObject" + _gameObject + valueHeal);
    }
}
