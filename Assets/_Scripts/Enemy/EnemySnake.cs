using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemySnake : SnakeObjManager,IObjectServerMovement
{
    public static EnemySnake Instance { get; private set; }
    private List<ISnakeObserver> listSnakeObserver = new List<ISnakeObserver>();

    private Rigidbody2D rb;

    [SerializeField] protected float velocityX = 0;
    [SerializeField] protected float velocityY = 0;

    [SerializeField] protected float rotationSpeed = 1f;
    [SerializeField] protected float speed = 30f;

    [SerializeField] private int styleCurrent;
    [SerializeField] private int comboCurrent;
    [SerializeField] private int stepComboCurrent;

    [SerializeField] private bool bodyCanShoot;
    [SerializeField] private bool canMove=false;
    [SerializeField] private bool isFinishStep=true;

    private Coroutine coroutineCurrent;

    [SerializeField] private List<GameObject> listTargetObj;

    [SerializeField] private Vector3 posCurrent;

    public NetworkVariable<Vector3> nTargetPos = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

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

    private void Start()
    {
        posCurrent = this.transform.position;

        comboCurrent = 0;
        styleCurrent = 0;
        isFinishStep = true;

        if(!IsHost) return;
        ChangeComboClientRpc(UnityEngine.Random.Range(0, 2));
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
        rb.velocity = new Vector3(_x, _y) * speed;
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
            default:
                ComboZero();
                break;
        }
    }

    [ClientRpc]
    public void ChangeComboClientRpc(int index)
    {
        stepComboCurrent = 0;

        switch (index)
        {
            case 0:
                ComboZero();
                comboCurrent = index;
                break;
            case 1:
                ComboOne();
                comboCurrent = index;
                break;
            default:
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
                StyleMove(false, 0);
                break;
            default:
                StyleMove(false, 0);
                break;
        }
    }

    /*[ClientRpc]
    public void ChangeStyleClientRpc(int index, int model)
    {
        switch (index)
        {
            case 0:
                StyleMove(true, 0);
                styleCurrent = index;
                break;
            default:
                StyleMove(true, 0);
                styleCurrent = index;
                break;
        }
    }*/
    #endregion

    #region ComboStyle
    #region ComboZero
    private void ComboZero()
    {
        //Setting
        if (isFinishStep)
        {
            isFinishStep =false;

            switch (stepComboCurrent)
            {
                case 0:
                    StyleMove(true, 4);
                    break;
                case 1:
                    StyleMove(true, 5);
                    break;
                case 2:
                    StyleMove(true, 1);
                    break;
                case 3:
                    StyleMove(true, 8);
                    break;
                case 4:
                    StyleMove(true, 2);
                    break;
                case 5:
                    StyleMove(true, 7);
                    break;
                default:
                    if (!IsHost) return;
                    ChangeComboClientRpc(UnityEngine.Random.Range(0,2));
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

            switch (stepComboCurrent)
            {
                case 0:
                    StyleMove(true, 0);
                    break;
                case 1:
                    StyleMove(true, 7);
                    break;
                case 2:
                    StyleMove(true, 2);
                    break;
                case 3:
                    StyleMove(true, 8);
                    break;
                case 4:
                    StyleMove(true, 1);
                    break;
                case 5:
                    StyleMove(true, 6);
                    break;
                case 6:
                    StyleMove(true, 5);
                    break;
                case 7:
                    StyleMove(true, 3);
                    break;
                default:
                    if (!IsHost) return;
                    ChangeComboClientRpc(UnityEngine.Random.Range(0, 2));
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
    private void StyleMove(bool isSetting, int index)
    {
        //Setting
        if (isSetting)
        {
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
            if (distance < 1)
            {
                posCurrent = nTargetPos.Value;
                isFinishStep=true;
            }
        }
    }

    /*public IEnumerator Chase(float _time)
    {
        if (IsHost)
        {
            nTargetPos.Value = TargetPos(5);
        }
        Movement((nTargetPos.Value - posCurrent).normalized);
        yield return new WaitForSeconds(_time);
        if (IsHost)
        {
            nTargetPos.Value = TargetPos(7);
        }
        Movement((nTargetPos.Value - posCurrent).normalized);
        yield return new WaitForSeconds(_time);
        if (IsHost)
        {
            nTargetPos.Value = TargetPos(3);
        }
        Movement((nTargetPos.Value - posCurrent).normalized);
        yield return new WaitForSeconds(_time);
        if (IsHost)
        {
            nTargetPos.Value = TargetPos(4);
        }
        Movement((nTargetPos.Value - posCurrent).normalized);
        yield return new WaitForSeconds(_time);
        if (IsHost)
        {
            nTargetPos.Value = TargetPos(6);
        }
        Movement((nTargetPos.Value - posCurrent).normalized);
        yield return new WaitForSeconds(_time);
        ChangeStyleClientRpc(UnityEngine.Random.Range(0, 5));
    }*/

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
    private void SetTargetAllBody()
    {
        bodyParts[0].GetComponent<EnemySnakeBody>().ChangeTarget(this.gameObject);
        for (int i = 1; i < size; i++)
        {
            bodyParts[i].GetComponent<EnemySnakeBody>().ChangeTarget(bodyParts[i-1]);
        }
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
    public void OnSettingServerRpc()
    {
        listSnakeObserver.ForEach(observer => observer.OnSetting());
    }

    public override void OnPause(int time)
    {

    }

    public override void OnResume()
    {
        canMove = true;
        VirtualCameraSetting.Instance.ChangeFieldOfView(25f);
        GenerateObjsServerRPC();
    }

    public override void OnLoadDone()
    {
    }
    #endregion
}
