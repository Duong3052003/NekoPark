using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D col;
    [SerializeField] private float speed = 5f;
    private Vector2 vectorTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void SetTarget(Vector2 position, Vector2 newTarget)
    {
        this.transform.position = position;
        vectorTarget = (position - newTarget).normalized;
        rb.velocity = vectorTarget * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BulletSpawn.Instance.DeSpawn(this.gameObject);

    }
}
