using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanMovement : PlayerMove
{
    [Header("Physics")]
    [SerializeField] private float jumpPower = 18f;
    [SerializeField] private float gravitymin = 5f;
    [SerializeField] private float gravitymax = 7f;

    [Header("Others")]
    private bool rightCheck = true;

    public void Move(Vector2 inputVector)
    {
        if ((rightCheck == true && inputVector.x < 0f || rightCheck == false && inputVector.x > 0f))
        {
            Flip();
        }

        playerCtrl.rb.velocity = new Vector2(inputVector.x * speed, playerCtrl.rb.velocity.y);
    }

    public void Jump(Vector2 inputVector)
    {
        if (playerCtrl.checkGroundColiision.IsGrounded())
        {
            if (inputVector.y == 1)
            {
                playerCtrl.rb.velocity = new Vector2(playerCtrl.rb.velocity.x, jumpPower);
            }
        }
    }

    private void Flip()
    {
        /*rightCheck = !rightCheck;

        if (rightCheck == true)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        }*/
        rightCheck = !rightCheck;

        transform.rotation = rightCheck ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, -180f, 0f);
    }

    public override void Movement(Vector3 inputVector)
    {
        if (playerCtrl.rb.bodyType == RigidbodyType2D.Static) return;
        Move(inputVector);
        Jump(inputVector);
    }

    protected override Vector2 MoveInput()
    {
        return new Vector2(InputManager.InputHorizon(), InputManager.InputVertical());
    }

    private void GravityChanged()
    {
        /*if (playerCtrl.rb.velocity.y < 0)
        {
            playerCtrl.rb.gravityScale = gravitymax;
        }
        else
        {
            playerCtrl.rb.gravityScale = gravitymin;
        }*/

        playerCtrl.rb.gravityScale = playerCtrl.rb.velocity.y < 0 ? gravitymax : gravitymin;
    }
}
