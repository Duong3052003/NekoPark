using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelBricksGenerator : LevelGenerator
{
    public static LevelBricksGenerator Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public List<GameObject> CallAllBallsOfClient(ulong clientId)
    {
        GameObject[] allBalls = GameObject.FindGameObjectsWithTag("Bullet");

        List<GameObject> clientBalls = new List<GameObject>();

        foreach (GameObject ball in allBalls)
        {
            NetworkObject networkObject = ball.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.OwnerClientId == clientId)
            {
                clientBalls.Add(ball);
            }
        }

        return clientBalls;
    }
}
