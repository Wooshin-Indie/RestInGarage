using Garage.Controller;
using IUtil;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Garage.UI.GameScene;
using Unity.Netcode;

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

        public GameObject carPrefab;
		public GameObject spawnPointPrefab;
		public List<VehicleSpawnPoint> spawnPoints = new();
		public float laneLength;

		/// <summary>
		/// mapId, stageId 에 따라 spawnPoints를 설정합니다.
		/// </summary>
		[Button]
		public void OnStageStart(/*int mapId, int stageId*/) // 서버에서 호출
		{
			spawnPoints.Clear();

			VehicleSpawnPoint sp = Instantiate(spawnPointPrefab, new Vector3(-10, 0, laneLength), Quaternion.identity).GetComponent<VehicleSpawnPoint>();
			sp.SetSpawnPoint(Utils.VehicleDirection.Down);
			spawnPoints.Add(sp);
			VehicleSpawnPoint sp1 = Instantiate(spawnPointPrefab, new Vector3(0, 0, -laneLength), Quaternion.identity).GetComponent<VehicleSpawnPoint>();
			sp1.SetSpawnPoint(Utils.VehicleDirection.Up);
			spawnPoints.Add(sp1);
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

				car.SyncIsBrokenClientRPC(car.CarStatus.isBroken);
                car.SetLane(spawnPoint.transform.position.x, spawnPoint.transform.position.z > 0 ? Utils.VehicleDirection.Down : Utils.VehicleDirection.Up);
                car.InitCarStatus();
			}
			else return;
		}

		
	}
}