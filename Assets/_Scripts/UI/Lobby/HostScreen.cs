using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostScreen : MonoBehaviour
{
    private bool isPrivate=false;
    [SerializeField] private TextMeshProUGUI lobbyMode;

    private int index = 0;
    [SerializeField] private TextMeshProUGUI gameMode;

    public void LobbyMode()
    {
        isPrivate = !isPrivate;
        if (isPrivate)
        {
            lobbyMode.text = "Private";
        }else
        {
            lobbyMode.text = "Public";
        }
    }

    public void GameMode()
    {
        index++;
        if (index >= 2)
        {
            index = 0;
        }
        switch (index)
        {
            case 0:
                gameMode.text = "Story";
                break;
            case 1:
                gameMode.text = "HighScore";
                break;
        }
    }

    public void Return()
    {
        gameObject.SetActive(false);
    }
}
