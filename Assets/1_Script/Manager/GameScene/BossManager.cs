using Garage.Utils;
using Garage.Vehicle;
using Newtonsoft.Json.Bson;
using NUnit.Framework;
using System.Collections;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Garage.Manager
{
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
        
        private bool bossStarted = false;
        public bool BossStarted => bossStarted;

        private Vector3[] bossSpawnPoints = null;

        private void Start()
        {
            GameManagerEx.Instance.OnBeforeStageEndAction += OnStageEnd;
        }

        public void StartBossFight()
        {
            // TODO - 보스전이 몇초동안 진행될지 설정, 어디어디 스테이지에서 진행될지 설정(bossStarted값 관리해줘야함)
            // HACK - 무슨 보스랑 싸울지 설정(임시)
            StartBikerGangFight();
            bossStarted = true;
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
            bossSpawnPoints = new Vector3[TrafficManager.Instance.CurStageData.SpawningPoints.Count * 2];
            float laneWidth = TrafficManager.Instance.CurStageData.LaneWidth;
            float laneLength = TrafficManager.Instance.CurStageData.LaneLength;
            int i = 0;
            foreach (var spData in TrafficManager.Instance.CurStageData.SpawningPoints)
            {
                bossSpawnPoints[i++] = new Vector3(spData.SpawnPointX + laneWidth/2, 0, laneLength);
                bossSpawnPoints[i++] = new Vector3(spData.SpawnPointX - laneWidth/2, 0, laneLength);
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

            while (elapsedTime < bikerGangEventDuration) // bikerGangEventCount만큼 폭주족 웨이브 왔었으면 종료
            {
                float randomSpawnInterval = Random.Range(bikerGangSpawnInterval.x, bikerGangSpawnInterval.y);

                TrafficManager.Instance.SpawnBikerGang(bossSpawnPoints[Random.Range(0, bossSpawnPoints.Length)]);

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
