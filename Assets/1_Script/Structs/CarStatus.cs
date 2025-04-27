using Garage.Props;
using Garage.Utils;
using UnityEngine;
using System;

namespace Garage.Structs
{
    public class CarStatus
    {
        // Each Status is PartStatus.Fine or PartStatus.Broken
        public CarStatus()
        {
            Array values = Enum.GetValues(typeof(CarParts));
            isBroken = 0; // isBroken에 부품들(CarParts) 상태를 비트마스킹
            int count = UnityEngine.Random.Range(1, 4); // 고장날 CarPart 개수

            while (count > 0)
            {
                int idx = UnityEngine.Random.Range(0, values.Length);
                if ((isBroken & (1 << idx)) == 0) // LSB부터 idx번째 isBroken이 0이면 실행
                {
                    isBroken |= 1 << idx; // (1 << idx) 에 해당하는 비트 켜기
                    count--;
                }
            }
        }

        public int isBroken; // CarParts 상태를 LSB부터 비트마스킹
    }
}