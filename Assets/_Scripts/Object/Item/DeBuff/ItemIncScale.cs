using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemIncScale : ItemDebuff
{
    private List<GameObject> clientBalls;

    public override void Effect(GameObject _gameObject)
    {
        ulong idOwner = PlayerManager.Instance.players[PlayerManager.Instance.players.FindIndex(player => player == _gameObject)].GetComponent<NetworkObject>().OwnerClientId;

        ApplyEffectClientRpc(idOwner);
    }

    [ClientRpc]
    private void ApplyEffectClientRpc(ulong idOwner)
    {
        clientBalls = LevelGenerator.Instance.CallAllBallsOfClient(idOwner);

        foreach (GameObject clientBall in clientBalls)
        {
            clientBall.transform.localScale = clientBall.transform.localScale * 1.25f;
        }
    }
}
