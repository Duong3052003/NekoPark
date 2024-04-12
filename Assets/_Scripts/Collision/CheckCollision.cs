using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CheckCollision : NetworkBehaviour, ICollisionChecker
{
    private bool isTouching = false;

    public bool IsTouching()
    {
        return isTouching;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        isTouching = true;
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        isTouching = true;
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        isTouching = false;
    }
}
