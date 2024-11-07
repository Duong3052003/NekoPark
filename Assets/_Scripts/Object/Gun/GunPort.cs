using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPort : MonoBehaviour
{
    [SerializeField] public Transform headTransform;
    [SerializeField] public Transform positionTransform;

    //private Rigidbody2D rb;

    /*private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setting(bool _canRoatate)
    {
        targetTransform = Target().transform;
        canRotate = _canRoatate;
    }

    private void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        if (targetTransform != null && canRotate)
        {
            float angle = Mathf.Atan2(targetTransform.position.y, targetTransform.position.x) * Mathf.Rad2Deg +90;

            rb.rotation = Mathf.LerpAngle(rb.rotation, angle, Time.deltaTime * rotationSpeed);
        }
        else if(targetTransform == null)
        {
            targetTransform = Target().transform;
        }
        else
        {
            rb.rotation = 0;
        }
    }

    private GameObject Target()
    {
        List<GameObject> validTargets = PlayerManager.Instance.players;

        return RandomGameObjectFromList.GetRandomGameObject(validTargets);
    }*/
}
