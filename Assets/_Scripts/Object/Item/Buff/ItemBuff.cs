using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemBuff : ItemTrigger
{
    public override void OnNetworkSpawn()
    {

    }

    protected virtual void Start()
    {

    }

    public  override void Effect(GameObject _gameObject)
    {
        Debug.Log(_gameObject + "Duoc buff");
    }

    public override GameObject Target(Collider2D _collision)
    {
        List<GameObject> validTargets = PlayerManager.Instance.players
        .Where(player => player != _collision.gameObject)
        .ToList();

        if (validTargets.Count == 0)
        {
            Debug.LogWarning("Khong co muc tieu.");
            return null;
        }

        int randomIndex = Random.Range(0, validTargets.Count);
        GameObject target = validTargets[randomIndex];

        return target;
    }
}
