using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DeSpawnObj();
    }

    private void DeSpawnObj()
    {
        CancelInvoke();
        spawner.DeSpawn(this.gameObject);
    }
}
