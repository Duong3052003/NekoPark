using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortGun : MonoBehaviour
{
    public Transform headTransform;
    public Transform positionTransform;

    private void OnEnable()
    {
        BulletSpawn.Instance.AddPorts(this.gameObject.transform);
    }

    private void OnDisable()
    {
        BulletSpawn.Instance.RemovePort(this.gameObject.transform);
    }
}
