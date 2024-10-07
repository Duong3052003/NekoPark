using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpawnByGroup
{
    void AddListObserver(ISceneObserver observer);
    void RemoveListObserver(ISceneObserver observer);
    void CanSpawn(bool boolen);
}
