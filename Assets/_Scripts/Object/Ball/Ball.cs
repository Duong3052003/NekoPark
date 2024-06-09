using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    public int damage = 1;
    public float maxVelocity = 15f;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        VelocityIsMax();
    }

    private void VelocityIsMax()
    {
        if(rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ChangedGravity();

        ITakeDamaged objTakeDamaged = collision.gameObject.GetComponent<ITakeDamaged>();
        if (objTakeDamaged != null )
        {
            objTakeDamaged.TakeDamaged(damage);
            Debug.Log(objTakeDamaged + "take "+ damage + " damage");
        }
    }

    private void ChangedGravity()
    {
        if (rb.gravityScale != 0f) return;
        rb.gravityScale = 0.05f;
    }
}
