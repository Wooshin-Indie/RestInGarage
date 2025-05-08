using DG.Tweening;
using Garage.Structs;
using UnityEngine;

namespace Garage.UI.GameScene
{
	public class ShopInfo : MonoBehaviour
	{
		private RectTransform rect;

		[SerializeField] private Vector2 inScreenPos;
		[SerializeField] private Vector2 outScreenPos;
		[SerializeField] private float tweenDuration = 0.5f;

		private ItemData currentData = null;
		private Tween currentTween;

		private void Awake()
		{
			rect = GetComponent<RectTransform>();	
		}

		public void SetInfo(ItemData data)
		{
			if (data == currentData)
				return;

			bool wasNull = currentData == null;
			bool isNull = data == null;
			currentData = data;

			if (wasNull == isNull)
			{
				if (!isNull)
					UpdateUI(data);
				return;
			}

			currentTween?.Kill();

			if (data != null)
			{
				UpdateUI(data);
				currentTween = rect.DOAnchorPos(outScreenPos, tweenDuration)
								   .SetEase(Ease.InOutSine);
			}
			else
			{
				currentTween = rect.DOAnchorPos(inScreenPos, tweenDuration)
								   .SetEase(Ease.OutBack);
			}
		}

		private void UpdateUI(ItemData data)
		{
			// TODO - 여기에 UI 내용 갱신 코드 작성
			// ex. 
		}
	}
}
