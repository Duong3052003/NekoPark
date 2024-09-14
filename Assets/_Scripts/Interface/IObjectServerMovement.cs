using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IObjectServerMovement
{
    void Movement(Vector3 inputVector);
}

public interface IObjectServerSpawn
{
    void Spawn(Vector3 inputVector, Vector2 velocityVector);
}
