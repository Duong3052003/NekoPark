using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawnByGroup
{
    void AddListObserver(IObserver observer);
    void RemoveListObserver(IObserver observer);
    void CanSpawn(bool boolen);
}
