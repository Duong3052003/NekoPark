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
    [SerializeField] private int styleCurrent = 1;
    [SerializeField] private int styleFirst = 1;
    [SerializeField] private GameObject turbo;

    [SerializeField] private bool lockStyle = false;

    [SerializeField] private bool isStraght = true;
    private Coroutine coroutineCurrent;
    [SerializeField] private Transform targetTransformInScene;

    public NetworkVariable<Vector3> nTargetTransform = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<Vector3> nPosCurrent = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<float> nRange = new NetworkVariable<float>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    protected override void Awake()
    {
        bulletSpawner = GetComponent<BulletSpawner>();
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
        if (IsHost)
        {
            nPosCurrent.Value = this.transform.position;
        }
        reconciliationThreshold = 2.5f;
        ChangeStyleClientRpc(-1);
        bulletSpawner.StartCoroutineSpawn();
    }

    public override void Spawn(Vector3 inputVector, Vector2 velocityVector)
    {
        if (targetTransformInScene != null && IsHost)
        {
            nTargetTransform.Value = targetTransformInScene.transform.position;
            lockStyle = false;
            canMove = true;
        }

        styleFirst = (int)velocityVector.x;
        SetCanMoveClientRpc(true);
    }

    #region Time
    private void SetTimeChangeStyle(float _time)
    {
        time = _time;
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
            reconciliationThreshold = 0.5f;

            if (nTargetTransform.Value == Vector3.zero && IsHost)
            {
                nTargetTransform.Value = LevelShipGenerator.Instance.Target().transform.position;
            }
            Movement((nTargetTransform.Value - nPosCurrent.Value).normalized);
        }
        //Conditions
        else
        {
            float distance = (nTargetTransform.Value - transform.position).magnitude;
            if (distance <= 1f)
            {
                if (IsHost)
                {
                    nPosCurrent.Value = this.transform.position;
                }
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
            speed = 15f;
            canMove = true;

            if (IsHost)
            {
                nTargetTransform.Value = LevelShipGenerator.Instance.Target().transform.position;
            }
            Movement((nTargetTransform.Value - nPosCurrent.Value).normalized);
        }
        //Conditions
        else
        {
            float distance = (nTargetTransform.Value - nPosition.Value).magnitude;
            if (distance <= 1f)
            {
                if (!IsHost) return;
                nPosCurrent.Value = this.transform.position;
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
            SetTimeChangeStyle(3);
            isStraght = true;
            bulletSpawner.canSpawn = false;
            speed = 3;
            canMove = true;

            if (IsHost)
            {
                nRange.Value = 3;
            }
        }
        //Conditions
        else
        {
            float distance = Math.Abs((nPosCurrent.Value.x + nRange.Value) - nPosition.Value.x);
            if (distance <= 0.2f && IsHost)
            {
                nRange.Value *= -1;
            }
            Movement(new Vector3(nPosCurrent.Value.x + nRange.Value, nPosCurrent.Value.y, 0));

            if (!ExcuteTime()) return;
            float distanceEnd = Math.Abs((nPosCurrent.Value.x) - nPosition.Value.x);
            if (distanceEnd > 0.1f || !IsHost) return;
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
            SetTimeChangeStyle(8);

            isStraght = true;
            bulletSpawner.canSpawn = true;
            speed = 5;
            canMove = true;
            if(IsHost)
            {
                nRange.Value = 3;
            }

        }
        //Conditions
        else
        {
            float distance = Math.Abs((nPosCurrent.Value.x + nRange.Value) - nPosition.Value.x);
            if (distance <= 0.2f && IsHost)
            {
                nRange.Value *= -1;
            }
            Movement(new Vector3(nPosCurrent.Value.x + nRange.Value, nPosCurrent.Value.y, 0));

            if (!ExcuteTime()) return;
            float distanceEnd = Math.Abs((nPosCurrent.Value.x) - nPosition.Value.x);
            if (distanceEnd > 0.1f || !IsHost) return;
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
            speed = 25f;

            lockStyle = true;

            TurnTurboClientRpc(true);
            coroutineCurrent = StartCoroutine(Chase(3f));
        }
        //Conditions
        else
        {
            float distance = (nPosCurrent.Value - nPosition.Value).magnitude;
            if (distance < 0.2f)
            {
                StopCoroutine(coroutineCurrent);
                coroutineCurrent = null;
                if (!IsHost) return;
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
        if(IsHost)
        {
            nTargetTransform.Value = Target().GetComponent<Transform>().position;
        }
        yield return new WaitForSeconds(_time);
        canMove = true;
        Vector3 posTarget = nTargetTransform.Value;
        Movement((posTarget - nPosCurrent.Value).normalized);

        yield return new WaitForSeconds(_time);
        Movement((nPosCurrent.Value - posTarget).normalized);
        lockStyle = false;
    }

    private GameObject Target()
    {
        List<GameObject> validTargets = PlayerManager.Instance.players;

        return RandomGameObjectFromList.GetRandomGameObject(validTargets);
    }

    protected override void ReconcileTransform()
    {
        if (IsOwner)
        {
            nPosition.Value = transform.position;
            nRotation.Value = transform.rotation;
        }
        else
        {
            transform.rotation = nRotation.Value;
            transform.position = nPosition.Value;
        }
    }

    /*[ServerRpc]
    public void GetNewTarget()
    {
        *//*randomValue = Random.Range(0, 100);

        SendRandomValueToClientsClientRpc(randomValue);*//*
    }

    [ClientRpc]
    private void SendRandomValueToClientsClientRpc(int value)
    {
        //randomValue = value;
    }*/
    #endregion

    #region StyleFour
    private void StyleFour(bool isSetting)
    {
        //Setting
        if (isSetting)
        {
            SetTimeChangeStyle(6);

            isStraght = false;
            bulletSpawner.canSpawn = true;
            speed = 0;
            canMove = true;
        }
        //Conditions
        else
        {
            if (IsHost && nTargetTransform.Value != null)
            {
                var targetPos = Target().GetComponent<Transform>().position;
                if(targetPos != null)
                {
                    nTargetTransform.Value = targetPos;
                }
                else
                {
                    nTargetTransform.Value = transform.position;
                }
            }
            Movement((nTargetTransform.Value - nPosCurrent.Value).normalized);
            if (!ExcuteTime() || !IsHost) return;
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
