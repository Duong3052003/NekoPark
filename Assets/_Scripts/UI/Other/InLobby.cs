using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InLobby : MonoBehaviour
{
    public GameObject gameObjScene;

    private void OnEnable()
    {
        gameObjScene.GetComponent<SceneInformation>().openWithFullButton = true;
        Instantiate(gameObjScene, this.transform);
    }
}
