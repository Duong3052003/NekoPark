using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EnemySnake : SnakeObjManager,IObjectServerMovement
{
    public static EnemySnake Instance { get; private set; }
    private List<ISnakeObserver> listSnakeObserver = new List<ISnakeObserver>();

    private Rigidbody2D rb;
    [SerializeField] private bool isStarted = false;

    [SerializeField] private GameObject[] machineGuns;

    [SerializeField] protected float velocityX = 0;
    [SerializeField] protected float velocityY = 0;

    [SerializeField] protected float rotationSpeed = 1f;
    [SerializeField] protected float speed = 30f;

    [SerializeField] private int styleCurrent;
    [SerializeField] private int comboCurrent;
    [SerializeField] private int stepComboCurrent;
    [SerializeField] private bool isRotate=false;

    [SerializeField] private bool bodyCanShoot;
    [SerializeField] private bool canMove=false;
    [SerializeField] private bool isFinishStep=true;

    [SerializeField] private int PHASE=1;
    [SerializeField] private int maxCombo=2;
    [SerializeField] private int minCombo=0;
    [SerializeField] private float timer;

    private Coroutine coroutineCurrent;

    [SerializeField] private List<GameObject> listTargetObj;

    [SerializeField] private Vector3 posCurrent;

    public NetworkVariable<Vector3> nTargetPos = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // Netcode general
    protected const int k_bufferSize = 1024;

    public NetworkVariable<Vector3> nPosition = new NetworkVariable<Vector3>(new Vector3(0, 0, 0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Quaternion> nRotation = new NetworkVariable<Quaternion>(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Hp Setting")]
    [SerializeField] private GameObject hpBar;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private float hpMax;
    public NetworkVariable<float> nHPValue = new NetworkVariable<float>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public int indexBody = 0;

    protected float reconciliationThreshold = 0.5f;

    [SerializeField] private float distanceForChange = 2;

    public override void OnNetworkSpawn()
    {
        ReconcileTransform();
        NetworkTimer.Instance.CurrentTick.OnValueChanged += (oldValue, newValue) => SendMovementToServer();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        rb = this.gameObject.GetComponent<Rigidbody2D>();
    }

    [ClientRpc]
    private void SetupClientRpc(float _hpMax)
    {
        VirtualCameraSetting.Instance.ChangeFieldOfView(18f);
        canMove = true;
        posCurrent = this.transform.position;
        hpMax = _hpMax;
        comboCurrent = 0;
        ChangeComboClientRpc(comboCurrent);
        styleCurrent = 0;
        isFinishStep = true;
    }

    private void Update()
    {
        CheckConditionsCombo();
        if (!canMove) return;
        Move(velocityX, velocityY);
        Rotate(GetMovement());
    }

    #region Movement
    public void Movement(Vector3 direction)
    {
        velocityX = direction.x;
        velocityY = direction.y;
    }

    public Vector3 GetMovement()
    {
        return new Vector3(velocityX, velocityY, 0);
    }

    private void Move(float _x, float _y)
    {
        if (isRotate == false)
        {
            rb.velocity = new Vector3(_x, _y) * speed;
        }
        else
        {
            this.transform.RotateAround(new Vector3(_x, _y), Vector3.forward, speed*4.5f * Time.deltaTime);
            rb.velocity = Vector3.forward;

        }
    }

    private void Rotate(Vector3 targetVector)
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

    [ClientRpc]
    private void ChangeSpeedClientRpc(float newSpeed)
    {
        speed = newSpeed;
    }
    #endregion

    #region Combo
    private void CheckConditionsCombo()
    {
        switch (comboCurrent)
        {
            case 0:
                ComboZero();
                break;
            case 1:
                ComboOne();
                break;
            case 2:
                ComboTwo();
                break;
            case 3:
                ComboThree();
                break;
            default:
                ComboZero();
                break;
        }
    }

    [ClientRpc]
    public void ChangeComboClientRpc(int index)
    {
        switch (index)
        {
            case 0:
                stepComboCurrent = 0;
                ComboZero();
                comboCurrent = index;
                break;
            case 1:
                stepComboCurrent = 0;
                ComboOne();
                comboCurrent = index;
                break;
            case 2:
                stepComboCurrent = 0;
                ComboTwo();
                comboCurrent = index;
                break;
            case 3:
                stepComboCurrent = 0;
                ComboThree();
                comboCurrent = index;
                break;
            default:
                stepComboCurrent = 0;
                ComboZero();
                comboCurrent = index;
                break;
        }
    }
    #endregion

    #region Style
    private void CheckConditions()
    {
        switch (styleCurrent)
        {
            case 0:
                StyleMoveZero(false, 0);
                break;
            case 1:
                StyleMoveOne(false, 0);
                break;
            case 2:
                StyleMoveTwo(false);
                break;
            default:
                StyleMoveZero(false, 0);
                break;
        }
    }

    #endregion

    #region ComboStyle
    #region ComboZero
    private void ComboZero()
    {
        //Setting
        if (isFinishStep)
        {
            isFinishStep =false;
            distanceForChange = 3;
            switch (stepComboCurrent)
            {
                case 0:
                    StyleMoveZero(true, 7);
                    break;
                case 1:
                    StyleMoveZero(true, 2);
                    break;
                case 2:
                    StyleMoveZero(true, 8);
                    break;
                case 3:
                    StyleMoveZero(true, 1);
                    break;
                case 4:
                    StyleMoveZero(true, 5);
                    break;
                case 5:
                    StyleMoveZero(true, 4);
                    break;
                default:
                    if (!IsHost) return;
                    ChangeComboClientRpc(UnityEngine.Random.Range(minCombo, maxCombo));
                    break;
            }
            stepComboCurrent++;
        }

        //Conditions
        else
        {
            CheckConditions();
        }
    }

    #region ComboOne
    private void ComboOne()
    {
        //Setting
        if (isFinishStep)
        {
            isFinishStep = false;
            distanceForChange = 2;
            switch (stepComboCurrent)
            {
                case 0:
                    StyleMoveZero(true, 3);
                    break;
                case 1:
                    StyleMoveZero(true, 5);
                    break;
                case 2:
                    StyleMoveZero(true, 6);
                    break;
                case 3:
                    StyleMoveZero(true, 1);
                    break;
                case 4:
                    StyleMoveZero(true, 8);
                    break;
                case 5:
                    StyleMoveZero(true, 2);
                    break;
                case 6:
                    StyleMoveZero(true, 7);
                    break;
                case 7:
                    StyleMoveZero(true, 0);
                    break;
                default:
                    if (!IsHost) return;
                    ChangeComboClientRpc(UnityEngine.Random.Range(minCombo, maxCombo));
                    break;
            }
            stepComboCurrent++;
        }

        //Conditions
        else
        {
            CheckConditions();
        }
    }

    #endregion

    #region ComboTwo
    private void ComboTwo()
    {
        //Setting
        if (isFinishStep)
        {
            isFinishStep = false;
            distanceForChange = 0.5f;
            switch (stepComboCurrent)
            {
                case 0:
                    StyleMoveZero(true, 9);
                    break;
                case 1:
                    StyleMoveOne(true, 4);
                    break;
                default:
                    if (!IsHost) return;
                    ChangeComboClientRpc(UnityEngine.Random.Range(minCombo, maxCombo));
                    break;
            }
            stepComboCurrent++;
        }

        //Conditions
        else
        {
            CheckConditions();
        }
    }
    #endregion

    #region ComboThree
    private void ComboThree()
    {
        //Setting
        if (isFinishStep)
        {
            isFinishStep = false;
            distanceForChange = 2;
            switch (stepComboCurrent)
            {
                case 0:
                    StyleMoveZero(true, 4);
                    break;
                case 1:
                    StyleMoveTwo(true);
                    break;
                default:
                    if (!IsHost) return;
                    UnityEngine.Random.Range(minCombo, maxCombo);
                    break;
            }
            stepComboCurrent++;
        }

        //Conditions
        else
        {
            CheckConditions();
        }
    }
    #endregion

    #endregion
    #endregion

    #region StyleMove
    private void StyleMoveZero(bool isSetting, int index)
    {
        //Setting
        if (isSetting)
        {
            styleCurrent = 0;
            isRotate = false;
            timer = 5f;

            if (IsHost)
            {
                nTargetPos.Value = TargetPos(index);
            }

            Movement((nTargetPos.Value - posCurrent).normalized);
        }
        //Conditions
        else
        {
            float distance = (nTargetPos.Value - this.transform.position).magnitude;
            timer = timer - Time.deltaTime;

            if (distance < distanceForChange || timer <= 0)
            {
                posCurrent = this.transform.position;
                isFinishStep=true;
            }

        }
    }

    private void StyleMoveOne(bool isSetting, int index)
    {
        //Setting
        if (isSetting)
        {
            rb.velocity = Vector3.zero;
            styleCurrent = 1;

            isRotate = true;
            timer = 10f;
            if (IsHost)
            {
                nTargetPos.Value = TargetPos(index);
            }
            Movement((nTargetPos.Value));
        }
        //Conditions
        else
        {
            timer = timer - Time.deltaTime;
            if (timer <= 0)
            {
                posCurrent = this.transform.position;
                isFinishStep = true;
            }
        }
    }

    private void StyleMoveTwo(bool isSetting)
    {
        //Setting
        if (isSetting)
        {
            rb.velocity = Vector3.zero;
            rb.rotation = 0;
            timer = 5f;

            styleCurrent = 2;
            isRotate = false;
            canMove = false;

            for (int j = 0; j < machineGuns.Length; j++)
            {
                machineGuns[j].GetComponent<BulletSpawner>().isSpin = true;
                if (IsHost)
                {
                    machineGuns[j].GetComponent<BulletSpawner>().StartCoroutineSpawn();
                }
            }
        }
        //Conditions
        else
        {
            timer = timer - Time.deltaTime;
            if (timer <= 0)
            {
                canMove = true;
                posCurrent = this.transform.position;
                isFinishStep = true;
            }
        }
    }
    #endregion

    #region HPSetting
    [ServerRpc]
    public void ChangeHpServerRpc(float hpValue)
    {
        if (!IsHost) return;
        if(PHASE == 1 && (nHPValue.Value/hpMax <= 0.9f))
        {
            PHASE = 2;
            minCombo = 0;
            maxCombo = 2;

            ChangeSpeedClientRpc(30f);
            OnSettingServerRpc(30f, 31f, false);
        }
        else if(PHASE == 2 && (nHPValue.Value / hpMax <= 0.75f))
        {
            PHASE = 3;
            minCombo = 0;
            maxCombo = 2;

            ChangeSpeedClientRpc(50f);
            OnSettingServerRpc(2.5f, 51f, false);
        }
        else if(PHASE == 3 && (nHPValue.Value / hpMax <= 0.6f))
        {
            PHASE = 4;
            minCombo = 0;
            maxCombo = 3;

            ChangeSpeedClientRpc(20f);
            OnSettingServerRpc(3f, 21f, true);
            ChangeComboClientRpc(2);
        }
        else if (PHASE == 4 && (nHPValue.Value / hpMax <= 0.40f))
        {
            PHASE = 5;
            minCombo = 1;
            maxCombo = 3;

            ChangeSpeedClientRpc(30);
            OnSettingServerRpc(50f, 32f, true);
        }else if (PHASE == 5 && (nHPValue.Value / hpMax <= 0.25f))
        {
            this.gameObject.layer = 7;

            PHASE = 6;
            minCombo = 1;
            maxCombo = 3;

            ChangeSpeedClientRpc(40f);
            OnSettingServerRpc(2.5f, 41f, true);
            ChangeComboClientRpc(3);
        }

        nHPValue.Value = nHPValue.Value + hpValue;

        UpdateHpBarClientRpc(nHPValue.Value);
    }

    [ClientRpc]
    private void UpdateHpBarClientRpc(float hpValue)
    {
        hpBar.SetActive(true);
        if (hpSlider == null)
        {
            hpSlider = hpBar.GetComponent<Slider>();
        }
        hpSlider.value = hpValue/ hpMax;
    }
    #endregion

    #region Target
    public GameObject Target(List<GameObject> validTargets)
    {
        if (validTargets.Count == 0)
        {
            Debug.LogWarning("Khong co muc tieu.");
            return null;
        }
        return RandomGameObjectFromList.GetRandomGameObject(validTargets);
    }

    public Vector3 TargetPos(int index)
    {
        return listTargetObj[index].transform.position;
    }
    #endregion

    #region BodyFollow
    [ServerRpc(RequireOwnership = false)]
    public override void GenerateObjsServerRPC()
    {
        if (isStarted == true) return;
        isStarted = true;
        hpMax = 0;

        for (int i = 0; i < size; i++)
        {
            objSpawned = ObjIsSpawned();

            if (i <= 5)
            {
                hpMax = hpMax + hpPart * 4;
                objSpawned.GetComponent<ObjDeSpawnByHp>().SetHp(hpPart * 5);
            }
            else if(i <= 15)
            {
                hpMax = hpMax + hpPart * 3;
                objSpawned.GetComponent<ObjDeSpawnByHp>().SetHp(hpPart * 4);
            }
            else if(i <= 30)
            {
                hpMax = hpMax + hpPart * 2;
                objSpawned.GetComponent<ObjDeSpawnByHp>().SetHp(hpPart * 3);
            }
            else if (i <= 50)
            {
                hpMax = hpMax + hpPart * 2;
                objSpawned.GetComponent<ObjDeSpawnByHp>().SetHp(hpPart * 2);
            }
            else
            {
                hpMax = hpMax + hpPart;
                objSpawned.GetComponent<ObjDeSpawnByHp>().SetHp(hpPart);
            }
        }

        SetupClientRpc(hpMax);

        ChangeHpServerRpc(hpMax);
    }

    public void AddtoListBody(GameObject bodyPart)
    {
        bodyParts.Add(bodyPart);
    }

    public void GetPosition()
    {
        if (indexBody == 0)
        {
            bodyParts[indexBody].GetComponent<EnemySnakeBody>().target = this.gameObject;
            bodyParts[indexBody].transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + distanceBetween);
        }
        else
        {
            bodyParts[indexBody].GetComponent<EnemySnakeBody>().target = bodyParts[indexBody-1];
            bodyParts[indexBody].transform.position = new Vector3(bodyParts[indexBody - 1].transform.position.x, bodyParts[indexBody - 1].transform.position.y + distanceBetween);
        }

        if (indexBody % 5==0)
        {
            bodyParts[indexBody].GetComponent<EnemySnakeBody>().GetGun();
        }
        bodyParts[indexBody].GetComponent<EnemySnakeBody>().distanceBetween = distanceBetween;

        indexBody++;

    }
    #endregion

    #region IObserver

    public void AddListSnakeObserver(ISnakeObserver observer)
    {
        listSnakeObserver.Add(observer);
    }

    public void RemoveListSnakeObserver(ISnakeObserver observer)
    {
        listSnakeObserver.Remove(observer);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnSettingServerRpc(float _distanceBetween, float _speed, bool targetFollow)
    {
        listSnakeObserver.ForEach(observer => observer.OnSetting(_distanceBetween, _speed, targetFollow));
    }

    public override void OnPause(int time)
    {

    }

    public override void OnResume()
    {

    }

    public override void OnLoadDone()
    {
    }

    #endregion

    #region SendToServer
    private void ReconcileTransform()
    {
        if (IsOwner)
        {
            nPosition.Value = transform.position;
            nRotation.Value = transform.rotation;
        }
        else
        {
            //float positionError = Vector3.Distance(nPosition.Value, transform.position);
            transform.rotation = nRotation.Value;
            //if (positionError < reconciliationThreshold) return;
            transform.position = nPosition.Value;
        }
    }

    private void SendMovementToServer()
    {
        if (this == null) return;
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
    #endregion
}
