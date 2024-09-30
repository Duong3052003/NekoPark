using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    private PlayerCtrl playerCtrl;

    private int playerLayer;
    private int trapLayer;

    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
        trapLayer = LayerMask.NameToLayer("Trap");
    }

    private void Start()
    {
        playerCtrl.animator.SetLayerWeight(1, 0);
        IgnorePhysicsLayerAllPlayer(false);
    }

    void Update()
    {
        if(!IsOwner) return;
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
        StopAllCoroutines();
        this.gameObject.SetActive(false);

        if (!PlayerManager.Instance.CheckGameOver()) return;
        UIManager.Instance.GameOverScreen();
    }

    public IEnumerator Invisible(float _time)
    {
        playerLayer = this.gameObject.layer;

        Physics2D.IgnoreLayerCollision(playerLayer, trapLayer);
        playerCtrl.animator.SetLayerWeight(1,1);

        yield return new WaitForSeconds(_time);

        Physics2D.IgnoreLayerCollision(playerLayer, trapLayer, false);
        playerCtrl.animator.SetLayerWeight(1, 0);
    }

    private void IgnorePhysicsLayerAllPlayer(bool boolen)
    {
        Physics2D.IgnoreLayerCollision(10, trapLayer, boolen);
        Physics2D.IgnoreLayerCollision(11, trapLayer, boolen);
        Physics2D.IgnoreLayerCollision(12, trapLayer, boolen);
        Physics2D.IgnoreLayerCollision(13, trapLayer, boolen);
    }
}
