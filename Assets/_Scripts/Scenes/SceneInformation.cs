using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class SceneInformation : MonoBehaviour
{
    [SerializeField] private SceneScriptableObject dataScene;

    [SerializeField] private Button buttonScene;
    [SerializeField] private Button settingSceneBtn;
    [SerializeField] private Button createLobbyBtn;

    public bool openWithFullButton = false;

    private void Start()
    {
        InLobby inLobby = UIManager.Instance.inLobby.GetComponent<InLobby>();

        transform.GetComponent<Image>().sprite = dataScene.imageScene;
        Debug.Log(dataScene.imageScene);

        buttonScene.onClick.AddListener(() =>
        {
            inLobby.gameObjScene = Instantiate(this.gameObject);
            UIManager.Instance.ChoiceLevelScreen();
            this.gameObject.SetActive(false);
        });

        if (!openWithFullButton) return;

        settingSceneBtn.gameObject.SetActive(true);
        createLobbyBtn.gameObject.SetActive(true);

        settingSceneBtn.onClick.AddListener(() =>
        {

        });

        createLobbyBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.CreateLobby(dataScene.nameScene);
        });
    }

    private void OnDisable()
    {
        Destroy(this.gameObject);
    }
}
