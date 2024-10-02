using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyBehaviour : NetworkBehaviour, IObjectServerMovement, IObserver, IObjectServerSpawn
{
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }

    public float speed = 16f;
    public float rotationSpeed = 10f;
    private bool canMove=true;

    [SerializeField] private float velocityX = 0;
    [SerializeField] private float velocityY = -1f;

    [SerializeField] private GameObject gunPreb;

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
    }

    public override void OnNetworkSpawn()
    {
        ReconcileTransform();
        NetworkTimer.Instance.CurrentTick.OnValueChanged += (oldValue, newValue) => SendMovementToServer();
    }

    void Update()
    {
        if (!canMove) return;
        Move(velocityX, velocityY); ;
        RotateShip(GetMovement());
    }

    private void Move(float _x, float _y)
    {
        rb.velocity = new Vector2(_x, _y);
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

    void RotateShip(Vector3 targetVector)
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

    [ClientRpc]
    private void SetCanMoveClientRpc(bool boolen)
    {
        canMove = boolen;
    }

    public void Spawn(Vector3 inputVector, Vector2 velocityVector)
    {
        throw new System.NotImplementedException();
    }

    public void DeSpawn()
    {
        NetworkTimer.Instance.CurrentTick.OnValueChanged -= (oldValue, newValue) => SendMovementToServer();
    }
}
