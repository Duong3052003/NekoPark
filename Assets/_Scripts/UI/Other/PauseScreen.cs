using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PauseScreen : NetworkBehaviour,IObserver
{
    [SerializeField] TextMeshProUGUI timeCountText;
    [SerializeField] GameObject backGround;

    public NetworkVariable<int> nTime = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float floatTime;

    private void Update()
    {
        timeCountText.text = nTime.Value.ToString();

        if (IsServer && nTime.Value >0 && nTime !=null)
        {
            floatTime -= Time.deltaTime;

            if (floatTime <= 0)
            {
                floatTime = 1;
                nTime.Value--;
            }
        }
    }

    private void Start()
    {
        AddListObserver(this);
    }

    private void OnDisable()
    {
        RemoveListObserver(this);
    }

    public void AddListObserver(IObserver observer)
    {
        NetworkTimer.Instance.AddListObserver(observer);
    }

    public void RemoveListObserver(IObserver observer)
    {
        NetworkTimer.Instance.AddListObserver(observer);
    }

    public void OnPause(int time)
    {
        if (!IsServer) return;
        nTime.Value = time;
        SetActiveClientRPC(true);
    }

    public void OnResume()
    {
        if (!IsServer) return;
        SetActiveClientRPC(false);
    }

    [ClientRpc]
    private void SetActiveClientRPC(bool boolen)
    {
        backGround.SetActive(boolen);
        timeCountText.gameObject.SetActive(boolen);
    }
}
