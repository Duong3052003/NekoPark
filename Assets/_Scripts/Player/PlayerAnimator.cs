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
        playerCtrl.animator.SetFloat("velocityX", Mathf.Abs(InputManager.InputHorizon()));
    }

    public void Desappear()
    {
        playerCtrl.animator.SetTrigger("Desappear");
    }

    private void Dead()
    {
        this.gameObject.SetActive(false);

        if (!PlayerManager.Instance.CheckGameOver()) return;

    }

    public IEnumerator Invisible(float _time)
    {
        Physics2D.IgnoreLayerCollision(3, 7);
        playerCtrl.animator.SetLayerWeight(1,1);
        yield return new WaitForSeconds(_time);
        Physics2D.IgnoreLayerCollision(3, 7, false);
        playerCtrl.animator.SetLayerWeight(1, 0);
    }
}
