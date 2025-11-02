using Garage.Actions;
using Garage.Interfaces;
using Garage.Manager;
using Garage.Props;
using Garage.Structs.CarPart;
using Garage.Utils;
using Manager;
using Unity.Netcode;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

namespace Garage.Controller
{
	/// <summary>
	/// Interaction 관련 함수들
	/// </summary>
	public partial class PlayerController
	{
        #region InteractKey
        public void OnInteractPressed()
        {
            if (recentlyDetectedProp == null) return;

            if (currentOwningProp == null)
            {
                TryStartInteractWithProp();
            }
            else
            {
                TryEndInteractWithProp();
            }
            return;
        }

		/// <summary>
		/// Controller가 Interact를 시작하고 싶을 때 사용합니다.
		/// </summary>
		public void TryStartInteractWithProp()
		{
			if (recentlyDetectedProp == null) return;

			if (GameManagerEx.Instance.IsDay)
			{
				recentlyDetectedProp.TryInteract(NetworkManager.Singleton.LocalClientId);
			}
			else
			{
				if (recentlyDetectedProp.GetComponent<IPlaceable>() == null) return;
				recentlyDetectedProp.TryInteract(NetworkManager.Singleton.LocalClientId);
			}
		}

		/// <summary>
		/// Controller가 들고있는 Prop과의 Interact를 끊고싶을 때 사용합니다.
		/// </summary>
		public void TryEndInteractWithProp()
		{
			if (currentOwningProp == null) return;

			if (GameManagerEx.Instance.IsDay)
			{
				currentOwningProp.OnEndInteraction(transform);
				currentOwningProp = null;
				SetAnimParam((int)AnimationType.Carry, false);
			}
			else
			{
				BuildingManager.Instance.TryPlaceBuilding(currentOwningProp);
				currentOwningProp.OnEndInteraction(transform);
				currentOwningProp = null;
			}
		}
        #endregion

        private PropAction currentPropAction = null;
		private bool isActionStarted = false;
        #region ActionKeyEtc
        public void OnActionKeyStart()
        {
            if (currentOwningProp == null) return;

            // 고칠거 있으면 fix하면서 interact state로 점프
            if (currentFixablePart != null)
			{ 
				TryStartFix();
				return;
			}

            if (currentPropAction == null) return;

            // 1. Action에게 Player가 할 일을 시킴
            currentPropAction.OnStart(transform);

            // 2. Prop에게 "이제 액션 시작했으니 너도 반응해" 라고 알려줌 (콜백 호출)
            currentOwningProp.GetComponent<IActionableProp>()?.OnStartPropAction(transform);

            isActionStarted = true;
        }
        public void OnActionKeyHolding()
        {
            if (currentPropAction == null) return;
            if (currentOwningProp == null) return;
			if (!isActionStarted) return;

            // 1. Action에게 Player가 할 일을 시킴
            currentPropAction.OnHolding(transform);

			// 2. Prop에게 "액션 계속되고 있어" 라고 알려줌
			currentOwningProp.GetComponent<IActionableProp>()?.OnHoldingPropAction(transform);
        }
        // 액션 버튼에서 손을 뗐을 때
        public void OnActionKeyReleased()
        {
			if (currentPropAction == null) return;
			if (currentOwningProp == null) return;

            // 1. Action에게 Player가 할 일을 시킴
            currentPropAction.OnReleased(transform);

            // 2. Prop에게 "액션 끝났어" 라고 알려줌
            currentOwningProp.GetComponent<IActionableProp>()?.OnReleasedPropAction(transform);
			isActionStarted = false;
        }

        public void OnEndAction()
		{
			if (currentOwningProp == null) return;
			if (currentOwningProp.GetComponent<IActionableProp>() == null) return;

			switch (currentOwningProp)
            {
                case WrenchProp wr:
                    wr.GetComponent<IActionableProp>().OnReleasedPropAction(transform);
                    break;
                case TireProp _:
					SetAnimParam((int)AnimationType.Carry, false);
					SetAnimParam((int)AnimationType.Place);
					break;
            }
        }

        /// <summary>
        /// 수리를 시작할 때 호출
        /// 
        /// </summary>
        public void TryStartFix()
		{
			if (currentFixablePart == null) return;

			if (currentFixablePart.IsAbleToInteract(currentOwningProp))
			{
				if (currentOwningProp is TireProp)
				{
					SetAnimParam((int)AnimationType.Carry, false);
					SetAnimParam((int)AnimationType.Tire);
				}
				else
				{
					switch (currentFixablePart.PartType)
					{
						case CarParts.FLT:
						case CarParts.RLT:
						case CarParts.FRT:
						case CarParts.RRT:
							SetAnimLayerWeight(Constants.ANIM_LAYER_INDEX_LOWERBODY, 1f);
							SetAnimParam((int)((WrenchProp)currentOwningProp).AnimType, true);
							break;
						case CarParts.Oil:
							SetAnimParam((int)AnimationType.Oil, true);
							break;
						case CarParts.Engine:
							SetAnimParam((int)AnimationType.Hammer, true);
							break;
					}
					stateMachine.ChangeState(interactState);
				}
			}
		}
        #endregion

        /// <summary>
        /// TryInteract 후에 상호작용 가능한 경우에만 Prop쪽에서 호출됩니다.
        /// </summary>
        public void OnInteractionGranted(OwnableProp prop)
		{
			currentOwningProp = prop;
			currentPropAction = currentOwningProp.GetComponent<IActionableProp>()?.GetPropAction();

			if (currentOwningProp.GetComponent<IPlaceable>() == null)
			{
				stateMachine.ChangeState(carryState);
			}
			else
			{
				stateMachine.ChangeState(carryState);
			}
		}

		/// <summary>
		/// 스테이지 종료 시
		/// 강제로 Interaction을 끊는 함수
		/// </summary>
		public void EndAllInteraction()
		{
			stateMachine.ChangeState(idleState);
			OnEndAction();
			TryEndInteractWithProp();
			currentOwningProp = null;
			currentFixablePart = null;
		}

		private float fixablePartDistance = 1000f;
		private float interactableDistance = 1000f;
		private OwnableProp prevDetectedProp = null;
		/// <summary>
		/// Player의 forward 근처의 물체를 탐지합니다.
		/// </summary>
		public void DetectInteractableParts()
		{
			fixablePartDistance = 1000f;
			interactableDistance = 1000f;

			Vector3 boxSize = new Vector3(boxWidth, boxHeight, boxWidth);
			Vector3 boxCenter = transform.position + transform.forward * (boxSize.z / 2f + 0.5f) + new Vector3(0f, boxSize.y / 2, 0f);

			int targetLayer = Constants.LAYER_INTERACTABLE;
			int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, boxSize * 0.5f, interactableHits, transform.rotation, targetLayer);

			prevDetectedProp = recentlyDetectedProp;

			recentlyDetectedProp = null;
			currentFixablePart = null;
			currentKickableCar = null;

			for (int i = 0; i < hitCount; i++)
			{
				if (currentOwningProp != null && interactableHits[i].GetComponent<OwnableProp>() == currentOwningProp)
					continue;

				if (interactableHits[i].GetComponent<OwnableProp>() != null && !interactableHits[i].GetComponent<OwnableProp>().IsOwned())
				{
					if (transform.position.ManhatanDistance(interactableHits[i].transform.position) < fixablePartDistance)
					{
						fixablePartDistance = transform.position.ManhatanDistance(interactableHits[i].transform.position);
						recentlyDetectedProp = interactableHits[i].GetComponent<OwnableProp>();
					}
				}

				// CarParts탐지
				if (interactableHits[i].GetComponent<CarPartBase>() != null
					&& interactableHits[i].GetComponent<CarPartBase>().IsAbleToInteract(currentOwningProp))
				{
					if (transform.position.ManhatanDistance(interactableHits[i].transform.position) < fixablePartDistance)
					{
						fixablePartDistance = transform.position.ManhatanDistance(interactableHits[i].transform.position);
						currentFixablePart = interactableHits[i].GetComponent<CarPartBase>();
					}
				}

				// 찰 수 있는 차량 탐지
				if (interactableHits[i].GetComponent<CarSideDoor>() != null)
				{
					currentKickableCar = interactableHits[i].GetComponent<CarSideDoor>().Car;
				}
			}


			if (currentFixablePart == null || currentFixablePart != preEnlargedFixablePart)
			{
				UIManager.Game.TryToReducePreCarPartUI(preEnlargedFixablePart);
			}
			if (currentFixablePart != null)
			{
				UIManager.Game.TryToEnlargeCurCarPartUI(currentFixablePart);
				preEnlargedFixablePart = currentFixablePart;
			}

			recentlyDetectedProp?.OnTargetted();
			if (prevDetectedProp != recentlyDetectedProp) prevDetectedProp?.OnUntargetted();
			Debugger.DebugDrawBox(boxCenter, boxSize, transform.rotation, Color.green);
		}

		private bool shopInfoActivated = false;
		public void ActivateShopInfoUI()
		{
			shopInfoActivated = true;
        }
        public void UpdateShopInfoUIStatus()
        {
			if (!shopInfoActivated) return;
			if (recentlyDetectedProp == null)
			{
                UIManager.Game.PopupItemInfo(null);
				shopInfoActivated = false;
                return;
			}

            UIManager.Game.PopupItemInfo(recentlyDetectedProp);
        }

        public void ExtinguishFire(Vector3 position)
        {
			if (currentOwningProp == null || currentOwningProp.GetComponent<Extinguisher>() == null) return;

			Vector3 sprayEndPosition = position + transform.forward * currentOwningProp.GetComponent<Extinguisher>().ExDistance;

			int counts = Physics.OverlapCapsuleNonAlloc(position, sprayEndPosition, currentOwningProp.GetComponent<Extinguisher>().ExRadius, interactableHits, fireExLayer);

			for (int i = 0; i < counts; i++)
			{
				CarPartBase part = interactableHits[i].GetComponent<CarPartBase>();
				if (part == null) continue;

				part.Interact(this, currentOwningProp);
			}
		}

	}
}