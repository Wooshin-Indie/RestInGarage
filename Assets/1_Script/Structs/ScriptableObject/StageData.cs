using Garage.Manager;
using System.Collections.Generic;
using UnityEngine;

namespace Garage.Structs
{
    [CreateAssetMenu(fileName = "Stage Data", menuName = "SO/Stage Data")]
    public class StageData : ScriptableObject
    {
        [Header("Stage Info")]
        [SerializeField] private float stageTime;
        [SerializeField] private int firingCarLimit; // 불타고있는 차량 개수가 이 값 초과하면 스폰되는 차량에 불 안붙음
        [SerializeField] private float fireChance;
        [SerializeField] private int brokenTireLimit;
        [SerializeField] private Vector2 spawnInterval;
        [SerializeField] private Vector2Int earnMoney;
        [SerializeField] private Vector2Int eraseMoney;
        [SerializeField] private BossWaveInfo bossWaveInfo;

        public float StageTime => stageTime;
        public int FiringCarLimit => firingCarLimit;
        public float FireChance => fireChance;
        public Vector2 SpawnInterval => spawnInterval;
        public Vector2Int EarnMoney => earnMoney;
        public Vector2Int EraseMoney => eraseMoney;
        public BossWaveInfo BossWaveInfo => bossWaveInfo;
        public int BrokenTireLimit => brokenTireLimit;
    }
}