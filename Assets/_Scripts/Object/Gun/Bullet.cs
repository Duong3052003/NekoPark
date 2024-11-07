using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Collider2D col;
    [SerializeField] protected float speed = 5f;
    protected Vector2 vectorTarget;
    protected Spawner spawner;

    protected void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void SetTarget(Vector2 position, Vector2 newTarget, Spawner _spawner)
    {
        this.transform.position = position;
        spawner = _spawner;
        vectorTarget = (newTarget - position).normalized;
        rb.velocity = vectorTarget * speed;
        Invoke("DeSpawnObj", 10f);
    }

    public void SetRotate(Vector2 position, Quaternion rotate, bool isRight, Spawner _spawner)
    {
        this.transform.position = position;
        this.transform.rotation = rotate;

        spawner = _spawner;
        if (isRight == false)
        {
            rb.velocity = -transform.right * speed;
        }
        else
        {
            rb.velocity = transform.right * speed;
        }
        Invoke("DeSpawnObj", 10f);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        DeSpawnObj();
    }

    protected void DeSpawnObj()
    {
        CancelInvoke();
        spawner.DeSpawn(this.gameObject);
    }
}
