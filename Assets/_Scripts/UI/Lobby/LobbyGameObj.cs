using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyGameObj : BaseButton
{
    [SerializeField] private TextMeshProUGUI textLobby;
    public int index;

    public void SetInformation(string nameLobby, int currentPlayers, int maxPlayers, string gameMode)
    {
        textLobby.text = nameLobby + " - " + currentPlayers.ToString() + "/" + maxPlayers.ToString()+ " - " +gameMode;
    }

    protected override void OnClick()
    {
        Debug.Log("Pressed");
        UIManager.Instance.JoinLobby(index);
    }
}
