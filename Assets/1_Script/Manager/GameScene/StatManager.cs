using System.Collections.Generic;
using Garage.Manager;
using UnityEngine;

namespace Manager {
    public enum StatEnum {
        None = -1,
        PlayerSpeed = 0,

    }

    public class StatManager : MonoBehaviour
    {

        #region Singleton
        private static StatManager instance;
        public static StatManager Instance { get => instance; }

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

        private Dictionary<StatEnum, float> statDict = new();
        private KeyValuePair<StatEnum, float> currentPerk;
        // TODO - Perk이 여러개 설정 가능하다면 List로 바꿔야됨

        void Start()
        {
            GameManagerEx.Instance.OnStartGameAction += OnGameStart;
        }

        private void OnGameStart(int mapIdx)
        {
            // TODO - 맵에 관련된 스탯을 추가할 수도 있음
            statDict.Clear();
            statDict.Add(currentPerk.Key, currentPerk.Value);
        }

        public void SetCurrentPerk(KeyValuePair<StatEnum, float> perk)
        {
            currentPerk = perk;
        }

        public void AddStat(StatEnum statEnum, float amount)
        {
            if (!statDict.ContainsKey(statEnum))
            {
                statDict.Add(statEnum, amount);
            }
            else
            {
                statDict[statEnum] += amount;
            }
        }

        public void RemoveStat(StatEnum statEnum, float amount)
        {
            if (!statDict.ContainsKey(statEnum))
            {
                statDict.Add(statEnum, amount);
            }
            else
            {
                statDict[statEnum] -= amount;
            }
        }

        public float GetStat(StatEnum statEnum)
        {
            if (!statDict.ContainsKey(statEnum))
            {
                return 0f;
            }
            else
            {
                return statDict[statEnum];
            }
        }
    }
}