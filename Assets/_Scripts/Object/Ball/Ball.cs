using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour,IObjectMovement,IObserver
{
    private int damage = 1;
    private Rigidbody2D rb;
    private float forceMagnitude = 10f;
    private float velocityX=0;
    private float velocityY=-0.5f;

    private bool canMove=true;

    // Netcode general
    const float k_serverTickRate = 60f; // 60 FPS
    const int k_bufferSize = 1024;
    public NetworkVariable<Vector3> nPosition = new NetworkVariable<Vector3>(new Vector3(0, 0, 0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private float reconciliationThreshold = 2.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        ReconcileTransform();
    }

    private void Update()
    {
        if (!canMove) return;
        Move(velocityX, velocityY);
    }

    private void FixedUpdate()
    {
        ReconcileTransform();
    }

    private void Move(float _x,float _y)
    {
        rb.velocity = new Vector2(_x,_y);
    }

    private void SendMovementToServer(Vector3 _inputVector)
    {
        var currentTick = NetworkTimer.Instance.CurrentTick.Value;
        var bufferIndex = currentTick % k_bufferSize;

        InputPayLoad inputPayload = new InputPayLoad()
        {
            tick = currentTick,
            timestamp = System.DateTime.Now,
            networkObjID = NetworkObjectId,
            inputVector = _inputVector,
            position = transform.position
        };

        Server.Instance.OnClientInput(inputPayload);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsOwner) return;
        AddForceToServer(collision);

        ITakeDamaged objTakeDamaged = collision.gameObject.GetComponent<ITakeDamaged>();
        if (objTakeDamaged != null)
        {
            objTakeDamaged.TakeDamagedServerRpc(damage);
        }
    }

    private void AddForceToServer(Collision2D collision)
    {
        Vector2 normal = collision.contacts[0].normal;

        Vector2 reflectDirection = Vector2.Reflect(new Vector2(velocityX,velocityY), normal);

        SendMovementToServer(reflectDirection);
    }

    private void ChangedGravity()
    {
        if (rb.gravityScale != 0f) return;
        rb.gravityScale = 0.05f;
    }

    public void Movement(Vector3 direction)
    {
        Vector3 newVelocity = direction.normalized * forceMagnitude;
        velocityX = newVelocity.x;
        velocityY = newVelocity.y;
    }

    private void ReconcileTransform()
    {
        if (IsOwner)
        {
            nPosition.Value = transform.position;
        }
        else
        {
            float positionError = Vector3.Distance(nPosition.Value, transform.position);
            if (positionError < reconciliationThreshold) return;
            transform.position = nPosition.Value;
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
}
