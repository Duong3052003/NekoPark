using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScreen : MonoBehaviour
{
    private void OnEnable()
    {
        UIManager.Instance.ListLobby();
    }
}
