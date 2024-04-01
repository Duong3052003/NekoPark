using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCollision : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


public class RaycastWallChecker : MonoBehaviour, ICollisionWallChecker
{
    public LayerMask wallLayer; // Layer ch? ??nh t??ng

    public bool IsTouchingWall()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 0.1f, wallLayer))
        {
            return true;
        }
        return false;
    }
}
