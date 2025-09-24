using Garage.Utils;
using Garage.Manager;
using System;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

namespace Garage.Structs
{
    public class CarStatus
    {
        // Each Status is PartStatus.Fine or PartStatus.Broken
        public CarStatus()
        {
            Array values = Enum.GetValues(typeof(CarParts));
            isBroken = 0; // isBroken에 부품들(CarParts) 상태를 비트마스킹
            hasTire = 0;
            int count = UnityEngine.Random.Range(1, 4); // 고장날 CarPart 개수 (타이어 제외)
            int brokenTireLimit = GameManagerEx.Instance.CurMapData.
                StageDatas[GameManagerEx.Instance.CurStageIdx].
                BrokenTireLimit;
            int tireCount = UnityEngine.Random.Range(0, brokenTireLimit); // 고장날 타이어 개수

            count = count < tireCount ? 0 : count - tireCount;

            List<int> brokenIdxs = Utility.GetUniqueRandomsByShuffle(0, 3, tireCount); // 고장날 타이어 인덱스 추가
            brokenIdxs.AddRange(Utility.GetUniqueRandomsByShuffle(4, values.Length - 1, count)); 
            // 타이어 제외한 고장날 CarPart 인덱스 추가
            // values.Length - 1 은 CarParts.Fire 제외하려고

            foreach (int idx in brokenIdxs)
            {
                if ((isBroken & (1 << idx)) == 0) // LSB부터 idx번째 isBroken이 0이면 실행
                {
                    isBroken |= 1 << idx; // (1 << idx) 에 해당하는 비트 켜기
                    count--;
                }
            }

            progress = new float[values.Length];
        }

        public int isBroken; // CarParts 상태를 LSB부터 비트마스킹
        private int hasTire;
        private float[] progress; // 0 ~ 1
        private float fireProgress = -1f;
        // FireProgress는 host에서만 만짐
        public float FireProgress { get => fireProgress; set => fireProgress = value > 1.1f ? 1.1f : value; }
        public int HasTire { get => hasTire; set => hasTire = value; }
        public float[] Progress { get => progress; set => progress = value; }

		public bool IsProgressFull(CarParts part)
        {
            return progress[(int)part] >= 1f;
        }
        public bool IsTireEmpty(CarParts part)
        {
            return ((hasTire & (1<<(int)part)) == 0);
		}
        public bool IsBroken(CarParts part)
        {
            return (isBroken & (1 << (int)part)) != 0;
        }
        public float GetProgress(CarParts part)
        {
            if ((isBroken & (1 << (int)part)) == 0) return float.MaxValue;
            return progress[(int)part];
        }
        public void AddProgress(CarParts part, float gage)
		{
			progress[(int)part] += gage;
            //Debug.Log($"{part} : {progress[(int)part]}");
            return;
        }
        public void AddTire(CarParts part)
		{
			hasTire |= 1 << (int)part;
		}
        public void SetIsBrokenAsFalse(CarParts part)
        {
            isBroken &= ~(1 << (int)part);
        }
        public bool IsThereAnyBroken()
        {
            return isBroken != 0 || fireProgress > 0f;
        }

        public bool IsFiring()
        {
            return fireProgress > 0f;
		}

        public void ExtinguishFire(float gage)
        {
            fireProgress += gage;
        }

        public void StartFire(float startProgress)
        {
            if (IsFiring()) return;

            fireProgress = startProgress;
        }
	}
}