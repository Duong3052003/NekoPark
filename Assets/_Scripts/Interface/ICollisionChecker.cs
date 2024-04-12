using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollisionGroundChecker
{
    bool IsGrounded();
}

public interface ICollisionChecker
{
    bool IsTouching();
}
