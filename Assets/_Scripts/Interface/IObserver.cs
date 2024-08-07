using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObserver
{
    void AddListObserver(IObserver observer);
    void RemoveListObserver(IObserver observer);
    void OnPause(int time);
    void OnResume();
}
