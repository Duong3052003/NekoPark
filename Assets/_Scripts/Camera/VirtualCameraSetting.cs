using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCameraSetting : MonoBehaviour
{
    public static VirtualCameraSetting Instance { get; private set; }

    private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float targetFOV = 7.5f;
    [SerializeField] private float zoomSpeed = 2f;

    private float initialFOV;
    private bool isZooming = false;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        initialFOV = virtualCamera.m_Lens.FieldOfView;

        isZooming = true;
    }

    private void Update()
    {
        if (!isZooming) return;
        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, targetFOV, Time.deltaTime * zoomSpeed);

        if (Mathf.Abs(virtualCamera.m_Lens.OrthographicSize - targetFOV) < 0.1f)
        {
            virtualCamera.m_Lens.FieldOfView = targetFOV;
            isZooming = false;
        }

    }

    public void ChangeFieldOfView(float _targetFOV)
    {
        targetFOV = _targetFOV;
        isZooming = true;
    }
}
