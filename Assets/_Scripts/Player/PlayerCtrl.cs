using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCtrl : MonoBehaviour
{
    [SerializeField] private GameObject checkGroundColiisionObj;
    public CheckGroundColiision checkGroundColiision { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Animator animator { get; private set; }
    public PlayerMove playerMove { get; private set; }
    public PlayerSpawn playerSpawn { get; private set; }
    public PlayerAnimator playerAnimator { get; private set; }
    public Collider2D col { get; private set; }


    private void Awake()
    {
        LoadComponents();
    }

    private void OnEnable()
    {
        SetActivePlayer(true);
    }

    private void LoadComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerMove = GetComponent<PlayerMove>();
        playerSpawn = GetComponent<PlayerSpawn>();
        playerAnimator = GetComponent<PlayerAnimator>();
        col = GetComponent<Collider2D>();

        if (checkGroundColiisionObj != null)
        {
            checkGroundColiision = checkGroundColiisionObj.GetComponent<CheckGroundColiision>();
        }
    }

    public void SetActivePlayer(bool boolen)
    {
        if (boolen == true)
        {
            playerAnimator.SetActiveLayer();
            rb.bodyType = RigidbodyType2D.Dynamic;
            col.isTrigger = false;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Static;
            col.isTrigger = true;
        }
    }
}
