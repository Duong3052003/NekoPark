using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITakeDamaged
{
    void TakeDamagedServerRpc(int damage);
}
