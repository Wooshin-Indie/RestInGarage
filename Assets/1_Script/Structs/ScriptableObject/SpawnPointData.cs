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

    [CreateAssetMenu(fileName = "SpawnPoint Data", menuName = "SO/SpawnPoint Data")]
    public class SpawnPointData : ScriptableObject
    {
        [SerializeField] private List<LaneData> spawningPoints = new List<LaneData>();
        [SerializeField] private float laneLength;  // 차량이 스폰부터 정비소까지 달려오는 거리
        [SerializeField] private float laneWidth;
        [SerializeField] private float removeLength; // 정비소부터 사라지는 곳 까지의 거리
        public List<LaneData> SpawningPoints => spawningPoints;
        public float LaneLength => laneLength;
        public float LaneWidth => laneWidth;
        public float RemoveLength => removeLength;
    }
}
