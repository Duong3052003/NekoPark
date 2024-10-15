using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public abstract class ItemTrigger : DeSpawn, IItemTrigger, IObjectServerSpawn
{
    [SerializeField] protected float velocityX;
    [SerializeField] protected float velocityY;
    protected Rigidbody2D rb;

    [SerializeField] protected AudioClip SoundEffect;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || !IsHost || Target(collision)== null) return;
        SoundManager.Instance.PlaySound(SoundEffect);
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

    public void Spawn(Vector3 inputVector, Vector2 velocityVector)
    {
        this.transform.position = inputVector;
        velocityX= velocityVector.x;
        velocityY= velocityVector.y;
    }

    public void DeSpawn()
    {
        Destroy(this.gameObject);
    }
}
