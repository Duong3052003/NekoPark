using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class LobbyScreen : MonoBehaviour
{
    private int indexColor = 0;
    private void OnEnable()
    {
        UIManager.Instance.ListLobby();
    }

    public void GetColor()
    {
        indexColor += 1;

        if (indexColor > 3)
        {
            indexColor = 0;
        }

        UIManager.Instance.UpdateColorPlayer(GetStringColor(indexColor));
        UIManager.Instance.UpdatePlayerLobby();
        Debug.Log(indexColor);
    }

    private string GetStringColor(int color)
    {
        switch (color)
        {
            case 0:
                return "Brown";
            case 1:
                return "Green";
            case 2:
                return "Pink";
            case 3:
                return "Blue";
            default:
                return "Brown";
        }
    }
}
