using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ReviveObj : NetworkBehaviour
{
    public NetworkVariable<float> nLoadingValue = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private Image loadingImage;
    [SerializeField] private float speed;
    [SerializeField] private bool isActiving;
    [SerializeField] private GameObject playerObj;

    private Collider2D col;

    private void Awake()
    {
        col = this.GetComponent<Collider2D>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost) return;
        nLoadingValue.Value = 0;
    }

    void Update()
    {
        if (IsHost && isActiving == true)
        {
            if (nLoadingValue.Value < 1)
            {
                nLoadingValue.Value += speed * Time.deltaTime;
                col.enabled = false;
            }
            else
            {
                col.enabled = true;
            }
        }

        loadingImage.fillAmount = nLoadingValue.Value;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        RevivePlayerObj();
    }

    public void SetPlayerObj(GameObject _playerObj)
    {
        isActiving = true;
        playerObj = _playerObj;
    }

    private void RevivePlayerObj()
    {
        if (!IsHost) return;
        playerObj.gameObject.SetActive(true);
        playerObj.GetComponent<PlayerTakeDame>().ReviveClientRpc();
        isActiving = false;
        nLoadingValue.Value = 0;
    }
}
