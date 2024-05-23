using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceLevelScreen : MonoBehaviour
{
    [SerializeField] private GameObject[] scenes;

    private void OnEnable()
    {
        for(int i = 0; i < scenes.Length; i++)
        {
            scenes[i].GetComponent<SceneInformation>().openWithFullButton = false;
            Instantiate(scenes[i],this.transform);
        }
    }
}
