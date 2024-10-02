using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDespawn : ObjDeSpawnByHp
{
    private EnemyBehaviour enemyBehaviour;

    private bool beingDetroyed=false;

    private void Awake()
    {
        enemyBehaviour = GetComponent<EnemyBehaviour>();
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
        //do somthing
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
