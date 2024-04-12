using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGroundColiision : MonoBehaviour, ICollisionGroundChecker
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Collider2D col;
    private bool isGrounded = false;

    public bool IsGrounded()
    {
        return isGrounded;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded=true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
        }
    }
}
