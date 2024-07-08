using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class NetworkTimer : NetworkBehaviour
{
    public static NetworkTimer Instance { get; private set; }

    float timer;
    public float MinTimeBetweenTicks { get; private set; }
    public NetworkVariable<int> CurrentTick = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        MinTimeBetweenTicks = 1f / 60f;
    }

    /*public NetworkTimer(float serverTickRate)
    {
        MinTimeBetweenTicks = 1f / serverTickRate;
    }*/

    public void Update()
    {
        if (this.IsSpawned && IsHost)
        {
            timer += Time.deltaTime;
            ShouldTick();
        }
    }

    public bool ShouldTick()
    {
        if (timer >= MinTimeBetweenTicks)
        {
            timer -= MinTimeBetweenTicks;
            CurrentTick.Value ++;
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