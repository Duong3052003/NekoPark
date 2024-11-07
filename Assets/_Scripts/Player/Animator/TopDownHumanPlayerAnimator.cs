using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownHumanPlayerAnimator : PlayerAnimator
{
    private bool rightCheck = false;

    protected override void Action()
    {
        Move();
    }

    void Move()
    {
        if ((rightCheck == true && InputManager.InputHorizon() < 0f || rightCheck == false && InputManager.InputHorizon() > 0f))
        {
            Flip();
        }

        playerCtrl.animator.SetFloat("Speed", new Vector2(InputManager.InputHorizon(), InputManager.InputVertical()).sqrMagnitude);

        if (InputManager.InputHorizon() == 0 && InputManager.InputVertical() == 0) return;
        playerCtrl.animator.SetFloat("Horizontal", InputManager.InputHorizon());
        playerCtrl.animator.SetFloat("Vertical", InputManager.InputVertical());
    }

    private void Flip()
    {
        rightCheck = !rightCheck;

        transform.rotation = rightCheck ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, -180f, 0f);
    }
}
