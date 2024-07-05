using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerMovement
{
    void Move(Vector2 Input);
    void Jump(Vector2 Input);
}
