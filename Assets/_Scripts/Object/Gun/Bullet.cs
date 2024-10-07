using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D col;
    [SerializeField] private float speed = 5f;
    private Vector2 vectorTarget;
    private Spawner spawner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void SetTarget(Vector2 position, Vector2 newTarget, Spawner _spawner)
    {
        this.transform.position = position;
        spawner = _spawner;
        vectorTarget = (position - newTarget).normalized;
        rb.velocity = vectorTarget * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);
        spawner.DeSpawn(this.gameObject);
    }
}
