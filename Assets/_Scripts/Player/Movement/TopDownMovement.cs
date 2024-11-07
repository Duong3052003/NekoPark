using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownMovement : PlayerMove
{
    /*[Header("Physics")]
    [SerializeField] private float jumpPower = 18f;
    [SerializeField] private float gravitymin = 5f;
    [SerializeField] private float gravitymax = 7f;

    [Header("Others")]
    private bool rightCheck = true;*/

    public void Move(Vector2 inputVector)
    {
        playerCtrl.rb.velocity = new Vector2(inputVector.x * speed, inputVector.y * speed);
    }

    public override void Movement(Vector3 inputVector)
    {
        if (playerCtrl.rb.bodyType == RigidbodyType2D.Static) return;
        Move(inputVector);
    }

    protected override Vector2 MoveInput()
    {
        return new Vector2(InputManager.InputHorizon(), InputManager.InputVertical());
    }
}
