using System.Collections.Generic;
using Garage.Manager;
using Garage.Utils;
using UnityEngine;

namespace Garage.UI
{
    /// <summary>
    /// RuntimeRecord 결과를 받아서 Result를 나타내는 UI
    /// </summary>
    public class GameResultUI : MonoBehaviour
    {
        [SerializeField] private List<RectTransform> resultCards = new();


        private List<GameplayRecordData> recordDatas;

        public void RevealGameResult()
        {
            // TODO - inputBlockPanel 키고
            // Record 결과 받아와서 UI에 출력

            recordDatas = Managers.Record.GetData(resultCards.Count);
        }

        // 해당 UI 닫는 함수
        public void OnClose()
        {
            recordDatas = null;
        }

    }
}