using Garage.Actions;
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
	/// Interaction, Action, Detect 등 다른 물체와의 상호작용에 관한 스크립트
	/// </summary>
	public partial class PlayerController
	{
        #region Interacts
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
				currentPropAction = null;
				SetAnimParam((int)AnimationType.Carry, false);
			}
			else
			{
				BuildingManager.Instance.TryPlaceBuilding(currentOwningProp);
				currentOwningProp.OnEndInteraction(transform);
				currentOwningProp = null;
                currentPropAction = null;
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
							if (currentOwningProp is HammerProp)
                                SetAnimParam((int)AnimationType.HammerRepair, true);
							else
								SetAnimParam((int)AnimationType.WrenchRepair, true);
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
			OnActionKeyReleased();
			TryEndInteractWithProp();
			currentFixablePart = null;
		}
		#endregion

		#region Actions

		private ActionBase currentPropAction = null;
		public ActionBase CurrentPropAction => currentPropAction;

		private bool isActionKeyReleased = false;

		public void GetActionInput()
		{
			if (currentOwningProp == null) return;

			for (int i = 0; i < currentOwningProp.PropActions.Count; i++)
			{
				if (currentOwningProp.PropActions[i].GetActionIA().WasPressedThisFrame())
				{
					currentPropAction = currentOwningProp.PropActions[i];
					stateMachine.ChangeState(actionState);
				}
			}
		}
		public void OnActionKeyStart()
		{
			if (currentPropAction == null || currentOwningProp == null)
			{
				Debug.LogError("PlayerContorller.Interaction - Prop/PropAction is null");
				return;
			}

			isActionKeyReleased = false;
			currentPropAction.OnStart(currentOwningProp);
		}
        public void OnActionKeyHolding()
		{
			if (currentPropAction == null || currentOwningProp == null)
			{
				Debug.LogError("PlayerContorller.Interaction - Prop/PropAction is null");
				return;
			}
			if (isActionKeyReleased) return;

			if (currentPropAction.GetActionIA().IsPressed())
			{
				currentPropAction.OnHolding(currentOwningProp);
			}

			if (currentPropAction.IsAbleToCancel)
			{
				if (currentPropAction.GetCancelIA().WasPressedThisFrame())
				{
					currentPropAction.OnCanceled(currentOwningProp);
					stateMachine.ChangeState(carryState);
				}
			}

			if (currentPropAction.GetActionIA().WasReleasedThisFrame())
				OnActionKeyReleased();
		}
        public void OnActionKeyReleased()
        {
            if (currentPropAction == null || currentOwningProp == null)
			{
				Debug.LogError("PlayerContorller.Interaction - Prop/PropAction is null");
				return;
			}
			isActionKeyReleased = true;

			ActionEndCondition cond = currentPropAction.EndCondition;
			currentPropAction.OnReleased(currentOwningProp);

			if (cond == ActionEndCondition.OnKeyUp)
			{
				stateMachine.ChangeState(carryState);
			}
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
		public void KickCar()
		{
			if (currentKickableCar == null) return;
			// HACK - if (currentKickableCar.CarStatus.IsThereAnyBroken()) return;

			Managers.Input.DisablePlayerInputs();
			SetAnimParam((int)AnimationType.Kick);
		}
		#endregion

		#region Detects
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

		private CarController curTransparentCar = null;
		private CarController tmpRaycastedCar = null;
		public void DetectFrontCarAndMakeTransparent()
		{
			int count = Physics.RaycastNonAlloc(transform.position, -camDir, hits, 3f, transparentLayer);
			Debug.DrawRay(transform.position, -camDir * 3f, Color.red);

			tmpRaycastedCar = null; // raycast 된거 없을 때 처리
			for (int i = 0; i < count; i++)
			{
				tmpRaycastedCar = hits[i].transform.GetComponent<CarController>();

				if (tmpRaycastedCar != null)
				{
					break;
				}
			}

			if (curTransparentCar == tmpRaycastedCar)
			{
				return;
			}

			if (curTransparentCar == null && tmpRaycastedCar != null) // 원래 투명화된 차량 없었을 때
			{
				curTransparentCar = tmpRaycastedCar;
				curTransparentCar.MakeCarBodyTransparent(); // 차량 투명화 함수 실행
				Debug.Log("새로 투명화");
			}
			else if (curTransparentCar != null && tmpRaycastedCar == null) // 투명화된 차량 있는데 밖으로 벗어났을 때
			{
				curTransparentCar.RestoreCarBodyTransparency(); // 차량 복원 함수 실행
				curTransparentCar = null;
				Debug.Log("차량 복원");
			}
			else // 투명화된 차량이 있는데 새로운 차량이 raycast 됐을 때
			{
				curTransparentCar.RestoreCarBodyTransparency(); // 차량 복원 함수 실행
				curTransparentCar = tmpRaycastedCar;
				curTransparentCar.MakeCarBodyTransparent(); // 차량 투명화 함수 실행
				Debug.Log("기존 차량 복원 및 새로 투명화");
			}
		}
		#endregion
	}
}