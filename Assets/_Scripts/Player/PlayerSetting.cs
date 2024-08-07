using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;


public class PlayerSetting : NetworkBehaviour
{
    private Sprite[] skins;
    private RuntimeAnimatorController[] controllers;
    [SerializeField] private GameObject playerHUDImg;

    [SerializeField] private Button startBtn;
    [SerializeField] private Button changedColorBtn;

    private int color;

    private NetworkVariable<int> nCharacter = new NetworkVariable<int>(0);

    public override void OnNetworkSpawn()
    {
        PlayerManager.Instance.players.Add(this.gameObject);
        SetUpPlayer();
        nCharacter.OnValueChanged += (oldValue, newValue) => UpdateModel(newValue);
    }

    public void SetUpPlayer()
    {
        skins = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().skins;
        controllers = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().controllers;

        if (IsOwner)
        {
            GetDataPlayer();
            GetButton();

            UpdateModelServerRpc(color);
            playerHUDImg.SetActive(true);

        }
    }

    private void GetDataPlayer()
    {
        if (UIManager.Instance.joinedLobby == null)
        {
            Debug.Log("Not found Lobby");
        }
        else
        {
            foreach (Player player in UIManager.Instance.joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    color = UIManager.Instance.GetIndexColor(player.Data["PlayerColor"].Value);
                }
            }
        }
    }

    private void GetButton()
    {
        if (IsHost)
        {
            startBtn.gameObject.SetActive(true);

            startBtn.onClick.AddListener(() =>
            {
                UIManager.Instance.StartLevel();
            });
        }
        else
        {
            startBtn.gameObject.SetActive(false);
        }

        changedColorBtn.onClick.AddListener(() =>
        {
            ChangedSkinBtn();
        });
    }

    private void UpdateModel(int _color)
    {
        transform.GetComponent<SpriteRenderer>().sprite = skins[_color];
        transform.GetComponent<Animator>().runtimeAnimatorController = controllers[_color];
    }

    [ServerRpc]
    private void UpdateModelServerRpc(int _color)
    {
        nCharacter.Value = _color;

        if (IsOwner)
        {
            UpdateModel(_color);
        }
    }

    public void ChangedSkinBtn()
    {
        if (!IsOwner) return;
        color += 1;

        if (color > 3)
        {
            color = 0;
        }

        UIManager.Instance.UpdateColorPlayer(color);

        UpdateModelServerRpc(color);
    }

    /* public override void OnNetworkSpawn()
     {
         GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

         foreach (GameObject player in players)
         {
             if (!IsOwner) return;
             if (OwnerClientId == 0) Debug.Log("HOST ne");
             GetDataPlayer();
             int skin = color + model;
             UpdateModelServerRpc(skin);
             playerHUDImg.SetActive(true);
         }
     }

    private void GetModel()
     {
         *//*   transform.GetComponent<SpriteRenderer>().sprite = skins[(int)OwnerClientId];
            transform.GetComponent<Animator>().runtimeAnimatorController = controllers[(int)OwnerClientId];*//*
         int skin = color + model;
         transform.GetComponent<SpriteRenderer>().sprite = skins[skin];
         transform.GetComponent<Animator>().runtimeAnimatorController = controllers[skin];
     }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateModelServerRpc(int skin)
    {
        UpdateModelClientRpc(skin, OwnerClientId);
    }

    [ClientRpc]
    private void UpdateModelClientRpc(int skin, ulong clientId)
    {
        if (clientId == OwnerClientId)
        {
            transform.GetComponent<SpriteRenderer>().sprite = skins[skin];
            transform.GetComponent<Animator>().runtimeAnimatorController = controllers[skin];
        }
    }
     */
}
