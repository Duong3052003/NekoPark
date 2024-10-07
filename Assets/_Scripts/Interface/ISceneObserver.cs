using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISceneObserver
{
    void AddListObserver(ISceneObserver observer);
    void RemoveListObserver(ISceneObserver observer);
    void OnPause(int time);
    void OnResume();
    void OnLoadDone();
}

public interface ISnakeObserver
{
    void AddListSnakeObserver(ISnakeObserver observer);
    void RemoveListSnakeObserver(ISnakeObserver observer);
    void OnSetting();
}
