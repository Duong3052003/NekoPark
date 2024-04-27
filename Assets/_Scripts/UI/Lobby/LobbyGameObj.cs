using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyGameObj : MonoBehaviour
{
    private TextMeshPro textLobby;

    private void Awake()
    {
        textLobby = GetComponent<TextMeshPro>();
    }

    public void SetInformation(string nameLobby, int currentPlayers, int maxPlayers)
    {
        textLobby.text = nameLobby + " - MaxPlayers: " + currentPlayers.ToString() + "/" + maxPlayers.ToString();
    }
}
