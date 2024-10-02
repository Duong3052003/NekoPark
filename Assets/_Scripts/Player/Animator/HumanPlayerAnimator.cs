using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayerAnimator : PlayerAnimator
{
    protected override void Action()
    {
        Jump();
        Move();
    }

    void Jump()
    {
        playerCtrl.animator.SetFloat("velocityY", playerCtrl.rb.velocity.y);
        playerCtrl.animator.SetBool("isJumping", !playerCtrl.checkGroundColiision.IsGrounded());
    }

    void Move()
    {
        playerCtrl.animator.SetFloat("velocityX", Mathf.Abs(InputManager.InputHorizon()));
    }
}
