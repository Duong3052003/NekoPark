using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class DealDame : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private bool collisionDame = false;
    [SerializeField] private bool triggerDame = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collisionDame == false) return;
        ITakeDamaged objTakeDamaged = collision.gameObject.GetComponent<ITakeDamaged>();
        if (objTakeDamaged != null)
        {
            objTakeDamaged.TakeDamaged(damage);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggerDame == false) return;
        ITakeDamaged objTakeDamaged = collision.gameObject.GetComponent<ITakeDamaged>();
        if (objTakeDamaged != null)
        {
            objTakeDamaged.TakeDamaged(damage);
        }
    }
}
