using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerTop : NetworkBehaviour, IObjectServerSpawn
{
    public void Spawn(Vector3 inputVector, Vector2 velocityVector = default)
    {
        transform.position = inputVector;
    }
}
