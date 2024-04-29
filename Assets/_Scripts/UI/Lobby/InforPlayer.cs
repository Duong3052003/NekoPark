using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class InforPlayer : BaseButton
{
    private TextMeshProUGUI textLobby;
    public int index;

    private void Awake()
    {
        textLobby=GetComponent<TextMeshProUGUI>();
    }

    public void SetInformation(string namPlayer, int color)
    {
        PrintColor(color);
        textLobby.text = namPlayer;
    }

    private void PrintColor(int color)
    {
        switch (color)
        {
            case 0:
                //colorTxt = "Brown";
                textLobby.color = new Color32(165, 42, 42, 255);
                break;
            case 1:
                //colorTxt = "Green";
                textLobby.color = new Color32(0, 128, 0, 255);
                break;
            case 2:
                //colorTxt = "Pink";
                textLobby.color = new Color32(255, 192, 203, 255);
                break;
            case 3:
                //colorTxt = "Blue";
                textLobby.color = new Color32(0, 0, 255, 255);
                break;
        }
    }

    protected override void OnClick()
    {
        if (!IsHost) return;
        Debug.Log("Pressed");
        UIManager.Instance.KickPlayer(index);
    }
}
