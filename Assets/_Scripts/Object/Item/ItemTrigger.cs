using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public abstract class ItemTrigger : NetworkBehaviour, IItemTrigger
{
    [SerializeField] protected float velocityX;
    [SerializeField] protected float velocityY;
    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        Move(velocityX, velocityY);
    }

    protected virtual void Move(float _x, float _y)
    {
        rb.velocity = new Vector2(_x, _y);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        Effect(Target(collision));
    }

    public abstract void Effect(GameObject _gameObject);
    public abstract GameObject Target(Collider2D _collision);
}
