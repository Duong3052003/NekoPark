using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Unity.Networking.Transport.NetworkDriver;

public class EnemyBehaviour : NetworkBehaviour, IObjectServerMovement, IObserver, IObjectServerSpawn
{
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public BulletSpawner bulletSpawner { get; private set; }
    [SerializeField] private GameObject turbo;

    public float speed = 5f;
    public float rotationSpeed = 10f;
    private bool canMove=true;
    private bool isStraght=true;

    [SerializeField] private float velocityX = 0;
    [SerializeField] private float velocityY = 0;

    [SerializeField] private float rangeX = 0;

    //Style
    [SerializeField] private int styleCurrent = 0;
    [SerializeField] private Vector3 posCurrent;
    private float time;

    //Style2
    private Transform targetTransform;

    // Netcode general
    const float k_serverTickRate = 60f; // 60 FPS
    const int k_bufferSize = 1024;
    public NetworkVariable<Vector3> nPosition = new NetworkVariable<Vector3>(new Vector3(0, 0, 0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> nRotation = new NetworkVariable<Quaternion>(Quaternion.identity , NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private float reconciliationThreshold = 2.5f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bulletSpawner = GetComponent<BulletSpawner>();
    }

    public override void OnNetworkSpawn()
    {
        ReconcileTransform();
        NetworkTimer.Instance.CurrentTick.OnValueChanged += (oldValue, newValue) => SendMovementToServer();
    }

    void Update()
    {
        CheckConditions();

        /*if (time <= 0)
        {
            ChangeStyle(UnityEngine.Random.Range(0, 3));
        }
        else
        {
            time -= Time.deltaTime;
        }*/

        if (!canMove) return;
        Move(velocityX, velocityY);
        RotateShip(GetMovement());
    }

    #region Movement
    private void Move(float _x, float _y)
    {
        this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(_x,_y, 0), Time.deltaTime * speed);
    }

    private void SendMovementToServer()
    {
        if(this == null) return;
        ReconcileTransform();

        var currentTick = NetworkTimer.Instance.CurrentTick.Value;
        var bufferIndex = currentTick % k_bufferSize;

        InputPayLoad inputPayload = new InputPayLoad()
        {
            tick = currentTick,
            timestamp = System.DateTime.Now,
            OwnerObjID = OwnerClientId,
            NetworkObjID = NetworkObjectId,
            inputVector = GetMovement(),
            position = transform.position
        };

        Server.Instance.OnClientInput(inputPayload);
    }

    public void Movement(Vector3 direction)
    {
        velocityX = direction.x;
        velocityY = direction.y;
    }

    public Vector3 GetMovement()
    {
        return new Vector3(velocityX, velocityY, 0);
    }

    private void ReconcileTransform()
    {
        if (IsOwner)
        {
            nPosition.Value = transform.position;
            nRotation.Value = transform.rotation;
        }
        else
        {
            float positionError = Vector3.Distance(nPosition.Value, transform.position);
            transform.rotation = nRotation.Value;
            if (positionError < reconciliationThreshold) return;
            transform.position = nPosition.Value;
        }
    }
    #endregion

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
        switch (styleCurrent)
        {
            case 0:
                StyleZero(false);
                break;
            case 1:
                StyleOne(false);
                break;
            case 2:
                StyleTwo(false);
                break;
            default: break;
        }
    }

    public void ChangeStyle(int index)
    {
        StopAllCoroutines();
        styleCurrent = index;
        switch (styleCurrent)
        {
            case 0:
                StyleZero(true);
                break;
            case 1:
                StyleOne(true);
                break;
            case 2:
                StyleTwo(true);
                break;
            default: break;
        }
    }
    #endregion

    #region StyleZero
    private void StyleZero(bool isSetting)
    {
        //Setting
        if (isSetting)
        {
            SetTimeChangeStyle(2, 3);

            isStraght = true;
            bulletSpawner.canSpawn = false;
            canMove = true;

            rangeX = 3;
            speed = 3;
            int direct = UnityEngine.Random.Range(0, 2);
            if (direct == 0)
            {
                Movement(new Vector3(posCurrent.x - rangeX, posCurrent.y, 0));
            }
            else
            {
                Movement(new Vector3(posCurrent.x + rangeX, posCurrent.y, 0));
            }
            posCurrent = this.transform.position;
        }
        //Conditions
        else
        {
            float distance = Math.Abs((posCurrent.x + rangeX) - this.transform.position.x);
            if (distance <= 0.2f || this.transform.position.x <= -12.5f || this.transform.position.x >= 14.5)
            {
                rangeX *= -1;
            }
            Movement(new Vector3(posCurrent.x+ rangeX, posCurrent.y, 0));

            if (!ExcuteTime()) return;
            ChangeStyle(UnityEngine.Random.Range(0, 3));
        }
    }
    #endregion

    #region StyleOne
    private void StyleOne(bool isSetting)
    {
        //Setting
        if (isSetting)
        {
            SetTimeChangeStyle(8, 10);

            isStraght = true;
            bulletSpawner.canSpawn = true;
            canMove = true;

            rangeX = 3;
            speed = 5;
            int direct = UnityEngine.Random.Range(0, 2);
            if (direct == 0)
            {
                Movement(new Vector3(posCurrent.x - rangeX, posCurrent.y, 0));
            }
            else
            {
                Movement(new Vector3(posCurrent.x + rangeX, posCurrent.y, 0));
            }
            posCurrent = this.transform.position;
        }
        //Conditions
        else
        {
            float distance = Math.Abs((posCurrent.x + rangeX) - this.transform.position.x);
            if (distance <= 0.2f || this.transform.position.x<= -12.5f || this.transform.position.x >= 14.5)
            {
                rangeX *= -1;
            }
            Movement(new Vector3(posCurrent.x + rangeX, posCurrent.y, 0));

            if (!ExcuteTime()) return;
            ChangeStyle(UnityEngine.Random.Range(0, 3));
        }
    }
    #endregion

    #region StyleTwo
    private void StyleTwo(bool isSetting)
    {
        //Setting
        if (isSetting)
        {
            isStraght = false;
            bulletSpawner.canSpawn = false;
            canMove = true;

            TurnTurboClientRpc(true);
            StartCoroutine(Chase(3f));
            posCurrent = this.transform.position;
        }
        //Conditions
        else
        {

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
        Movement(targetTransform.position + posCurrent);
    }

    private GameObject Target()
    {
        List<GameObject> validTargets = PlayerManager.Instance.players;

        return RandomGameObjectFromList.GetRandomGameObject(validTargets);
    }
    #endregion

    #region Rotate
    void RotateShip(Vector3 targetVector)
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

    #region IObserver
    private void OnEnable()
    {
        AddListObserver(this);
    }

    private void OnDisable()
    {
        RemoveListObserver(this);
    }

    public void AddListObserver(IObserver observer)
    {
        NetworkTimer.Instance.AddListObserver(observer);
    }

    public void RemoveListObserver(IObserver observer)
    {
        NetworkTimer.Instance.RemoveListObserver(observer);
    }

    public void OnPause(int time)
    {
        SetCanMoveClientRpc(false);
    }

    public void OnResume()
    {
        SetCanMoveClientRpc(true);
    }
    #endregion

    [ClientRpc]
    private void SetCanMoveClientRpc(bool boolen)
    {
        canMove = boolen;
        ChangeStyle(0);
    }

    public void Spawn(Vector3 inputVector, Vector2 velocityVector)
    {
        throw new System.NotImplementedException();
    }

    public void DeSpawn()
    {
        StopAllCoroutines();
        NetworkTimer.Instance.CurrentTick.OnValueChanged -= (oldValue, newValue) => SendMovementToServer();
    }
}
