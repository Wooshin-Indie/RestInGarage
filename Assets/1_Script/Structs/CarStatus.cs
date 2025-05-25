using Garage.Utils;
using System;
using UnityEngine;

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
            int count = UnityEngine.Random.Range(1, 4); // 고장날 CarPart 개수

            while (count > 0)
            {
                int idx = UnityEngine.Random.Range(0, values.Length-1);
                if ((isBroken & (1 << idx)) == 0) // LSB부터 idx번째 isBroken이 0이면 실행
                {
                    isBroken |= 1 << idx; // (1 << idx) 에 해당하는 비트 켜기
                    count--;
                }
            }

            // HACK - 임시 테스트용
			fireProgress = (tc++ % 2 == 0) ? .1f : -1f;

            progress = new float[values.Length];
        }

        public static int tc = 0;

        public int isBroken; // CarParts 상태를 LSB부터 비트마스킹
        private int hasTire;
        private float[] progress; // 0 ~ 1
        private float fireProgress = -1f;

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

	}
}