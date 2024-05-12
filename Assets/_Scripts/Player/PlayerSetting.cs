using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
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
        foreach (Player player in UIManager.Instance.joinedLobby.Players)
        {
            int color = UIManager.Instance.GetIndexColor(player.Data["PlayerColor"].Value);
            int model = 0;
            int skin = color + model;
            transform.GetComponent<SpriteRenderer>().sprite = skins[skin];
            transform.GetComponent<Animator>().runtimeAnimatorController = controllers[skin];
        }

   /*     int modelValue = PlayerPrefs.GetInt("Model", 0);
        int colorValue = PlayerPrefs.GetInt("Color", 0);
        int skin = modelValue + colorValue;

        transform.GetComponent<SpriteRenderer>().sprite = skins[skin];
        transform.GetComponent<Animator>().runtimeAnimatorController = controllers[skin];*/
    }
}
