using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BoxCollision : CheckCollision
{
    [SerializeField] private ulong ownerClientId=1;
    [SerializeField] private Rigidbody2D rb;

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (!collision.gameObject.GetComponent<NetworkObject>().NetworkObjectId.Equals(ownerClientId)) return;
        base.OnCollisionEnter2D (collision);
    }

    protected override void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (!collision.gameObject.GetComponent<NetworkObject>().NetworkObjectId.Equals(ownerClientId)) return;
        base.OnCollisionStay2D (collision);
    }

    protected override void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (!collision.gameObject.GetComponent<NetworkObject>().NetworkObjectId.Equals(ownerClientId)) return;
        base.OnCollisionExit2D(collision);
    }
}
