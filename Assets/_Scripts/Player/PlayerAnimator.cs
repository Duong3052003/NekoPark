using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    private PlayerCtrl playerCtrl;
  
    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
    }

    void Update()
    {
        if(!IsLocalPlayer) return;
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
        playerCtrl.animator.SetFloat("velocityX", Mathf.Abs(InputManager.Instance.InputDownHorizon()));
    }


}
