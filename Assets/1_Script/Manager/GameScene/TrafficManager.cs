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
using System;

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

            curStageData = stageData[0]; // 로비에서 사용할 스테이지 정보
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
        [SerializeField] private GameObject lanePrefab;
        [SerializeField] private List<StageData> stageData;
        private StageData curStageData;
        public StageData CurStageData => curStageData;
        private List<VehicleSpawnPoint> spawnPoints = new List<VehicleSpawnPoint>();

        /// <summary>
        /// mapId, stageId 에 따라 spawnPoints를 설정합니다.
        /// </summary>
        [Button]
		public void OnStageStart(/*int mapId, int stageId*/) // 서버에서 호출
		{
            curStageData = stageData[0]; // TODO - 파라미터로 받아와야됨
            // TODO - StageData 바뀔 때마다 콜백으로 StageData 내부 필드 참조하는 곳들 업데이트해줘야됨 (차량, 플레이어, 카메라 등등)

            spawnPoints.Clear();
            foreach (var sp in curStageData.SpawningPoints)
			{
				Vector3 point = new Vector3(sp.SpawnPointX, 0, 0);
                //Up이면 아래쪽(z축 -쪽)에 스폰, Down이면 위쪽(z축 +쪽)에 스폰
                point.z = (sp.Direction == VehicleDirection.Up ? -curStageData.LaneLength : curStageData.LaneLength);
                VehicleSpawnPoint vsp = Instantiate(spawnPointPrefab, point, Quaternion.identity)
									.GetComponent<VehicleSpawnPoint>();
				vsp.SetSpawnDir(sp.Direction);
                spawnPoints.Add(vsp);
            }

            OnStageStarted?.Invoke(curStageData);
        }

        public event Action<StageData> OnStageStarted;

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
				VehicleSpawnPoint spawnPoint = availableSpawnPoints[UnityEngine.Random.Range(0, availableSpawnPoints.Count)];
                CarController car = Instantiate(carPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation).
					GetComponent<CarController>();
                car.GetComponent<NetworkObject>().Spawn();

                car.SetLane(spawnPoint.transform.position.x, curStageData.RemoveLength, spawnPoint.Direction);
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