using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDespawn : ObjDeSpawnByHp
{
    private EnemyBehaviour enemyBehaviour;
    private Collider2D col;

    private bool beingDetroyed=false;

    private void Awake()
    {
        enemyBehaviour = GetComponent<EnemyBehaviour>();
        col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        BeingDestroyed();
    }

    public override void TakeDamaged(int damage)
    {
        if (!IsOwner) return;
        TakeDamagedServerRpc(damage);
    }

    public override void SettingObjIfAlreadyInScene(float _float)
    {
        
    }

    protected void BeingDestroyed()
    {
        if (beingDetroyed == false) return;
        col.enabled = false;
    }

    protected override void Despawn()
    {
        enemyBehaviour.animator.SetBool("expl",true);
    }

    protected void BeDestroyed()
    {
        DestroyServerRPC();
    }
}
