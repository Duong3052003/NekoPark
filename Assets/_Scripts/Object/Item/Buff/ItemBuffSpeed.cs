using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ItemBuffSpeed : ItemBuff
{
    [SerializeField] private float percentSpeed=1.25f;

    public override void Effect(GameObject _gameObject)
    {
        int playerIndex = PlayerManager.Instance.players.FindIndex(player => player == _gameObject);

        if (playerIndex != -1)
        {
            ApplyEffectClientRpc(playerIndex);
        }
    }

    [ClientRpc]
    private void ApplyEffectClientRpc(int _index)
    {
        var playerMove = PlayerManager.Instance.players[_index].GetComponent<PlayerMove>();
        playerMove.SetSpeed(percentSpeed);
    }
}
