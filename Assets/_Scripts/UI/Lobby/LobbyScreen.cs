using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyScreen : MonoBehaviour
{
    private void OnEnable()
    {
        UIManager.Instance.ListLobby();
    }

    public void GetColor()
    {
        int indexColor = PlayerPrefs.GetInt("Color", 0);

        indexColor += 1;

        if (indexColor > 3)
        {
            indexColor = 0;
        }

        UIManager.Instance.UpdatePlayerLobby();

        MyPlayerPrefs.Instance.SaveColor(indexColor);
    }

}
