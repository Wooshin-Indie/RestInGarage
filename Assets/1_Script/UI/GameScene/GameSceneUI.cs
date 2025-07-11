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
using UnityEngine.UI;
using Garage.UI.Item;
using Garage.Interfaces;
using Garage.Props;
using UnityEngine.Rendering;

namespace Garage.UI.GameScene
{
    public class GameSceneUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private BalanceUI balanceText;
        [SerializeField] private TimerText timerText;
        [SerializeField] private StageStartEndUI stageStartEndUI;

        [Header("PropInfoUIs")]
        [SerializeField] private PropKeyInfoUI idlePropKeyInfoUI;
        [SerializeField] private PropKeyInfoUI carryPropKeyInfoUI;
        [SerializeField] private PropKeyInfoUI interactPropKeyInfoUI;

        [Header("UI Prefabs")]
        [SerializeField] private GameObject carStatusUIPrefab;
        [SerializeField] private GameObject carCountdownUIPrefab;
        [SerializeField] private ShopInfo shopInfo;


        private Dictionary<ulong, Dictionary<CarParts, CarStatusUI>> carStatusInfo = new Dictionary<ulong, Dictionary<CarParts, CarStatusUI>>();
        // fitstKey -> NetworkObjectId
        // secondKey -> (Enum)CarParts
        private bool isAnyEnlargedPart;

        private void Awake()
        {
            isAnyEnlargedPart = false;
        }

		private void Start()
		{
            GameManagerEx.Instance.OnTimeoutAction += OnTimeout;
		}

		private void LateUpdate()
        {
            foreach(var i in carStatusInfo)
            {
                foreach(var j in i.Value)
                {
                    if (j.Value == null) continue;
                    j.Value.OnUpdate();
                }
            }
            // 여기서 carStatusInfo에 있는 CarStatusUI들 전부 Update

            curPoppedPropKeyInfoUI?.OnUpdate();
        }

        public void UpdateCarFiringUI(CarController car, float progress)
        {
            if(!carStatusInfo.TryGetValue(car.GetComponent<NetworkObject>().NetworkObjectId, out Dictionary<CarParts, CarStatusUI> dict))
            {
                Debug.LogError("car status - Init doesn't work well.");
                return;
            }
            if(dict.TryGetValue(CarParts.Fire, out CarStatusUI statusUI))
			{
				statusUI.ApplyFill(progress);
            }
            else
            {
			    CarStatusUI tmpUI = Instantiate(carStatusUIPrefab, transform).GetComponent<CarStatusUI>();
                dict.Add(CarParts.Fire, tmpUI);
			    tmpUI.InitCarStatusUI(car, CarParts.Fire);
                tmpUI.ApplyFill(progress);
            }
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

            if (!carStatusInfo.ContainsKey(carID)) return;
            if (carStatusInfo[carID].ContainsKey(carPart) && carStatusInfo[carID][carPart] != null)
            {
                Transform curUiTf = carStatusInfo[carID][carPart].transform;
                Sequence uiScaleSeq = DOTween.Sequence();
                uiScaleSeq.Append(curUiTf.DOScale(curUiTf.localScale * 1.2f, 0.1f).SetEase(Ease.OutCubic));
                uiScaleSeq.Append(curUiTf.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));
                uiScaleSeq.OnComplete(() =>
                {
                    Destroy(curUiTf.gameObject);
                });

                uiScaleSeq.Play();
            }
            else Debug.Log($"Key \"{carPart}\" is not in Dictionary");
        }
        public void RemoveAllCarStatusUI(CarController car)
        {
            ulong carID = car.GetComponent<NetworkObject>().NetworkObjectId;
            Array parts = Enum.GetValues(typeof(CarParts));

            if (!carStatusInfo.ContainsKey(carID)) return;

            foreach (CarParts part in parts)
            {
                RemoveCarStatusUI(car, part);
            }

            carStatusInfo.Remove(carID);
        }    

        public void OnBalancedChanged(int prev, int balance)
        {
            if (prev == balance) return;

            if (prev < balance)
				Managers.Sound.PlaySfx(SFXType.EarnMoney, 1f, 1f);
            else
				Managers.Sound.PlaySfx(SFXType.UseMoney, .8f, .8f);

            balanceText.SetBalance(balance);
        }
        public void OnInsufficientBalance()
        {
            balanceText.OnInsufficientMoney();
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

        public void EnlargeAllFireUIs()
        {
            foreach (var carDic in carStatusInfo)
            {
                if(carDic.Value.TryGetValue(CarParts.Fire, out CarStatusUI carUI))
                {
                    carUI.EnlargeCarPartUI();
                }
            }
        }
        public void ReduceAllFireUIs()
        {
            foreach (var carDic in carStatusInfo)
            {
                if (carDic.Value.TryGetValue(CarParts.Fire, out CarStatusUI carUI))
                {
                    carUI.ReduceCarPartUI();
                }
            }
        }
        private Dictionary<ulong, CarCountdownUI> carCountdownInfo = new();
        public void ShowCountdownUI(CarController car, float elapsedTime, float maxTime)
        {
            // Set fill amount
            ulong carID = car.NetworkObjectId;
            CarCountdownUI countUI;

			if (carCountdownInfo.TryGetValue(carID, out countUI))
            {
                countUI.SetAmount(elapsedTime / maxTime);
            }
            else
            {
				countUI = Instantiate(carCountdownUIPrefab, transform).GetComponent<CarCountdownUI>();
                carCountdownInfo[carID] = countUI;
				countUI.SetAmount(elapsedTime / maxTime);
            }

            if (countUI == null) return;

			countUI.SetPosition(Camera.main.WorldToScreenPoint(car.transform.position));
		}

		public void HideCountdownUI(CarController car)
		{
            if (carCountdownInfo.TryGetValue(car.NetworkObjectId, out CarCountdownUI countUI))
            {
                Destroy(countUI.gameObject);
				carCountdownInfo.Remove(car.NetworkObjectId);
			}
		}

		public void OnTireInserted(CarController car, CarParts tire)
        {
            ulong carID = car.GetComponent<NetworkObject>().NetworkObjectId;
            carStatusInfo[carID][tire].ChangeTireImage();
		}
        // 화면에 Shop Item 정보를 띄움
        public void PopupItemInfo(OwnableProp prop)
        {
            shopInfo.SetInfo(prop);
            shopInfo.gameObject.SetActive(true);
        }
        public void OnTimerChanged(float prevTime, float curTime)
        {
            timerText.SetTime(prevTime, curTime);
            if (Mathf.FloorToInt(prevTime) != Mathf.FloorToInt(curTime))
            {
            }
		}
        public void OnStartStage(int idx)
		{
			Managers.Sound.PlaySfx(SFXType.StartUp, .9f, 1f);
			stageStartEndUI.OnStageStart(idx);
		}
        public void OnTimeout()
		{
			Managers.Sound.PlaySfx(SFXType.Alarm, .8f, 1f);
			stageStartEndUI.OnStageTimeout();
        }
        public void OnGameOver(int idx)
        {

        }


        #region Prop Key Information UI

        private Dictionary<Type, PropKeyInfoUI> idlePropKeyDataDict = new(); // IdleState에서 프랍에 작용가능한 키 정보들
        private Dictionary<Type, PropKeyInfoUI> carryPropKeyDataDict = new();
        private Dictionary<Type, PropKeyInfoUI> interactPropKeyDataDict = new();

        private PlayerState curPropKeyInfoFor;
        /* 어떤 상태에 대한 KeyInfo가 나와야할지 선택,
         * PlayerState.Interact일 때는 interactState에 들어갔을 때의 KeyInfo가 아니라
         * interactState에 들어갈 수 있을 때(ex. fix가능한 carPart 있을 때)에
         * 대한 정보를 띄움 (-> interactPropKeyInfoUI)
         */
        private PropKeyInfoUI curPoppedPropKeyInfoUI;
        private List<KeyData> curPropKeyDataList;

        public void PopPropKeyInfoUI(PropBase prop, PlayerState propInfoFor)
        {
            PopPropKeyInfoUI(prop, prop.transform, propInfoFor);
        }
        public void PopPropKeyInfoUI(PropBase prop, Transform target, PlayerState propInfoFor)
        {
            if (prop == null)
            {
                Debug.Log("Popping Prop is null");
                return;
            }

            // 원래 켜져있는거 있으면 끄기
            if (curPoppedPropKeyInfoUI != null)
                ClosePropKeyInfoUI();

            switch (propInfoFor)
            {
                case PlayerState.Idle:
                    curPoppedPropKeyInfoUI = idlePropKeyInfoUI;
                    curPropKeyDataList = prop.ItemData.IdleKeyDataList;
                    break;
                case PlayerState.Carry:
                    curPoppedPropKeyInfoUI = carryPropKeyInfoUI;
                    if (GameManagerEx.Instance.IsDay)
                        curPropKeyDataList = prop.ItemData.CarryKeyDataList;
                    else
                        curPropKeyDataList = prop.ItemData.CarryNightKeyDataList;
                    break;
                case PlayerState.Interact:
                    curPoppedPropKeyInfoUI = interactPropKeyInfoUI;
                    curPropKeyDataList = prop.ItemData.InteractKeyDataList;
                    break;
                default:
                    Debug.LogError("Enum.PlayerState Invalid");
                    break;
            }
            curPoppedPropKeyInfoUI.SetPropKeyInfoUI(target, curPropKeyDataList);
            curPoppedPropKeyInfoUI.PopUI();
        }
        public void ClosePropKeyInfoUI()
        {
            if (curPoppedPropKeyInfoUI == null)
            {
                Debug.Log("Current Popped PropKeyInfoUI is null");
                return;
            }

            curPoppedPropKeyInfoUI.CloseUI();
            curPoppedPropKeyInfoUI = null;
        }
        #endregion
    }
}