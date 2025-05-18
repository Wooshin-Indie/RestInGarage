using Garage.Controller;
using IUtil;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using System.Runtime.CompilerServices;
using Garage;
using Garage.Utils;
using Garage.Structs;

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

        [SerializeField] private GameObject carPrefab;
        [SerializeField] private GameObject spawnPointPrefab;
        //[SerializeField] private List<LaneData> spawningPoints = new List<LaneData>();
        [SerializeField] private SpawnPointData spawnPointData1;
        private float curMapLaneLength;
        private float curMapLaneWidth;
        public float CurMapLaneWidth => curMapLaneWidth;
        private float curMapRemoveLength;
        private List<VehicleSpawnPoint> spawnPoints = new List<VehicleSpawnPoint>();

        /// <summary>
        /// mapId, stageId 에 따라 spawnPoints를 설정합니다.
        /// </summary>
        [Button]
		public void OnStageStart(/*int mapId, int stageId*/) // 서버에서 호출
		{
			spawnPoints.Clear();
            curMapLaneLength = spawnPointData1.LaneLength;
            curMapLaneWidth = spawnPointData1.LaneWidth;
            curMapRemoveLength = spawnPointData1.RemoveLength;
            foreach (var sp in spawnPointData1.SpawningPoints)
			{
				Vector3 point = new Vector3(sp.SpawnPointX, 0, 0);
                //Up이면 아래쪽(z축 -쪽)에 스폰, Down이면 위쪽(z축 +쪽)에 스폰
                point.z = (sp.Direction == VehicleDirection.Up ? -curMapLaneLength : curMapLaneLength);
                VehicleSpawnPoint vsp = Instantiate(spawnPointPrefab, point, Quaternion.identity)
									.GetComponent<VehicleSpawnPoint>();
				vsp.SetSpawnDir(sp.Direction);
                spawnPoints.Add(vsp);
            }
		}

		/// <summary>
		/// 자동 스폰 or 게임 오버 시 남아있는 차들을 정리합니다.
		/// </summary>
		public void OnStageEnd()
		{

		}

		[Button]
		public void SpawnCar() // 서버에서 호출
		{
			List<VehicleSpawnPoint> availableSpawnPoints = spawnPoints.Where(p => p.IsAbleToSpawn()).ToList();

			if (availableSpawnPoints.Count > 0)
			{
				VehicleSpawnPoint spawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
                CarController car = Instantiate(carPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation).
					GetComponent<CarController>();
                car.GetComponent<NetworkObject>().Spawn();

                car.SetLane(spawnPoint.transform.position.x, curMapRemoveLength, spawnPoint.Direction);
                car.InitCarStatusServer();
			}
			else return;
		}

		public void DespawnCar(CarController car)
		{
			UIManager.Game.RemoveAllCarStatusUI(car);
            car.GetComponent<NetworkObject>().Despawn();
			Destroy(car.gameObject);
		}
	}
}