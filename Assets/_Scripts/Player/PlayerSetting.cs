using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerSetting : NetworkBehaviour
{
    private Sprite[] skins;
    private RuntimeAnimatorController[] controllers;
    [SerializeField] private GameObject playerHUDImg;

    [SerializeField] private Button startBtn;
    [SerializeField] private Button changedColorBtn;

    private int color;
    private NetworkVariable<int> nCharacter = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        skins = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().skins;
        controllers = GameObject.Find("=====LevelStorage=====").GetComponent<LevelStorage>().controllers;

        nCharacter.OnValueChanged += (oldValue, newValue) => UpdateModel();

        UpdateModel();
        if (!IsOwner) return;
        SetUpPlayer();
    }

    private void OnDisable()
    {
        playerHUDImg.SetActive(false);
    }

    public void SetUpPlayer()
    {
        if (IsOwner)
        {
            GetDataPlayer();

            if (SceneManager.GetActiveScene().name == "SampleScene")
            {
                GetButton();
            }
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
                    nCharacter.Value = color;
                }
            }
        }
    }

    private void GetButton()
    {
        playerHUDImg.SetActive(true);

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

    private void UpdateModel()
    {
        transform.GetComponent<SpriteRenderer>().sprite = skins[nCharacter.Value];
        transform.GetComponent<Animator>().runtimeAnimatorController = controllers[nCharacter.Value];
        this.gameObject.layer = LayerMask.NameToLayer(UIManager.Instance.GetStringColor(nCharacter.Value));
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
        //PlayerManager.Instance.SetColor(color);
        nCharacter.Value = color;
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
