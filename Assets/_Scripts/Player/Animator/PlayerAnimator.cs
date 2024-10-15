using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public abstract class PlayerAnimator : NetworkBehaviour
{
    protected PlayerCtrl playerCtrl;

    private int playerLayer;
    private int trapLayer;
    private int enemyPlayer;

    [SerializeField] private int numberPlayerCanWin;

    private void Awake()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
        trapLayer = LayerMask.NameToLayer("Trap");
        enemyPlayer = LayerMask.NameToLayer("Enemy");
    }

    private void Start()
    {
        SetActiveLayer();
    }

    void Update()
    {
        if (!IsOwner) return;
        Action();
    }

    protected abstract void Action();

    public void SetActiveLayer()
    {
        if (playerCtrl == null) return;
        playerCtrl.animator.SetLayerWeight(1, 0);
        IgnorePhysicsLayerAllPlayer(false);
    }

    public void Desappear()
    {
        playerCtrl.animator.SetTrigger("Desappear");
    }

    private void Dead()
    {
        StopAllCoroutines();
        this.gameObject.SetActive(false);

        if (!PlayerManager.Instance.CheckGameOver(numberPlayerCanWin)) return;
        UIManager.Instance.GameOverScreen();
    }

    public IEnumerator Invisible(float _time)
    {
        playerLayer = this.gameObject.layer;

        Physics2D.IgnoreLayerCollision(playerLayer, trapLayer);
        Physics2D.IgnoreLayerCollision(playerLayer, enemyPlayer);
        playerCtrl.animator.SetLayerWeight(1,1);

        yield return new WaitForSeconds(_time);

        Physics2D.IgnoreLayerCollision(playerLayer, trapLayer, false);
        Physics2D.IgnoreLayerCollision(playerLayer, enemyPlayer, false);
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
