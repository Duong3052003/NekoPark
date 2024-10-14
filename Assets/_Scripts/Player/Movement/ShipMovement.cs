using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShipMovement : PlayerMove
{
    public float rotationSpeed = 10f;

    void Start()
    {
     /*   Cursor.visible = false;

        Cursor.lockState = CursorLockMode.Confined;*/
    }

    public void Move(Vector2 inputVector)
    {
        transform.position = Vector2.MoveTowards(transform.position, inputVector, speed * Time.deltaTime);
        RotateShip();
    }

    public override void Movement(Vector3 inputVector)
    {
        if (playerCtrl.rb.bodyType == RigidbodyType2D.Static) return;
        Move(inputVector);
    }

    protected override Vector2 MoveInput()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return mousePosition;
    }

    void RotateShip()
    {
        if (MoveInput().x < transform.position.x)
        {
            Quaternion targetRotation = Quaternion.Euler(0, -30, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else if (MoveInput().x > transform.position.x)
        {
            Quaternion targetRotation = Quaternion.Euler(0, 30, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
