using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BallCollider : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    [ServerRpc(RequireOwnership =false)]
    void HandleCollisionServerRpc(Vector3 forceDirection, float forceMagnitude)
    {
        rb.AddForce(forceDirection * forceMagnitude, ForceMode2D.Impulse);
        Debug.Log(3);


        if (IsServer)
        {
            ApplyForceClientRpc(forceDirection, forceMagnitude);
        }
    }

    [ClientRpc]
    void ApplyForceClientRpc(Vector3 forceDirection, float forceMagnitude)
    {
        if (rb != null)
        {
            
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.tag);

        if (collision.gameObject.CompareTag("Breakout"))
        {
            Debug.Log("Va cham dung");

            Vector3 forceDirection = collision.contacts[0].normal.normalized;
            forceDirection.z = 0;
            Debug.Log(forceDirection);
            float forceMagnitude = 15f;

            HandleCollisionServerRpc(forceDirection, forceMagnitude);
        }
    }
}
