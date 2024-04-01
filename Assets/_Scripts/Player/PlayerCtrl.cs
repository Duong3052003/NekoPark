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

    private void Awake()
    {
        LoadComponents();
    }

    private void LoadComponents()
    {
        checkGroundColiision = checkGroundColiisionObj.GetComponent<CheckGroundColiision>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
}
