using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTimer
{
    float timer;
    public float MinTimeBetweenTicks { get; }
    public int CurrentTick { get; private set; }

    public NetworkTimer(float serverTickRate)
    {
        MinTimeBetweenTicks = 1f / serverTickRate;
    }

    public void Update(float deltaTime)
    {
        timer += deltaTime;
    }

    public bool ShouldTick()
    {
        if (timer >= MinTimeBetweenTicks)
        {
            //timer -= MinTimeBetweenTicks;
            CurrentTick++;
            return true;
        }

        return false;
    }
}

public class CircularBuffer<T>
{
    T[] buffer;
    int bufferSize;

    public CircularBuffer(int bufferSize)
    {
        this.bufferSize = bufferSize;
        buffer = new T[bufferSize];
    }

    public void Add(T item, int index) => buffer[index % bufferSize] = item;
    public T Get(int index) => buffer[index % bufferSize];
    public void Clear() => buffer = new T[bufferSize];

    public static implicit operator CircularBuffer<T>(CircularBuffer<InputPayLoad> v)
    {
        throw new NotImplementedException();
    }
}