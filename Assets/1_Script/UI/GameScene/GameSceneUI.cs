using Garage.Controller;
using Garage.Structs;
using Garage.UI.GameScene.Items;
using Garage.Utils;
using Garage.Structs.CarPart;
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
        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI balanceText;

        [Header("UI Prefabs")]
        [SerializeField] private GameObject carStatusUIPrefab;

        private Dictionary<ulong, Dictionary<CarParts, CarStatusUI>> carStatusInfo = new Dictionary<ulong, Dictionary<CarParts, CarStatusUI>>();
        // fitstKey -> NetworkObjectId
        // secondKey -> (Enum)CarParts
        private bool isAnyEnlargedPart;

        private void Awake()
        {
            isAnyEnlargedPart = false;
        }

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

        // 완성되면 꺼지는거나 Tire끼웠을때도 뭐 띄워야됨
        public void ApplyProgressToUI(CarParts part, float progress, CarController car)
        {
            ulong carID = car.GetComponent<NetworkObject>().NetworkObjectId;

            carStatusInfo[carID][part].ApplyFill(progress);
        }

        // enlarge = true   =>  확대
        // enlarge = false   =>  축소
        public void TryToResizeCarPartUI(CarPartBase partBase, bool enlarge)
        {
            if (!isAnyEnlargedPart && !enlarge) return; // 이미 다 축소돼있으면 return
            if (isAnyEnlargedPart && enlarge) return;   // 이미 확대가 되어있으면 return

            CarController car = partBase.CarController;
            ulong carID = car.NetworkObjectId;
            if (!carStatusInfo.ContainsKey(carID)) return;  // key 참조 에러 방지

            CarParts part = partBase.PartType;
            if (!carStatusInfo[carID].ContainsKey(part)) return;    // key 참조 에러 방지


            carStatusInfo[carID][part].ResizeCarPartUI(enlarge);
            isAnyEnlargedPart = enlarge;
        }

        public void OnTireInserted(CarController car, CarParts tire)
        {
            ulong carID = car.GetComponent<NetworkObject>().NetworkObjectId;
            carStatusInfo[carID][tire].ChangeTireImage();
        }
	}
}