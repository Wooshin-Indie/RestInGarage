using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Garage.Utils
{
    public static class Utility
    {
        /// <summary>
        /// 인자로 넣은 숫자의 확률만큼 true 반환
        /// </summary>
        public static bool Chance(float probability)
        {
            return UnityEngine.Random.value < probability;
        }

        /// <summary>
        /// [min, max] 범위의 정수에서 중복 없이 무작위로 count갯수만큼 선택
        /// </summary>
        public static List<int> GetUniqueRandomsByShuffle(int min, int max, int count)
        {
            if (count == 0) return null;
            // 1. 모든 숫자가 담긴 리스트 생성
            List<int> allNumbers = Enumerable.Range(min, max - min + 1).ToList();

            // 2. 리스트를 무작위로
            // 3. 앞에서부터 'count'개 만큼 가져오기
            return allNumbers.OrderBy(x => System.Guid.NewGuid()).Take(count).ToList();
        }
    }
}