using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class NetworkTimer : NetworkBehaviour
{
    public static NetworkTimer Instance { get; private set; }

    float timer;

    public float MinTimeBetweenTicks { get; private set; }
    public NetworkVariable<int> CurrentTick = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private List<IObserver> listObserver = new List<IObserver>();

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

    public void AddListObserver(IObserver observer)
    {
        listObserver.Add(observer);
    }

    public void RemoveListObserver(IObserver observer)
    {
        listObserver.Remove(observer);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnPauseServerRpc(int time)
    {
        listObserver.ForEach(observer => observer.OnPause(time));

        Invoke(nameof(OnResumeServerRpc), time);
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnResumeServerRpc()
    {
        CancelInvoke();
        for (int i = 0; i < listObserver.Count; i++)
        {
            listObserver[i].OnResume();
        }
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