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
        transform.GetComponent<SpriteRenderer>().sprite = skins[(int)OwnerClientId];
        transform.GetComponent<Animator>().runtimeAnimatorController = controllers[(int)OwnerClientId];

        if (!IsOwner) return;
        playerHUDImg.SetActive(true);
    }
}
