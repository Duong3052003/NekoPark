using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ShootBehaviour : StateMachineBehaviour
{
    public float time;
    public float minTime;
    public float maxTime;

    private Transform playerPos;
    private Vector2 target;
    private GameObject enemy;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playerPos = Target().GetComponent<Transform>();
        time = Random.Range(minTime, maxTime);
        target = new Vector2(playerPos.position.x, playerPos.position.y);
        enemy = animator.gameObject;
        enemy.GetComponent<EnemyBehaviour>().ChangeStyle(1);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (time <= 0)
        {
            animator.SetTrigger("Idle");
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    private GameObject Target()
    {
        List<GameObject> validTargets = PlayerManager.Instance.players;

        return RandomGameObjectFromList.GetRandomGameObject(validTargets);
    }
}
