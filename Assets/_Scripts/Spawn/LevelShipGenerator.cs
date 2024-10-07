using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class LevelShipGenerator : LevelGenerator
{
    public static LevelShipGenerator Instance { get; private set; }

    [SerializeField] private float timeBetweenWave;
    [SerializeField] private float[] timeDelayPerWave;
    [SerializeField] private int[] numberEnemyPerWave;
    [SerializeField] private float[] firstStyle;
    [SerializeField] private bool[] inScene;

    [SerializeField] private float timer;
    [SerializeField] private bool isPause;

    [SerializeField] private int waveCurrent=0;

    [SerializeField] private List<GameObject> enemyShip;
    private int enemyShipCurrent;
    [SerializeField] private List<GameObject> posSpawnNeedToGo;
    [SerializeField] private GameObject posSpawnNeedToGoCurrent;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkTimer.Instance.CurrentTick.OnValueChanged += (oldValue, newValue) => ExcuteTime(newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    protected override void GenerateObjsServerRPC(int k, int i, int j)
    {
        if (inScene[waveCurrent] == false)
        {
            GenerateNewObj(k, i, j);
        }
        else
        {
            GenerateObjInScene(k, i, j);
        }
        
    }

    private void GenerateNewObj(int k, int i, int j)
    {
        objSpawned = ObjIsSpawned();

        objSpawned.transform.position = objSpawnedHolders[k].transform.position + new Vector3((float)((size.x - 1) * 0.5f - i) * offset.x, j * offset.y, 0);

        objSpawned.GetComponent<IObjectServerSpawn>().Spawn(Vector3.zero, new Vector2(firstStyle[waveCurrent], 0));

        objSpawned.transform.SetParent(objSpawnedHolders[k].transform);
        int hp;
        if (mupltiHP == true)
        {
            hp = hpObj * (j + 1);
        }
        else
        {
            hp = hpObj;
        }
        objSpawned.GetComponent<ObjDeSpawnByHp>().SetHp(hp);
    }

    private void GenerateObjInScene(int k, int i, int j)
    {
        for(int n =enemyShipCurrent; n < enemyShip.Count; n++)
        {
            enemyShip[n].gameObject.SetActive(true);
            if (!enemyShip[n].GetComponent<NetworkObject>().IsSpawned)
            {
                enemyShip[n].GetComponent<NetworkObject>().Spawn();
            }

            if (mupltiHP == true)
            {
                hpObj = 3 * (j + 1);
            }
            enemyShip[n].GetComponent<ObjDeSpawnByHp>().SetHp(hpObj);
        }
    }

    public GameObject Target()
    {
        List<GameObject> validTargets = posSpawnNeedToGo
        .Where(obj => obj != posSpawnNeedToGoCurrent)
        .ToList();

        if (validTargets.Count == 0)
        {
            Debug.LogWarning("Khong co muc tieu.");
            return null;
        }
        posSpawnNeedToGoCurrent = RandomGameObjectFromList.GetRandomGameObject(validTargets);
        return posSpawnNeedToGoCurrent;
    }

    private void ExcuteTime(float newTime)
    {
        if (isPause || !IsHost) return;
        if (waveCurrent >= numberEnemyPerWave.Length) return;
        timer++;
        if (timer - (timeBetweenWave + timeDelayPerWave[waveCurrent]) == 0)
        {
            Wave(waveCurrent);
            timer =0;
        }
    }

    private void Wave(int i)
    {
        size.x = numberEnemyPerWave[i];
        GenerateObj(size.y);
        waveCurrent++;
        Debug.Log("|=== Wave ===|" + waveCurrent);
    }

    public override void OnPause(int time)
    {
        if (!IsHost) return;
        isPause = true;
        SetActivePlayersServerRPC();
    }

    public override void OnResume()
    {
        GenerateObj(size.y);
        if (!IsHost) return;
        isPause = false;
        SetUpLevelServerRPC();
        PlayerManager.Instance.GetSettingStatusPlayer();
    }
}
