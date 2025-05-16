using Garage.Controller;
using Garage.Structs;
using Garage.UI.GameScene.Items;
using Garage.Utils;
using Garage.Structs.CarPart;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;
using Garage.Manager;
using Garage.UI.Item;

namespace Garage.UI.GameScene
{
    public class GameSceneUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private BalanceUI balanceText;
        [SerializeField] private TimerText timerText;
        [SerializeField] private StageStartEndUI stageStartEndUI;

        [Header("UI Prefabs")]
        [SerializeField] private GameObject carStatusUIPrefab;
        [SerializeField] private ShopInfo shopInfo;


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
                Transform curUiTf = carStatusInfo[carID][carPart].transform;
                Sequence uiScaleSeq = DOTween.Sequence();
                uiScaleSeq.Append(curUiTf.DOScale(curUiTf.localScale * 1.2f, 0.1f).SetEase(Ease.OutCubic));
                uiScaleSeq.Append(curUiTf.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));
                uiScaleSeq.OnComplete(() =>
                {
                    Destroy(carStatusInfo[carID][carPart].gameObject);
                    carStatusInfo[carID].Remove(carPart);
                });

                uiScaleSeq.Play();
            }
            else Debug.Log($"Key \"{carPart}\" is not in Dictionary");
        }

        public void OnBalancedChanged(int prev, int balance)
        {
            if (prev == balance) return;

            if (prev < balance)
                SoundManager.Instance.PlaySfx(SFXType.EarnMoney, 1f, 1f);
            else
				SoundManager.Instance.PlaySfx(SFXType.UseMoney, .8f, .8f);

            balanceText.SetBalance(balance);
        }

        // 완성되면 꺼지는거나 Tire끼웠을때도 뭐 띄워야됨
        public void ApplyProgressToUI(CarParts part, float progress, CarController car)
        {
            ulong carID = car.GetComponent<NetworkObject>().NetworkObjectId;

            carStatusInfo[carID][part].ApplyFill(progress);
        }

        // enlarge = true   =>  확대
        // enlarge = false   =>  축소
        private CarPartBase curEnlaredPartBase = null;
        public void TryToEnlargeCurCarPartUI(CarPartBase curPartBase)
        {
            if (curEnlaredPartBase == curPartBase) return; // 이미 확대된 part 확대하려하면 return

            CarParts part = curPartBase.PartType;
            CarController car = curPartBase.CarController;
            ulong carID = car.NetworkObjectId;

            if (!carStatusInfo.ContainsKey(carID)) return;  // key 참조 에러 방지
            if (!carStatusInfo[carID].ContainsKey(part)) return;    // key 참조 에러 방지

            carStatusInfo[carID][part].EnlargeCarPartUI();
            curEnlaredPartBase = curPartBase;
        }
        public void TryToReducePreCarPartUI(CarPartBase prePartBase)
        {
            if (curEnlaredPartBase != prePartBase) return;   // 이미 확대된 파트가 아닌 파트 축소하려하면 return
            if (prePartBase == null) return;   // prePartBase가 null이면 return  : 이쪽 좀더 자세히 분석 필요

            CarParts part = prePartBase.PartType;
            CarController car = prePartBase.CarController;
            ulong carID = car.NetworkObjectId;

            if (!carStatusInfo.ContainsKey(carID)) return;  // key 참조 에러 방지
            if (!carStatusInfo[carID].ContainsKey(part)) return;    // key 참조 에러 방지
            if (!car.CarStatus.IsBroken(part)) return;  // 다 고쳐져서 사라질거는 축소 안되게

            carStatusInfo[carID][part].ReduceCarPartUI();
            curEnlaredPartBase = null;
        }

        public void OnTireInserted(CarController car, CarParts tire)
        {
            ulong carID = car.GetComponent<NetworkObject>().NetworkObjectId;
            carStatusInfo[carID][tire].ChangeTireImage();
		}

        // 화면에 Shop Item 정보를 띄움
        public void PopupItemInfo(ItemData data)
        {
            shopInfo.SetInfo(data);
            shopInfo.gameObject.SetActive(true);
        }

        [SerializeField] private GameObject priceTextPrefab;
        private Dictionary<ulong, ItemPriceText> priceTexts = new();

        public void RevealItemPrice(Vector3 pos, ulong netId, int price)
		{
            EraseItemPrice(netId);

			priceTexts[netId] = Instantiate(priceTextPrefab, transform).GetComponent<ItemPriceText>();
			priceTexts[netId].SetItemPrice(pos, price);
		}

        public void EraseItemPrice(ulong netId)
		{
			if (priceTexts.ContainsKey(netId))
			{
				if (priceTexts[netId] != null) Destroy(priceTexts[netId].gameObject);
			}
		}

        public void EraseAllItemPrice()
        {
            foreach(var item in priceTexts.Values)
            {
                if(item != null) Destroy(item.gameObject); 
            }
            priceTexts.Clear();
        }

        public void OnTimerChanged(float prevTime, float curTime)
        {
            if (Mathf.FloorToInt(prevTime) != Mathf.FloorToInt(curTime))
            {
                timerText.SetTime(Mathf.FloorToInt(curTime));
            }
		}

        public void OnStartStage(int idx)
		{
			SoundManager.Instance.PlaySfx(SFXType.StartUp, .9f, 1f);
			stageStartEndUI.OnStageStart(idx);
		}

        public void OnTimeout()
		{
			SoundManager.Instance.PlaySfx(SFXType.Alarm, .8f, 1f);
			stageStartEndUI.OnStageTimeout();
        }

        public void OnGameOver(int idx)
        {

        }
	}
}