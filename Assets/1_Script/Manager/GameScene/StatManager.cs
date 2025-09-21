using System;
using System.Collections.Generic;
using Garage.Manager;
using Garage.Props;
using Garage.Utils;
using UnityEngine;

namespace Manager {
    public enum StatEnum {
        None = -1,
        PlayerSpeed = 0,
        CarrySpeed,
        WrenchRepairSpeed,
        OilRepairSpeed,
        FireExtinguishSpeed,
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
        private Dictionary<StatEnum, float> speedBoostDict = new(); // 0 ~ ... , Ex) 30% == 0.3
        private KeyValuePair<StatEnum, float> nonePerk = new(StatEnum.None, 0f);
        private KeyValuePair<StatEnum, float> currentPerk = new(StatEnum.None, 0f);
        public  KeyValuePair<StatEnum, float> CurrentPerk => currentPerk;
        // TODO - Perk이 여러개 설정 가능하다면 List로 바꿔야됨

        private void Start()
        {

            statDict.Add(StatEnum.PlayerSpeed, 1f);
            statDict.Add(StatEnum.CarrySpeed, 1f);
            statDict.Add(StatEnum.WrenchRepairSpeed, 1f);
            statDict.Add(StatEnum.OilRepairSpeed, 1f);
            statDict.Add(StatEnum.FireExtinguishSpeed, 1f);
            foreach (StatEnum statEnum in Enum.GetValues(typeof(StatEnum)))
            {
                speedBoostDict.Add(statEnum, 0f);
            }
            GameManagerEx.Instance.OnStartGameAction += OnGameStart;
        }

        private void OnGameStart(int mapIdx)
        {
            // TODO - 맵에 관련된 스탯을 추가할 수도 있음
            //StatEnum[] statEnums = (StatEnum[])Enum.GetValues(typeof(StatEnum));
            StatEnum[] statEnums = {
                StatEnum.PlayerSpeed,
                StatEnum.CarrySpeed,
                StatEnum.WrenchRepairSpeed,
				StatEnum.OilRepairSpeed,
				StatEnum.FireExtinguishSpeed
			};
            if (statEnums.Length != Enum.GetValues(typeof(StatEnum)).Length - 1)
                Debug.LogError("Update local \"StatEnum[] statEnums\"");

            float[] values = new float[statEnums.Length];
            for (int i = 0; i < statEnums.Length; i++)
            {
                values[i] = GetStat(statEnums[i]);
            }

            NetworkTransmission.instance.ApplyStatsServerRPC(GameManagerEx.Instance.MyClientId, statEnums, values);
        }

        public void SetCurrentPerk(KeyValuePair<StatEnum, float> perk)
        {
            if (currentPerk.Key != StatEnum.None)
                SetStat(currentPerk.Key, 1f); // 이전에 선택된 perk은 원래대로

            SetStat(perk.Key, perk.Value);
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

        public float GetProgressSpeed(CarParts part)
        {
            StatEnum statEnum = StatEnum.None;
            switch (part) {
                case CarParts.FLT:
                case CarParts.FRT:
                case CarParts.RLT:
                case CarParts.RRT:
                case CarParts.Engine:
                    statEnum = StatEnum.WrenchRepairSpeed;
                    break;
                case CarParts.Oil:
					statEnum = StatEnum.OilRepairSpeed;
					break;
				case CarParts.Fire:
                    statEnum = StatEnum.FireExtinguishSpeed;
                    break;
            }
            float speedStat = statDict.ContainsKey(statEnum) ? statDict[statEnum] : 1f;
            float speedBoost = speedBoostDict.ContainsKey(statEnum) ? speedBoostDict[statEnum] : 1f;
            speedStat = speedStat + 1f * speedBoost;

            return speedStat;
		}


		public void SetStat(StatEnum statEnum, float value)
        {
            statDict[statEnum] = value;
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

        private float boostUpDuration = 4f;
        private float boostDownDuration = 6f;
        private float boostLimit = 0.2f;
        /// <summary>
        /// 여기서 프랍별로 속도 추가 어떻게 할 지 결정
        /// </summary>
        public void UpdateInteractSpeedBoost(OwnableProp prop, bool isInteractPressed)
        {
            float curSpeedBoost = 0f;
            if (prop is WrenchProp) // 임시로 WrenchProp만 해놓음 
            {
                curSpeedBoost = speedBoostDict[StatEnum.WrenchRepairSpeed];

                float boostDelta = isInteractPressed ? ( Time.deltaTime / boostUpDuration ) * boostLimit
                    : ( -Time.deltaTime / boostDownDuration ) * boostLimit;
                curSpeedBoost += boostDelta;

                speedBoostDict[StatEnum.WrenchRepairSpeed] = curSpeedBoost;
                if (curSpeedBoost > boostLimit)
                {
                    speedBoostDict[StatEnum.WrenchRepairSpeed] = boostLimit;
                    return;
                }
                if (curSpeedBoost < 0f)
                {
                    speedBoostDict[StatEnum.WrenchRepairSpeed] = 0f;
                    return;
                }
            }
        }
    }
}