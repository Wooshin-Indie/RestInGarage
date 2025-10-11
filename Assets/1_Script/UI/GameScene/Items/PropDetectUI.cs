using DG.Tweening;
using Garage.Interfaces;
using Garage.Props;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.UI.GameScene
{
	/// <summary>
	/// Prop Detect 시 띄우는 UI
	/// </summary>
	[RequireComponent(typeof(Image))]
	public class PropDetectUI : MonoBehaviour, IPopupUI, IWorldSpaceUI
    {
		[SerializeField] private float upOffset;
		[SerializeField] private float popupDuration;
		[SerializeField] private List<Sprite> sprites = new();

		private Image image = null;
		private bool targetActive = false;
		private OwnableProp targetProp = null;
		private Transform targetTransform = null;

		private void Awake()
		{
			image = GetComponent<Image>();
		}

		public void SetTargetProp(OwnableProp prop)
		{
			if (targetProp != prop) CloseUI();
			else return;

			targetProp = prop;
		}

		private void SetSprite()
		{
			if (image == null) return;

			int index = -1;
			switch (targetProp)
			{
				case TireRack _:
				case TireProp _:
					index = 0;
					break;
				case OilPump _:
					index = 1;
					break;
				case WrenchProp _:
					index = 2;
					break;
				case Extinguisher _:
					index = 3;
					break;
				case Barricade _:
					index = 4;
					break;
				default:
					return;
			}

			image.sprite = sprites[index];
		}

		public void PopUI()
		{
			if (targetActive) return;
			targetActive = true;
			DOTween.Kill(transform);

			targetTransform = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
			transform.localScale = Vector3.zero;
			SetSprite();
			gameObject.SetActive(true);
			transform.DOScale(Vector3.one, popupDuration).SetEase(Ease.OutCubic);
		}

		public void CloseUI()
		{
			if (!targetActive) return;
			targetActive = false;
			DOTween.Kill(transform);

			targetProp = null;
			transform.localScale = Vector3.one;
			transform.DOScale(Vector3.zero, popupDuration).SetEase(Ease.OutCubic)
				.OnComplete(() =>
				{
					gameObject.SetActive(false);
				});
		}

		public void UpdateUIScreenPos()
		{
			if (!targetActive || targetTransform == null) return;

			Vector3 screenPos = Camera.main.WorldToScreenPoint(targetTransform.position);
			screenPos.z = 0;
			screenPos.y += upOffset;

			transform.position = screenPos;
		}
	}
}