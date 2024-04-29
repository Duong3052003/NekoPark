using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSetting : NetworkBehaviour
{
    [SerializeField] private Sprite[] skins;
    [SerializeField] private RuntimeAnimatorController[] controllers;
    [SerializeField] private GameObject playerHUDImg;

    public override void OnNetworkSpawn()
    {
        GetModel();
        if (!IsOwner) return;
        playerHUDImg.SetActive(true);
    }

    private void GetModel()
    {
        int modelValue = PlayerPrefs.GetInt("Model", 1);
        int colorValue = PlayerPrefs.GetInt("Color", 0);

        int skin = modelValue + colorValue;

        transform.GetComponent<SpriteRenderer>().sprite = skins[skin];
        transform.GetComponent<Animator>().runtimeAnimatorController = controllers[skin];
    }
}
