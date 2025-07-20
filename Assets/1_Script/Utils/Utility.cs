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
    }
}