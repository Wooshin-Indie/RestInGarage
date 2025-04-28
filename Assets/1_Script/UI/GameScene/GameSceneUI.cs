using Garage.Controller;
using Garage.Manager;
using Garage.Structs;
using Garage.UI.GameScene.Items;
using Garage.UI.LobbyScene.Items;
using Garage.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.UI.GameScene
{
    public class GameSceneUI : MonoBehaviour
    {
        [Header("UI Prefabs")]
        [SerializeField] private GameObject carStatusUIPrefab;

        private Dictionary<ulong, Dictionary<CarParts, CarStatusUI>> carStatusInfo = new Dictionary<ulong, Dictionary<CarParts, CarStatusUI>>();
        // fitstKey -> objectId
        // secondKey -> (Enum)CarParts

        private void LateUpdate()
        {
            foreach(var i in carStatusInfo)
            {
                foreach(var j in i.Value)
                {
                    j.Value.OnUpdate();
                }
            }
            // 여기서 carStatusInfo에 있는 CarStatusUI들 전부 Update
        }

        public void GenerateCarStatusUIs(CarController car, CarStatus status)
        {
            Dictionary<CarParts, CarStatusUI> carStatusUIs = new Dictionary<CarParts, CarStatusUI>();
            carStatusInfo.Add(car.MyId, carStatusUIs);

            Array values = Enum.GetValues(typeof(CarParts)); // 0 ~ (Last CarParts Value)
            foreach (CarParts v in values)
            {
                if ( ( status.isBroken & (1 << (int)v )) != 0 )
                {
                    CarStatusUI tmpUI = Instantiate(carStatusUIPrefab, transform).GetComponent<CarStatusUI>();
                    carStatusInfo[car.MyId].Add(v, tmpUI);
                    tmpUI.InitCarStatusUI(car, v);
                }
            }
        }

        public void RemoveCarStatusUI(CarController car, CarParts carPart)
        {
            if (carStatusInfo[car.MyId].ContainsKey(carPart))
                carStatusInfo[car.MyId].Remove(carPart);
            else Debug.Log($"Key \"{carPart}\" is not in Dictionary");
        }


	}
}