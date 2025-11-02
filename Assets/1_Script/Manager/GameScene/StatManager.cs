using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<StatEnum, float> speedBoostDict = new(); // 기본 stat에 합연산으로 처리 Ex) 0.3 => 30%
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
            float speedBoost = speedBoostDict.ContainsKey(statEnum) ? speedBoostDict[statEnum] : 0f;
            speedStat = speedStat + speedBoost;

            Debug.Log(statEnum + ": " + speedStat);
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
        /// 고치고있으면 서서히 속도 증가, 안고치면 서서히 속도 감소
        /// 프랍별 속도 추가 어떻게 할 지 결정
        /// </summary>
        public void UpdateInteractSpeedBoosts(OwnableProp prop, bool isInteractPressed)
        {
            float curSpeedBoost = 0f;
            StatEnum statEnum = StatEnum.None;
            switch (prop)
            {
                case WrenchProp:
                    statEnum = StatEnum.WrenchRepairSpeed;
                    break;
                case Extinguisher:
                    isInteractPressed = Managers.Input.Control.Player.Action.IsPressed();
                    statEnum = StatEnum.FireExtinguishSpeed;
                    break;
                case OilPump:
                    statEnum = StatEnum.OilRepairSpeed;
                    break;
                default:
                    statEnum = StatEnum.None;
                    break;
            }

            foreach (StatEnum stEnum in speedBoostDict.Keys.ToList())
            {
                if (statEnum == stEnum) continue;
                speedBoostDict[stEnum] = 0f;
            }

            curSpeedBoost = speedBoostDict[statEnum];

            float boostDelta = isInteractPressed ? (Time.deltaTime / boostUpDuration) * boostLimit
                : (-Time.deltaTime / boostDownDuration) * boostLimit;
            curSpeedBoost += boostDelta;

            speedBoostDict[statEnum] = curSpeedBoost;
            if (curSpeedBoost > boostLimit)
            {
                speedBoostDict[statEnum] = boostLimit;
                return;
            }
            else if (curSpeedBoost < 0f)
            {
                speedBoostDict[statEnum] = 0f;
                return;
            }
        }
        private void UpdateSpeedBoost(StatEnum statEnum, bool isInteractPressed)
        {
            float curSpeedBoost = speedBoostDict[statEnum];

            float boostDelta = isInteractPressed ? (Time.deltaTime / boostUpDuration) * boostLimit
                : (-Time.deltaTime / boostDownDuration) * boostLimit;
            curSpeedBoost += boostDelta;

            speedBoostDict[statEnum] = curSpeedBoost;
            if (curSpeedBoost > boostLimit)
            {
                speedBoostDict[statEnum] = boostLimit;
                return;
            }
            else if (curSpeedBoost < 0f)
            {
                speedBoostDict[statEnum] = 0f;
                return;
            }
        }
    }
}