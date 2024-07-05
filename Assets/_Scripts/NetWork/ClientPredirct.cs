using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPredirct : MonoBehaviour
{
    private float timer;
    private int currentTick;
    private float minTimerBetweenTicks;

    private const float SERVER_TICK_RATE = 30f;

    private void Start()
    {
        minTimerBetweenTicks = 1f / SERVER_TICK_RATE;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        while (timer >= minTimerBetweenTicks)
        {
            timer -= Time.deltaTime;
            HandleTime();
            currentTick++;
        }
    }

    void HandleTime()
    {

    }
}
