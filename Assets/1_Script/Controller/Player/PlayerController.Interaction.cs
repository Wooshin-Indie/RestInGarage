using Garage.Interfaces;
using Garage.Manager;
using Garage.Props;
using Garage.Structs.CarPart;
using Garage.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Controller
{
	/// <summary>
	/// Interaction 관련 함수들
	/// </summary>
	public partial class PlayerController
	{

		/// <summary>
		/// Controller가 Interact를 시작하고 싶을 때 사용합니다.
		/// </summary>
		public void TryStartInteract()
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
		/// Controller가 Interact를 끊고싶을때 사용합니다.
		/// </summary>
		public void TryEndInteract()
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

        /// <summary>
        /// 처음 한 번 눌릴 때의 액션 처리
        /// 들고있는 Prop의 Action을 수행합니다.
        /// ex. 타이어 -> 굴림, 소화기 -> 분사
        /// </summary>
        public void TryWasPressedThisFrameAction() // 
		{
			if (currentOwningProp == null) return;
			if (currentOwningProp.GetComponent<IActionable>() == null) return;

			switch (currentOwningProp)
			{
				case TireProp _:
				case WrenchProp _:
					ChargeTireRoll();
                    break;
				case Extinguisher ex:
					isAbleToMove = false;
					ex.GetComponent<IActionable>().OnStartPropAction(transform);
					SetAnimParam((int)AnimationType.Oil, true);
					break;
			}
		}

        /// <summary>
        /// 여러 프레임동안 눌려있을 때의 액션 처리
        /// </summary>
        public void TryIsPressedAction()
		{
            if (currentOwningProp == null) return;
            if (currentOwningProp.GetComponent<IActionable>() == null) return;

            switch (currentOwningProp)
            {
                case TireProp _:
				case WrenchProp _:
                    ChargeTireRoll();
                    break;
            }
        }

		public void TryEndAction()
		{
			if (currentOwningProp == null) return;
			if (currentOwningProp.GetComponent<IActionable>() == null) return;

			switch (currentOwningProp)
			{
				case TireProp _:
					SetAnimParam((int)AnimationType.Carry, false);
					SetAnimParam((int)AnimationType.Place);
					break;
				case Extinguisher ex:
					isAbleToMove = true;
					ex.GetComponent<IActionable>().OnStopPropAction(transform);
					SetAnimParam((int)AnimationType.Oil, false);
					break;
				case WrenchProp _:
					SetAnimParam((int)AnimationType.Throw);
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

		/// <summary>
		/// TryInteract 후에 상호작용 가능한 경우에만 Prop쪽에서 호출됩니다.
		/// </summary>
		public void OnInteractionGranted(OwnableProp prop)
		{
			currentOwningProp = prop;

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
			TryEndAction();
			TryEndInteract();
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