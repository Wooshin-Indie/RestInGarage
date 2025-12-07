using Garage.Manager;
using Garage.Props;
using Garage.Utils;
using UnityEngine;

namespace Garage.Controller
{
	/// <summary>
	/// Player의 UI 조작에 관한 스크립트
	/// </summary>
	public partial class PlayerController
	{
		public void OnUpdatePlayerUI()
		{
			UpdateSizeOfFireUIs();
			UpdatePropKeyInfoUIs();
			UpdateDetectPropUI();
		}

		private float interactPropKeyInfoUITimer = 0f;
		private float idlePropKeyInfoUITimer = 0f;
		private float propKeyInfoUIDelay = 1.5f;

		private bool isFireUIsEnlarged = false;
		private void UpdateSizeOfFireUIs()
		{
			if (currentOwningProp is not Extinguisher)
			{
				if (isFireUIsEnlarged)
				{
					isFireUIsEnlarged = false;
					UIManager.Game.ReduceAllFireUIs();
				}
				return;
			}

			isFireUIsEnlarged = true;
			UIManager.Game.EnlargeAllFireUIs();
		}
		private void UpdatePropKeyInfoUIs()
		{
			if (stateMachine.CurState == interactState)
			{
				UIManager.Game.ClosePropKeyInfoUI();
				interactPropKeyInfoUITimer = 0f;
				idlePropKeyInfoUITimer = 0f;

				return;
			}

			// interactPropKeyInfoUI condition
			if (currentFixablePart != null && currentFixablePart.IsAbleToInteract(currentOwningProp))
			{
				idlePropKeyInfoUITimer = 0f;

				interactPropKeyInfoUITimer += Time.deltaTime;
				if (interactPropKeyInfoUITimer > propKeyInfoUIDelay)
				{
					interactPropKeyInfoUITimer = 0f;
					UIManager.Game.PopPropKeyInfoUI(currentOwningProp, currentFixablePart.transform, PlayerState.Interact);
				}
				return;
			}
			// carryPropKeyInfoUI condition
			else if (currentOwningProp != null)
			{
				interactPropKeyInfoUITimer = 0f;
				idlePropKeyInfoUITimer = 0f;

				UIManager.Game.PopPropKeyInfoUI(currentOwningProp, PlayerState.Carry);
				return;
			}
			// idlePropKeyInfoUI condition
			else if (recentlyDetectedProp != null && recentlyDetectedProp == prevDetectedProp)
			{
				interactPropKeyInfoUITimer = 0f;

				idlePropKeyInfoUITimer += Time.deltaTime;
				if (idlePropKeyInfoUITimer > propKeyInfoUIDelay)
				{
					idlePropKeyInfoUITimer = 0f;
					UIManager.Game.PopPropKeyInfoUI(recentlyDetectedProp, PlayerState.Idle);
				}
				return;
			}

			UIManager.Game.ClosePropKeyInfoUI();
			interactPropKeyInfoUITimer = 0f;
			idlePropKeyInfoUITimer = 0f;
		}
		private void UpdateDetectPropUI()
		{
			if (currentOwningProp != null || recentlyDetectedProp == null) // 감지된거 있고 현재 프랍 안들고있을 때만 실행
			{
				UIManager.Game.ClosePropDetectUI();
				return;
			}

			UIManager.Game.PopPropDetectUI(recentlyDetectedProp);
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
	}
}