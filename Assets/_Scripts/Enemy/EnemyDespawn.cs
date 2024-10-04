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

    public override void TakeDamaged(int damage)
    {
        if (!IsOwner) return;
        TakeDamagedServerRpc(damage);
    }

    public override void SettingObjIfAlreadyInScene(float _float)
    {
        
    }

    protected override void Despawn()
    {
        if (beingDetroyed == true) return;
        beingDetroyed = true;
        col.enabled = false;
        enemyBehaviour.animator.SetTrigger("expl");
    }

    protected void BeDestroyed()
    {
        DestroyServerRPC();
    }
}
