using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InLobby : MonoBehaviour
{
    public GameObject gameObjScene;

    private void OnEnable()
    {
        gameObjScene.GetComponent<SceneInformation>().openWithFullButton = true;
        gameObjScene.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(50,50);
        Instantiate(gameObjScene, this.transform);
    }
}
