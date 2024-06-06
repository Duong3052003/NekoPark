using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

public class UINetWorkManager : NetworkBehaviour
{
    [SerializeField] private Button hostbtn;
    [SerializeField] private Button clientbtn;
    [SerializeField] private TextMeshProUGUI numberPlayerText;

    private NetworkVariable<int> numberPlayers = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone);


    private void Awake()
    {
        hostbtn.onClick.AddListener(() =>
        {
            UIManager.Instance.CreateLobby("test");
            NetworkManager.Singleton.StartHost();
        });
        clientbtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }

    private void Update()
    {
        numberPlayerText.text = "Player :" + numberPlayers.Value.ToString();
        if (!IsServer) return;
        numberPlayers.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }












    //private void OnGUI()
    //{
    //    GUILayout.BeginArea(new Rect(10, 10, 300, 300));
    //    if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
    //    {
    //        StartButtons();
    //    }
    //    else
    //    {
    //        StatusLabels();
    //    }
    //    GUILayout.EndArea();
    //}

    //static void StartButtons()
    //{
    //    if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
    //    if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
    //    if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    //}

    //static void StatusLabels()
    //{
    //    var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
    //    GUILayout.Label("Transport: " +
    //        NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
    //    GUILayout.Label("Mode: " + mode);
    //}
}
