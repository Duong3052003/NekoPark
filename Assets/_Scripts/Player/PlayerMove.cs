using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class PlayerMove : NetworkBehaviour
{
    private PlayerCtrl playerCtrl;

    [SerializeField] private float speed = 7f;
    [SerializeField] private float jumpPower = 18f;
    private bool rightCheck = true;
    [SerializeField] private float gravitymin = 5f;
    [SerializeField] private float gravitymax = 7f;

    private void Awake()
    {
        playerCtrl= GetComponent<PlayerCtrl>();
    }

    private void FixedUpdate()
    {
        if (!IsLocalPlayer) return;
        Jump();
        Move();
        GravityChanged();
    }

    public void Move()
    {
        int keyMoveDown = InputManager.Instance.InputDownHorizon();
        
        if ((rightCheck == true && keyMoveDown < 0f || rightCheck == false && keyMoveDown > 0f))
        {
            Flip();
        }

        playerCtrl.rb.velocity = new Vector2(keyMoveDown * speed, playerCtrl.rb.velocity.y);
    }

    private void Jump()
    {
        int keyJumpDown = InputManager.Instance.InputDownVertical();
        int keyJumpUp = InputManager.Instance.InputUpVertical();

        if (playerCtrl.checkGroundColiision.IsGrounded())
        {
            if (keyJumpDown == 1)
            {
                playerCtrl.rb.velocity = new Vector2(playerCtrl.rb.velocity.x, jumpPower);
            }
        }

        if (keyJumpUp == 1)
        {
            playerCtrl.rb.velocity = new Vector2(playerCtrl.rb.velocity.x, playerCtrl.rb.velocity.y * 0.5f);
        }
    }

    private void Flip()
    {
        rightCheck = !rightCheck;

        if (rightCheck == true)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        }
    }

    private void GravityChanged()
    {
        if (playerCtrl.rb.velocity.y < 0)
        {
            playerCtrl.rb.gravityScale=gravitymax;
        }
        else
        {
            playerCtrl.rb.gravityScale = gravitymin;
        }
    }
}

