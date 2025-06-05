using Garage.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Garage.Structs
{
    [System.Serializable]
    public struct LaneData
    {
        public float SpawnPointX;
        public VehicleDirection Direction;
    }

    [CreateAssetMenu(fileName = "Stage Data", menuName = "SO/Stage Data")]
    public class StageData : ScriptableObject
    {
        [Header("Map Info")]
        [SerializeField] private List<LaneData> spawningPoints = new List<LaneData>();
        [SerializeField] private float laneLength;  // 차량이 스폰부터 정비소까지 달려오는 거리
        [SerializeField] private float laneWidth;
        [SerializeField] private float removeLength; // 정비소부터 사라지는 곳 까지의 거리
        [SerializeField] private Vector3 camRotation;
        private Vector3 stdPointWith2Lane = new Vector3(-5, 0, 0);
        private Vector3 stdPointWith3Lane = new Vector3(-8, 0, 0);
        private Vector3 standardPoint;

        [Header("Wave Info")]
        [SerializeField] private int maxStage;
        [SerializeField] private int[] laneCounts;
        [SerializeField] private Vector2[] spawnInterval;
        [SerializeField] private Vector2Int earnMoney;
        [SerializeField] private Vector2Int eraseMoney;


        private float playerRangeX; // 스테이지 시작 시 플레이어가 움직일 수 있는 범위
        private void OnEnable()
        {
            int laneNum = spawningPoints.Count;
            switch (laneNum)
            {
                case 2:
                    playerRangeX = 6;
                    standardPoint = stdPointWith2Lane;
                    break;
                case 3:
                    playerRangeX = 20;
                    standardPoint = stdPointWith3Lane;
                    break;
            }
		}

        public List<LaneData> SpawningPoints => spawningPoints;
        public float LaneLength => laneLength;
        public float LaneWidth => laneWidth;
        public float RemoveLength => removeLength;
        public float PlayerRangeX => playerRangeX;
        public Vector3 StandardPoint => standardPoint;
        public Vector3 CamRotation => camRotation;

        public Vector2Int EraseMoney => eraseMoney;
        public Vector2Int EarnMoney => earnMoney;
        public Vector2[] SpawnInterval => spawnInterval;
    }
}
