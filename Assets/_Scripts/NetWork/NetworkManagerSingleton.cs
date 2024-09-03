using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerSingleton : MonoBehaviour
{
    public static NetworkManagerSingleton Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);

        }
    }
}
