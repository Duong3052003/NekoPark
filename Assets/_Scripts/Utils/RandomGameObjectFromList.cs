using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomGameObjectFromList
{
    public static GameObject GetRandomGameObject(List<GameObject> listGame)
    {
        int randomIndex = Random.Range(0, listGame.Count);
        return listGame[randomIndex];
    }
}

