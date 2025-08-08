using Garage.Controller;
using IUtil;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Garage.Utils;
using Garage.Structs;
using System;
using Garage.Vehicle;

namespace Garage.Manager
{
	public class TrafficManager : NetworkBehaviour
	{
        #region Singleton
        private static TrafficManager instance;
        public static TrafficManager Instance { get => instance; }

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

		private void Start()
		{
            GameManagerEx.Instance.OnStartGameAction += OnStageStart;
		}

		[SerializeField] private List<GameObject> carPrefabList = new();
		[SerializeField] private GameObject bikerGangPrefab;
        [SerializeField] private GameObject spawnPointPrefab;
        [SerializeField] private GameObject lanePrefab;
        
        private MapData curStageData;
        public MapData CurStageData => curStageData;

        private List<VehicleSpawnPoint> spawnPoints = new List<VehicleSpawnPoint>();
        private Dictionary<ulong, CarController> curStageCars = new Dictionary<ulong, CarController>();

        /// <summary>
        /// mapId, stageId 에 따라 spawnPoints를 설정합니다.
        /// </summary>
		public void OnStageStart(int mapIdx)
		{
            curStageData = Managers.Resource.GetData<MapData>(mapIdx);

            spawnPoints.Clear();
            foreach (var sp in curStageData.SpawningPoints)
			{
				Vector3 point = new Vector3(sp.SpawnPointX, 0, 0);
                point.z = (sp.Direction == VehicleDirection.Up ? -curStageData.LaneLength : curStageData.LaneLength);
                VehicleSpawnPoint vsp = Instantiate(spawnPointPrefab, point, Quaternion.identity)
									.GetComponent<VehicleSpawnPoint>();
                Managers.Scene.MoveGameObjectToCurrentScene(vsp.gameObject);
				vsp.SetSpawnDir(sp.Direction);
                spawnPoints.Add(vsp);
            }
        }

        /// <summary>
        /// 자동 스폰 or 게임 오버 시 남아있는 차들을 정리합니다.
        /// </summary>
        public void OnStageEnd()
		{
            foreach (var carDict in curStageCars)
            {
                carDict.Value.OnStageEnd();
            }
        }

		[Button]
		public void SpawnCar() // 서버에서 호출
		{
			List<VehicleSpawnPoint> availableSpawnPoints = spawnPoints.Where(p => p.IsAbleToSpawn()).ToList();

			if (availableSpawnPoints.Count > 0)
			{
				VehicleSpawnPoint spawnPoint = availableSpawnPoints[UnityEngine.Random.Range(0, availableSpawnPoints.Count)];
                CarController car = Instantiate(carPrefabList[UnityEngine.Random.Range(0, carPrefabList.Count())], spawnPoint.transform.position, spawnPoint.transform.rotation).
					GetComponent<CarController>();
                car.GetComponent<NetworkObject>().Spawn();
                car.InitCarController(spawnPoint);

                curStageCars.Add(car.GetComponent<NetworkObject>().NetworkObjectId, car);

			}
			else return;
		}

		public void DespawnCar(CarController car)
		{
			UIManager.Game.RemoveAllCarStatusUI(car);

            car.GetComponent<NetworkObject>().Despawn();
            curStageCars.Remove(car.GetComponent<NetworkObject>().NetworkObjectId);
            Destroy(car.gameObject);
		}


        public BikerGang SpawnBikerGang(Vector3 spawnPoint)
        {
            // TrafficManager에서 물량 관리
            // 나중에 필요하면 Despawn도 여기에서 하면 될 듯?
            BikerGang bikerGang = Instantiate(bikerGangPrefab, spawnPoint, Quaternion.Euler(0f, 180f, 0f)).GetComponent<BikerGang>();
            bikerGang.GetComponent<NetworkObject>().Spawn();

            return bikerGang;
        }
	}
}