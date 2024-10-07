using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Unity.Networking.Transport.NetworkDriver;

public class EnemyPatrol : EnemyBehaviour
{
    public BulletSpawner bulletSpawner { get; private set; }
    [SerializeField] private float time;

    //Style
    [SerializeField] private float rangeX = 0;
    [SerializeField] private int styleCurrent = 1;
    [SerializeField] private int styleFirst = 1;
    [SerializeField] private Vector3 posCurrent;
    [SerializeField] protected Transform transformSpawnNeedToGo;
    [SerializeField] protected Vector3 posTransformSpawnNeedToGo;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private GameObject turbo;

    [SerializeField] private bool lockStyle = false;

    [SerializeField] private bool isStraght = true;
    private Coroutine coroutineCurrent;

    protected override void Awake()
    {
        bulletSpawner = GetComponent<BulletSpawner>();
        reconciliationThreshold = 10f;
        base.Awake();
    }

    protected override void Update()
    {
        CheckConditions();
        base.Update();
    }

    [ClientRpc]
    public override void SetCanMoveClientRpc(bool boolen)
    {
        Setup();
    }

    private void Setup()
    {
        /*posCurrent = this.transform.position;
        if (posTransformSpawnNeedToGo != null)
        {
            posSpawnNeedToGo = posTransformSpawnNeedToGo.transform.position;
            ChangeStyle(0);
        }
        else if (posSpawnNeedToGo != Vector3.zero)
        {
            ChangeStyle(0);
        }
        else
        {
            ChangeStyle(styleFirst);
        }*/
        posCurrent = this.transform.position;

        ChangeStyleClientRpc(-1);
        bulletSpawner.StartCoroutineSpawn();
    }

    public override void Spawn(Vector3 inputVector, Vector2 velocityVector)
    {
        /*if(inputVector != null)
        {
            posSpawnNeedToGo = inputVector;
        }

        if ((int)velocityVector.x == 0)
        {
            styleFirst = 1;
        }
        else
        {
            styleFirst = (int)velocityVector.x;
        }*/
        styleFirst = (int)velocityVector.x;

        SetCanMoveClientRpc(true);
    }

    #region Time
    private void SetTimeChangeStyle(float minTime, float maxTime)
    {
        time = UnityEngine.Random.Range(minTime, maxTime);
    }

    private bool ExcuteTime()
    {
        time -= Time.deltaTime;
        if (time <= 0)
        {
            return true;
        }

        return false;
    }
    #endregion

    #region Style
    private void CheckConditions()
    {
        if (lockStyle) return;
        switch (styleCurrent)
        {
            case -1:
                StyleBegin(false);
                break;
            case 0:
                StyleZero(false);
                break;
            case 1:
                StyleOne(false);
                break;
            case 2:
                StyleTwo(false);
                break;
            case 3:
                StyleThree(false);
                break;
            case 4:
                StyleFour(false);
                break;
            default:
                StyleOne(true);
                break;
        }
    }

    [ClientRpc]
    public void ChangeStyleClientRpc(int index)
    {
        if (coroutineCurrent != null)
        {
            StopCoroutine(coroutineCurrent);
            coroutineCurrent = null;
        }

        rb.velocity = new Vector3(0, 0);

        switch (index)
        {
            case -1:
                StyleBegin(true);
                styleCurrent = index;
                break;
            case 0:
                StyleZero(true);
                styleCurrent = index;
                break;
            case 1:
                StyleOne(true);
                styleCurrent = index;
                break;
            case 2:
                StyleTwo(true);
                styleCurrent = index;
                break;
            case 3:
                StyleThree(true);
                styleCurrent = index;
                break;
            case 4:
                StyleFour(true);
                styleCurrent = index;
                break;
            default:
                StyleOne(true);
                styleCurrent = 1;
                break;
        }
    }
    #endregion

    #region StyleBegin
    private void StyleBegin(bool isSetting)
    {
        //Setting
        if (isSetting)
        {
            isStraght = false;
            bulletSpawner.canSpawn = false;
            speed = 12f;
            canMove = true;

            if(transformSpawnNeedToGo== null)
            {
                transformSpawnNeedToGo = LevelShipGenerator.Instance.Target().transform;
            }
            posTransformSpawnNeedToGo = transformSpawnNeedToGo.position + new Vector3(UnityEngine.Random.Range(-3, 3), UnityEngine.Random.Range(-3, 3), 0);
            Movement((posTransformSpawnNeedToGo - posCurrent).normalized);
        }
        //Conditions
        else
        {
            float distance = (posTransformSpawnNeedToGo - transform.position).magnitude;
            if (distance <= 1f)
            {
                posCurrent = this.transform.position;
                ChangeStyleClientRpc(styleFirst);
            }
        }
    }
    #endregion

    #region StyleZero
    private void StyleZero(bool isSetting)
    {
        //Setting
        if (isSetting)
        {
            isStraght = false;
            bulletSpawner.canSpawn = false;
            speed = 12f;
            canMove = true;

            transformSpawnNeedToGo = LevelShipGenerator.Instance.Target().transform;
            Movement((transformSpawnNeedToGo.position - posCurrent).normalized);
        }
        //Conditions
        else
        {
            float distance = (transformSpawnNeedToGo.position - transform.position).magnitude;
            if (distance <= 1f)
            {
                posCurrent = this.transform.position;
                ChangeStyleClientRpc(UnityEngine.Random.Range(1, 5));
            }
        }
    }
    #endregion

    #region StyleOne
    private void StyleOne(bool isSetting)
    {
        //Setting
        if (isSetting)
        {
            SetTimeChangeStyle(2, 3);
            isStraght = true;
            bulletSpawner.canSpawn = false;
            speed = UnityEngine.Random.Range(0, 4);
            canMove = true;

            int direct = UnityEngine.Random.Range(0, 2);
            if (direct == 0)
            {
                rangeX = 3;
            }
            else
            {
                rangeX = - 3;
            }
        }
        //Conditions
        else
        {
            float distance = Math.Abs((posCurrent.x + rangeX) - this.transform.position.x);
            if (distance <= 0.2f)
            {
                rangeX *= -1;
            }
            Movement(new Vector3(posCurrent.x + rangeX, posCurrent.y, 0));

            if (!ExcuteTime()) return;
            float distanceEnd = Math.Abs((posCurrent.x) - this.transform.position.x);
            if (distanceEnd > 0.1f) return;
            ChangeStyleClientRpc(UnityEngine.Random.Range(2, 5));
        }
    }
    #endregion

    #region StyleTwo
    private void StyleTwo(bool isSetting)
    {
        //Setting
        if (isSetting)
        {
            SetTimeChangeStyle(8, 10);

            isStraght = true;
            bulletSpawner.canSpawn = true;
            speed = UnityEngine.Random.Range(0, 6);
            canMove = true;

            int direct = UnityEngine.Random.Range(0, 2);
            if (direct == 0)
            {
                rangeX = 3;
            }
            else
            {
                rangeX = - 3;
            }
        }
        //Conditions
        else
        {
            float distance = Math.Abs((posCurrent.x + rangeX) - this.transform.position.x);
            if (distance <= 0.2f)
            {
                rangeX *= -1;
            }
            Movement(new Vector3(posCurrent.x + rangeX, posCurrent.y, 0));

            if (!ExcuteTime()) return;
            float distanceEnd = Math.Abs((posCurrent.x) - this.transform.position.x);
            if (distanceEnd > 0.1f) return;
            ChangeStyleClientRpc(UnityEngine.Random.Range(0, 5));
        }
    }
    #endregion

    #region StyleThree
    private void StyleThree(bool isSetting)
    {
        //Setting
        if (isSetting)
        {
            isStraght = false;
            bulletSpawner.canSpawn = false;
            canMove = false;
            speed = 20f;

            lockStyle = true;

            TurnTurboClientRpc(true);
            coroutineCurrent = StartCoroutine(Chase(3f));
        }
        //Conditions
        else
        {
            float distance = (posCurrent - this.transform.position).magnitude;
            if (distance < 0.2f)
            {
                StopCoroutine(coroutineCurrent);
                coroutineCurrent = null;
                TurnTurboClientRpc(false);
                ChangeStyleClientRpc(UnityEngine.Random.Range(0, 5));
            }
        }
    }

    [ClientRpc]
    private void TurnTurboClientRpc(bool boolen)
    {
        turbo.SetActive(boolen);
    }

    public IEnumerator Chase(float _time)
    {
        targetTransform = Target().GetComponent<Transform>();
        yield return new WaitForSeconds(_time);
        canMove = true;
        Vector3 posTarget = targetTransform.position;
        Movement((posTarget - posCurrent).normalized);

        yield return new WaitForSeconds(_time);
        Movement((posCurrent - posTarget).normalized);
        lockStyle = false;
    }

    private GameObject Target()
    {
        List<GameObject> validTargets = PlayerManager.Instance.players;

        return RandomGameObjectFromList.GetRandomGameObject(validTargets);
    }
    #endregion

    #region StyleFour
    private void StyleFour(bool isSetting)
    {
        //Setting
        if (isSetting)
        {
            SetTimeChangeStyle(5, 7);

            isStraght = false;
            bulletSpawner.canSpawn = true;
            speed = 0;
            canMove = true;
        }
        //Conditions
        else
        {
            targetTransform = Target().GetComponent<Transform>();
            Movement((targetTransform.position - posCurrent).normalized);
            if (!ExcuteTime()) return;
            ChangeStyleClientRpc(UnityEngine.Random.Range(0, 5));
        }
    }
    #endregion

    #region Movement
    protected override void Move(float _x, float _y)
    {
        if (styleCurrent == -1 || styleCurrent == 0)
        {
            rb.velocity = new Vector3(_x, _y) * speed;
        }
        else if (styleCurrent == 1 || styleCurrent == 2)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(_x, _y, 0), Time.deltaTime * speed);
        }
        else if (styleCurrent == 3 || styleCurrent == 4)
        {
            rb.velocity = new Vector3(_x, _y) * speed;
        }
    }

    protected override void RotateShip(Vector3 targetVector)
    {
        if (isStraght)
        {
            RotateShipX(targetVector);
        }
        else
        {
            RotateShipZ(targetVector);
        }
    }

    void RotateShipX(Vector3 targetVector)
    {
        if (targetVector.x < transform.position.x)
        {
            Quaternion targetRotation = Quaternion.Euler(0, -45, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed / 2 * Time.deltaTime);
        }
        else if (targetVector.x > transform.position.x)
        {
            Quaternion targetRotation = Quaternion.Euler(0, 45, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed / 2 * Time.deltaTime);
        }
        else
        {
            Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed / 2 * Time.deltaTime);
        }
    }

    void RotateShipZ(Vector3 targetVector)
    {
        if (targetVector != Vector3.zero)
        {
            float angle = Mathf.Atan2(targetVector.y, targetVector.x) * Mathf.Rad2Deg + 90f;

            rb.rotation = Mathf.LerpAngle(rb.rotation, angle, Time.deltaTime * rotationSpeed);
        }
        else
        {
            rb.rotation = 0;
        }
    }
    #endregion

}
