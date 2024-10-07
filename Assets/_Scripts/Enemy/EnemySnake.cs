using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class EnemySnake : SnakeObjManager
{
    private EnemySnakeBody enemyChase;
    private Transform targetTransform;

    private void Awake()
    {
        enemyChase=this.gameObject.GetComponent<EnemySnakeBody>();
    }

    private void SetTargetAllBody()
    {
        bodyParts[0].GetComponent<EnemySnakeBody>().ChangeTarget(this.gameObject);
        for (int i = 1; i < size; i++)
        {
            bodyParts[i].GetComponent<EnemySnakeBody>().ChangeTarget(bodyParts[i-1]);
        }
    }

    public override void OnPause(int time)
    {
    }

    public override void OnResume()
    {
        GenerateObjsServerRPC();
        StartCoroutine(enemyChase.Chase(3));
    }
}
