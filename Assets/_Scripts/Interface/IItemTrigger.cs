using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemTrigger
{
    void Effect(GameObject _gameObject);
    GameObject Target(Collider2D _collision);
}
