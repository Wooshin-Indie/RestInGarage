using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using Garage.Manager;

namespace Garage.UI.GameScene
{
	public class IngameSettingUI : MonoBehaviour
	{
		[SerializeField] private RectTransform gearRect;
		[SerializeField] private List<RectTransform> settings = new();

		[SerializeField] private float duration;
		[SerializeField] private float height;
		[SerializeField] private float padding;
		[SerializeField] private float offset;

		private bool isOpened = false;

		public void Shut()
		{
			GameNetworkManager.Instance.Disconnected();
		}

		public void Init()
		{
			Debug.Log("INIT");
			isOpened = false;
			gearRect.rotation = Quaternion.identity;

			foreach (var setting in settings)
			{
				setting.gameObject.SetActive(false);

				var cg = setting.GetOrAddComponent<CanvasGroup>();
				cg.alpha = 0;
				cg.interactable = false;
				cg.blocksRaycasts = false;

				setting.DOKill();
			}
		}

		public void Toggle()
		{
			if (isOpened) Close();
			else Open();

			isOpened = !isOpened;
		}
		private void Open()
		{
			gearRect.DORotate(new Vector3(0, 0, 90f), duration, RotateMode.Fast);

			for (int i = 0; i < settings.Count; i++)
			{
				var setting = settings[i];
				setting.gameObject.SetActive(true);

				var cg = setting.GetComponent<CanvasGroup>(); 
				if (cg == null) continue;
				cg.DOKill();

				setting.anchoredPosition = gearRect.anchoredPosition - new Vector2(0, offset);
				Vector2 targetPos = gearRect.anchoredPosition - new Vector2(0, offset + height * i + (i + 1) * padding);

				setting.DOAnchorPos(targetPos, duration).SetEase(Ease.OutQuad);
				cg.DOFade(1f, duration);
				cg.interactable = true;
				cg.blocksRaycasts = true;
			}
		}

		private void Close()
		{
			gearRect.DORotate(Vector3.zero, duration, RotateMode.Fast);

			for (int i = 0; i < settings.Count; i++)
			{
				var setting = settings[i];
				var cg = setting.GetComponent<CanvasGroup>();
				if (cg == null) continue;

				cg.DOKill();
				setting.DOAnchorPos(gearRect.anchoredPosition - new Vector2(0, offset), duration).SetEase(Ease.InQuad);
				cg.DOFade(0f, duration).OnComplete(() =>
				{
					setting.gameObject.SetActive(false);
				});

				cg.interactable = false;
				cg.blocksRaycasts = false;
			}
		}

	}
}