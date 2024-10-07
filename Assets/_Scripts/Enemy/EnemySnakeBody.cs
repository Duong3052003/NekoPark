using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Networking.Transport.NetworkDriver;

public class EnemySnakeBody : EnemyBehaviour
{
    private GameObject target;
    private Vector3 posCurrent;

    public float distanceBetween;

    public void ChangeTarget(GameObject newTarget)
    {
        target = newTarget;
        SetCanMoveClientRpc(true);
    }

    protected override void Update()
    {
        if (!canMove) return;
        if(target == null)
        {
            this.GetComponent<EnemyDespawn>().CallDespawn();
        }
        if (target == null || distanceBetween >= (target.transform.position - transform.position).magnitude) return;
        Move(velocityX, velocityY);
        RotateShip(GetMovement());
        Movement((target.transform.position - transform.position).normalized);
    }

    #region StyleZero
    private GameObject Target()
    {
        return RandomGameObjectFromList.GetRandomGameObject(PlayerManager.Instance.players);
    }

    public IEnumerator Chase(float _time)
    {
        target = Target();
        posCurrent = this.transform.position;
        Movement((target.transform.position - posCurrent).normalized);
        yield return new WaitForSeconds(_time);
        canMove = true;
        yield return new WaitForSeconds(_time);
        Movement((posCurrent - this.transform.position).normalized);
    }
    #endregion

    #region Movement
    protected override void Move(float _x, float _y)
    {
        rb.velocity = new Vector3(_x, _y) * speed;
    }

    protected override void RotateShip(Vector3 targetVector)
    {
        if (targetVector != Vector3.zero)
        {
            float angle = Mathf.Atan2(targetVector.y, targetVector.x) * Mathf.Rad2Deg - 90f;

            rb.rotation = Mathf.LerpAngle(rb.rotation, angle, Time.deltaTime * rotationSpeed);
        }
        else
        {
            rb.rotation = 0;
        }
    }
    #endregion
}
