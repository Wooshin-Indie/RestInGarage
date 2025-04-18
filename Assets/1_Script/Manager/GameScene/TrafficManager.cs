using Garage.Controller;
using IUtil;
using UnityEngine;

namespace Garage.Manager
{
	public class TrafficManager : MonoBehaviour
	{
		public GameObject carPrefab;
		public Transform[] spawnPoints;

		/// <summary>
		/// mapId, stageId 에 따라 spawnPoints를 설정합니다.
		/// </summary>
		public void OnStageStart(int mapId, int stageId)
		{

		}

		/// <summary>
		/// 자동 스폰 or 게임 오버 시 남아있는 차들을 정리합니다.
		/// </summary>
		public void OnStageEnd()
		{

		}

		private static int laneIdx = 0;
		[Button]
		public void SpawnCar()
		{
			Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
			GameObject carObj = Instantiate(carPrefab, spawnPoint.position, spawnPoint.rotation);
			carObj.GetComponent<CarController>().SetLane(laneIdx % 2);
			laneIdx++;
		}
	}
}