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

    [SerializeField] private int numberEnemyCurrent;
    [SerializeField] private bool isEndAllWaves = false;

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

    protected virtual void Update()
    {
        CheckWin();
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
            if (!enemyShip[n].GetComponent<NetworkObject>().IsSpawned)
            {
                enemyShip[n].GetComponent<NetworkObject>().Spawn();
            }

            enemyShip[n].GetComponent<IObjectServerSpawn>().Spawn(Vector3.zero,new Vector2(firstStyle[waveCurrent],0));

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
        if (waveCurrent < numberEnemyPerWave.Length)
        {
            timer++;
            if (timer - (timeBetweenWave + timeDelayPerWave[waveCurrent]) == 0)
            {
                Wave(waveCurrent);
                timer = 0;
            }
        }
        else
        {
            isEndAllWaves = true;
        }
    }

    private void Wave(int i)
    {
        size.x = numberEnemyPerWave[i];
        GenerateObj(size.y);
        waveCurrent++;
        Debug.Log("|=== Wave ===|" + waveCurrent);
    }

    private void CheckWin()
    {
        if (!isEndAllWaves || !IsHost) return;
        numberEnemyCurrent = 0;
        for(int i=0; i < objSpawnedHolders.Length; i++)
        {
            numberEnemyCurrent = numberEnemyCurrent +objSpawnedHolders[i].transform.childCount;
        }
        if (numberEnemyCurrent != 0) return;
        isEndAllWaves = false;

        if (EnemySnake.Instance == null) return;
        EnemySnake.Instance.GenerateObjsServerRPC();
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
