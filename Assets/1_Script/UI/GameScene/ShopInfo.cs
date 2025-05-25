using DG.Tweening;
using Garage.Manager;
using Garage.Structs;
using System.Collections.Generic;
using TMPro;
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
			if (data != null && !data.IsRevealData)
				data = null;

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
				Managers.Sound.PlaySfx(SFXType.Whoosh, .85f, 1.1f);
				currentTween = rect.DOAnchorPos(outScreenPos, tweenDuration)
								   .SetEase(Ease.InOutSine);
			}
			else
			{
				Managers.Sound.PlaySfx(SFXType.Whoosh, .85f, .9f);
				currentTween = rect.DOAnchorPos(inScreenPos, tweenDuration)
								   .SetEase(Ease.OutBack);
			}
		}

		private void UpdateUI(ItemData data)
		{
			nameText.text = data.ItemName;				// 이거도 나중에 Key로 바꿔야됨
			buyPrice.text = data.BuyPrice.ToString();
			sellPrice.text = data.SellPrice.ToString();
			descriptionText.text = data.DescriptionKey; // TODO - 이거 Localization Table 참조해야됨

			for (int i = 0; i < data.ItemFeatures.Count; i++)
			{
				featureTexts[2 * i].text = data.ItemFeatures[i].FeatureName;
				featureTexts[2 * i].color = Color.white;
				featureTexts[2 * i + 1].text = (data.ItemFeatures[i].IsPositiveValue ? "+" : "- ") +
					data.ItemFeatures[i].FeatureValue + "%";
				featureTexts[2 * i + 1].color = (data.ItemFeatures[i].IsPositiveFeature ? Color.green : Color.red);
			}

			int featureCount = data.ItemFeatures.Count;
			if (data.ItemFeatures.Count == 0)
			{
				featureTexts[0].text = "None";
				featureTexts[1].text = "";
				featureCount = 1;
			}

			RebuildLayout(featureCount);
		}

		[Header("UI Elements")]
		[SerializeField] private RectTransform panelRect;
		[SerializeField] private TextMeshProUGUI nameText;
		[SerializeField] private TextMeshProUGUI buyText;
		[SerializeField] private TextMeshProUGUI buyPrice;
		[SerializeField] private TextMeshProUGUI sellText;
		[SerializeField] private TextMeshProUGUI sellPrice;
		[SerializeField] private TextMeshProUGUI descriptionTitle;
		[SerializeField] private TextMeshProUGUI descriptionText;
		[SerializeField] private TextMeshProUGUI featuresTitle;
		[SerializeField] private List<TextMeshProUGUI> featureTexts = new();

		[Header("Spacing")]
		[SerializeField] private float spacing = 10f;
		[SerializeField] private float parSpacing = 20f;

		[SerializeField] private float topPadding = 20f;
		[SerializeField] private float bottomPadding = 20f;

		public void RebuildLayout(int featureCount)
		{
			float y = -topPadding;

			SetAndMove(nameText.rectTransform, ref y, parSpacing);

			SetAndMove(buyText.rectTransform, ref y, 0f, false);
			SetAndMove(buyPrice.rectTransform, ref y, 0f, false);
			SetAndMove(sellText.rectTransform, ref y, 0f, false);
			SetAndMove(sellPrice.rectTransform, ref y, parSpacing);

			SetAndMove(descriptionTitle.rectTransform, ref y, spacing);
			SetAndMove(descriptionText.rectTransform, ref y, parSpacing);

			SetAndMove(featuresTitle.rectTransform, ref y, parSpacing);


			for (int i = 0; i<featureTexts.Count; i++)
			{
				featureTexts[i].gameObject.SetActive(i < featureCount * 2);
				if (i < featureCount * 2)
				{
					bool isEven = i % 2 == 0;
					SetAndMove(featureTexts[i].rectTransform, ref y, isEven ? 0f : spacing, !isEven);
				}
			}

			float totalHeight = -y + bottomPadding;
			panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, totalHeight);
		}

		private void SetAndMove(RectTransform rt, ref float y, float spacing = 0f, bool isMoveY = true)
		{
			var text = rt.GetComponent<TextMeshProUGUI>();
			float height = text.preferredHeight;
			rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
			rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);

			if (isMoveY)
				y -= height + spacing;
		}
	}
}
