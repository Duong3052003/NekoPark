using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemDebuff : ItemTrigger
{
    protected virtual void Update()
    {
        Move(velocityX, velocityY);
    }

    protected override void Move(float _x, float _y)
    {
        rb.velocity = new Vector2(_x, _y);
    }

    public override void Effect(GameObject _gameObject)
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

        return RandomGameObjectFromList.GetRandomGameObject(validTargets);
    }
}
