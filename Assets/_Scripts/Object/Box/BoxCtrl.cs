using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BoxCtrl : NetworkBehaviour
{
    public BoxCollision boxCollision { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Animator animator { get; private set; }

    private void Awake()
    {
        LoadComponents();
    }

    private void LoadComponents()
    {
        boxCollision = GetComponent<BoxCollision>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandlerCollision();
    }

    private void HandlerCollision()
    {
        if (boxCollision.IsTouching())
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }
}
