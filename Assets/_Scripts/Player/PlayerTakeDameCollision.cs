using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerTakeDameCollision : NetworkBehaviour
{
    private PlayerCtrl playerCtrl;
    [SerializeField] private LayerMask takeDameLayer;

    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & takeDameLayer) != 0)
        {
            BeingDead();
        }
    }

    private void BeingDead()
    {
        //Death
    }

}
