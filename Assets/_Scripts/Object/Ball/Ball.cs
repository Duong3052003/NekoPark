using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Ball : NetworkBehaviour,IObjectMovement
{
    public int damage = 1;
    public float maxVelocity = 15f;
    private Rigidbody2D rb;
    private float forceMagnitude = 15f;

    // Netcode general
    const float k_serverTickRate = 60f; // 60 FPS
    const int k_bufferSize = 1024;
    public NetworkVariable<Vector3> nPosition = new NetworkVariable<Vector3>(new Vector3(0, 0, 0), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] float reconciliationThreshold = 50f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        UpdateNetworkVariables();
    }

    private void FixedUpdate()
    {
        VelocityIsMax();
    }

    private void VelocityIsMax()
    {
        if(rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
        }
    }

    private void SendMovementToServer(Vector3 _inputVector)
    {
        if (!IsOwner) return;
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
        ChangedGravity();
        UpdateNetworkVariables();

        ITakeDamaged objTakeDamaged = collision.gameObject.GetComponent<ITakeDamaged>();
        if (objTakeDamaged != null)
        {
            objTakeDamaged.TakeDamagedServerRpc(damage);
            Debug.Log(objTakeDamaged + "take " + damage + " damage");
        }

        if (collision.gameObject.CompareTag("Breakout"))
        {
            Debug.Log("Va cham dung");

            Vector3 forceDirection = collision.contacts[0].normal.normalized;
            forceDirection.z = 0;
            Debug.Log(forceDirection);

            SendMovementToServer(forceDirection);
        }
    }

    /*[ClientRpc(RequireOwnership = false)]
    void HandleCollisionServerRpc(Vector3 forceDirection, float forceMagnitude)
    {
        rb.AddForce(forceDirection * forceMagnitude, ForceMode2D.Impulse);
        Debug.Log(3);


        if (IsServer)
        {
            ApplyForceClientRpc(forceDirection, forceMagnitude);
        }
    }*/
/*
    [ClientRpc]
    void ApplyForceClientRpc(Vector3 forceDirection)
    {
        if (rb != null)
        {
            

        }
    }*/

    private void ChangedGravity()
    {
        if (rb.gravityScale != 0f) return;
        rb.gravityScale = 0.05f;
    }

    public void Movement(Vector3 direction)
    {
        rb.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);
    }

    private void UpdateNetworkVariables()
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
}
