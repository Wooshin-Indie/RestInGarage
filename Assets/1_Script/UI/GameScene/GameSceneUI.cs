using Garage.Controller;
using Garage.Structs;
using Garage.UI.GameScene.Items;
using Garage.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Garage.UI.GameScene
{
    public class GameSceneUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI balanceText;

        [Header("UI Prefabs")]
        [SerializeField] private GameObject carStatusUIPrefab;


        private Dictionary<ulong, Dictionary<CarParts, CarStatusUI>> carStatusInfo = new Dictionary<ulong, Dictionary<CarParts, CarStatusUI>>();
        // fitstKey -> NetworkObjectId
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
            ulong carID = car.GetComponent<NetworkObject>().NetworkObjectId;

            Dictionary<CarParts, CarStatusUI> carStatusUIs = new Dictionary<CarParts, CarStatusUI>();
            carStatusInfo.Add(carID, carStatusUIs);


            Array values = Enum.GetValues(typeof(CarParts)); // 0 ~ (Last CarParts Value)
            foreach (CarParts v in values)
            {
                if ( ( status.isBroken & (1 << (int)v )) != 0 )
                {
                    CarStatusUI tmpUI = Instantiate(carStatusUIPrefab, transform).GetComponent<CarStatusUI>();
                    carStatusInfo[carID].Add(v, tmpUI);
                    tmpUI.InitCarStatusUI(car, v);
                }
            }
        }

        public void RemoveCarStatusUI(CarController car, CarParts carPart)
        { 
            ulong carID = car.GetComponent<NetworkObject>().NetworkObjectId;

            if (carStatusInfo[carID].ContainsKey(carPart))
            {
                Destroy(carStatusInfo[carID][carPart].gameObject);
                carStatusInfo[carID].Remove(carPart);
            }
            else Debug.Log($"Key \"{carPart}\" is not in Dictionary");
        }

        public void OnBalancedChanged(int prev, int balance)
        {
            balanceText.text = balance.ToString();
        }
	}
}