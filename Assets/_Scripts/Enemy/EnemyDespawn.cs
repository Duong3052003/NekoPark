using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDespawn : ObjDeSpawnByHp
{
    protected Animator animator;
    protected Collider2D col;

    protected bool beingDetroyed=false;

    protected void Awake()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    public override void TakeDamaged(int damage)
    {
        if (!IsOwner) return;
        TakeDamagedServerRpc(damage);
    }

    public override void SettingObjIfAlreadyInScene(float _float)
    {
        
    }

    public virtual void CallDespawn()
    {
        if (beingDetroyed == true) return;
        beingDetroyed = true;
        col.enabled = false;
        animator.SetTrigger("expl");
    }

    protected override void Despawn()
    {
        if (beingDetroyed == true || animator == null) return;
        beingDetroyed = true;
        col.enabled = false;
        animator.SetTrigger("expl");
    }

    protected void BeDestroyed()
    {
        DestroyServerRPC();
    }
}
