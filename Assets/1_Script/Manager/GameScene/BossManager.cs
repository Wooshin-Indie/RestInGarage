using Garage.Utils;
using Garage.Vehicle;
using System.Collections;
using UnityEngine;

namespace Garage.Manager
{
    public enum BossType
    {
        None = -1,
        BikerGang,

    }
    [System.Serializable]
    public class BossWaveInfo
    {
        public bool isBossExist;
        public float appearingTime;
        public BossType bossType;
    }
    public class BossManager : MonoBehaviour
    // Host에서만 접근함
    {
        #region Singleton
        private static BossManager instance;
        public static BossManager Instance { get => instance; }

        void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (null == instance)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
        #endregion

        [SerializeField] private Vector2 bikerGangSpawnInterval;
        [SerializeField] private float bikerGangEventDuration;

        private bool isBossAppeared = false;
        public bool IsBossAppeared => isBossAppeared;

        private Vector3[] bossSpawnPoints = null;

        private void Start()
        {
            GameManagerEx.Instance.OnBeforeStageEndAction += OnStageEnd;
        }

        public void StartBossFight(BossType bossType)
        {
            // TODO - 보스전이 몇초동안 진행될지 설정, 어디어디 스테이지에서 진행될지 설정(bossStarted값 관리해줘야함)
            // TODO - 보스타입 맞춰서 보스전 시작해야함
            StartBikerGangFight();
            isBossAppeared = true;
        }

        private Coroutine curBossFightCoroutine = null;
        private void StartBikerGangFight()
        {
            Debug.Log("Start BikerGangFight");
            SetBossSpawnPoints();
            curBossFightCoroutine = StartCoroutine(BikerGangFightCoroutine());

            NetworkTransmission.instance.StartBossWarningFXClientRPC();
        }

        // 지금은 BikerGang전용으로 해놓음
        private void SetBossSpawnPoints()
        {
            bossSpawnPoints = new Vector3[TrafficManager.Instance.CurMapData.SpawningPoints.Count * 2];
            float laneWidth = TrafficManager.Instance.CurMapData.LaneWidth;
            float laneLength = TrafficManager.Instance.CurMapData.LaneLength;
            int i = 0;
            foreach (var spData in TrafficManager.Instance.CurMapData.SpawningPoints)
            {
                bossSpawnPoints[i++] = new Vector3(spData.SpawnPointX + laneWidth / 2, 0, laneLength);
                bossSpawnPoints[i++] = new Vector3(spData.SpawnPointX - laneWidth / 2, 0, laneLength);
            }
            Debug.Log("bossSpawnPoints Count: " + bossSpawnPoints.Length);
        }

        private float elapsedTime = 0f;
        private IEnumerator BikerGangFightCoroutine()
        {
            Debug.Log("Coroutine Start: BikerGangFight");
            float eventCycleTime = bikerGangEventDuration;
            int curEventCount = 0;
            elapsedTime = 0f;
            int curSpawnIdx = 0;
            int preSpawnIdx = 0;
            while (elapsedTime < bikerGangEventDuration) // bikerGangEventCount만큼 폭주족 웨이브 왔었으면 종료
            {
                float randomSpawnInterval = Random.Range(bikerGangSpawnInterval.x, bikerGangSpawnInterval.y);

                curSpawnIdx = Random.Range(0, bossSpawnPoints.Length);
                if (preSpawnIdx == curSpawnIdx)
                {
                    if (curSpawnIdx == 0)
                        curSpawnIdx++;
                    else
                        curSpawnIdx--;
                }

                TrafficManager.Instance.SpawnBikerGang(bossSpawnPoints[curSpawnIdx]);

                preSpawnIdx = curSpawnIdx;

                elapsedTime += Time.deltaTime;
                yield return new WaitForSeconds(randomSpawnInterval);
            }
        }

        private void OnStageEnd()
        {
            Debug.Log("Stage ended and try to stop BossFightCoroutine");
            if (curBossFightCoroutine != null)
            {
                StopCoroutine(curBossFightCoroutine);
                curBossFightCoroutine = null;
            }
        }

        private void OnDestroy()
        {
            GameManagerEx.Instance.OnBeforeStageEndAction -= OnStageEnd;
        }
    }

}