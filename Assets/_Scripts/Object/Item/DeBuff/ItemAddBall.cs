using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemAddBall : ItemDebuff
{
    public override void Effect(GameObject _gameObject)
    {
        ulong idOwner = PlayerManager.Instance.players[PlayerManager.Instance.players.FindIndex(player => player == _gameObject)].GetComponent<NetworkObject>().OwnerClientId;

        List<GameObject> clientBalls =  LevelBricksGenerator.Instance.CallAllBallsOfClient(idOwner);

        foreach (GameObject clientBall in clientBalls)
        {
            LevelBricksGenerator.Instance.SpawnObjServerRpc(0,idOwner,clientBall.transform.position, new Vector2 (clientBall.GetComponent<Ball>().GetVelocity().x *-1, clientBall.GetComponent<Ball>().GetVelocity().y));
        }
    }
}
