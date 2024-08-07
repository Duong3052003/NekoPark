using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public abstract class ItemTrigger : DeSpawn, IItemTrigger
{
    [SerializeField] protected float velocityX;
    [SerializeField] protected float velocityY;
    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || !IsHost) return;
        Effect(Target(collision));
        Despawn();
    }

    protected override bool CanDespawn()
    {
        return true;
    }

    protected override void Despawn()
    {
        this.gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(this.gameObject);
    }

    protected abstract void Move(float _x, float _y);
    public abstract void Effect(GameObject _gameObject);
    public abstract GameObject Target(Collider2D _collision);
}
