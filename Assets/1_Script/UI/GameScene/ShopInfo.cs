using DG.Tweening;
using Garage.Manager;
using Garage.Props;
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

		private OwnableProp currentData = null;
		private Tween currentTween;

		private void Awake()
		{
			rect = GetComponent<RectTransform>();	
		}

		public void SetInfo(OwnableProp prop)
		{
			if (prop != null && !prop.ItemData.IsRevealData)
				prop = null;

			if (prop == currentData)
				return;

			bool wasNull = currentData == null;
			bool isNull = prop == null;
			currentData = prop;

			if (wasNull == isNull)
			{
				if (!isNull)
					UpdateUI(prop);
				return;
			}

			currentTween?.Kill();

			if (prop != null)
			{
				UpdateUI(prop);
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

		private void UpdateUI(OwnableProp prop)
		{
			ItemData data = prop.ItemData;
			List<ItemFeature> features = prop.ItemData.GetItemFeatures(prop.UpgradeLevel);
			List<StringFeature> stringFeatures = prop.ItemData.StringFeatures;

			nameText.text = data.ItemName + (prop.UpgradeLevel != 0 ? " +" + prop.UpgradeLevel.ToString() : "");				// 이거도 나중에 Key로 바꿔야됨
			buyPrice.text = data.GetBuyPrice(prop.UpgradeLevel).ToString();
			sellPrice.text = data.GetSellPrice(prop.UpgradeLevel).ToString();
			descriptionText.text = data.DescriptionKey; // TODO - 이거 Localization Table 참조해야됨

			for (int i = 0; i < features.Count; i++)
			{
				featureTexts[2 * i].text = features[i].FeatureName;
				featureTexts[2 * i].color = Color.white;
				featureTexts[2 * i + 1].text = (features[i].IsPositiveValue ? "+" : "- ") +
					features[i].FeatureValue + "%";
				featureTexts[2 * i + 1].color = (features[i].IsPositiveFeature ? Color.green : Color.red);
			}

			for (int i = 0; i < stringFeatures.Count; i++) 
			{
				stringFeatureTexts[i].text = $"- {stringFeatures[i].FeatureName}";
				stringFeatureTexts[i].color = (stringFeatures[i].IsPositiveFeature ? Color.green : Color.red);
			}

			int featureCount = features.Count;
			int stringFeatureCount = stringFeatures.Count;
			if (features.Count == 0)
			{
				featureTexts[0].text = "None";
				featureTexts[1].text = "";
				featureCount = 1;
			}

			RebuildLayout(featureCount, stringFeatureCount);
		}

		[Header("UI Elements")]
		[SerializeField] private RectTransform panelRect;
		[SerializeField] private TextMeshProUGUI nameText;
		[SerializeField] private TextMeshProUGUI buyText;
		[SerializeField] private TextMeshProUGUI buyPrice;
		[SerializeField] private TextMeshProUGUI sellText;
		[SerializeField] private TextMeshProUGUI sellPrice;
		[SerializeField] private TextMeshProUGUI descriptionText;
		[SerializeField] private List<TextMeshProUGUI> featureTexts = new();
		[SerializeField] private List<TextMeshProUGUI> stringFeatureTexts = new();

		[Header("Spacing")]
		[SerializeField] private float spacing = 10f;
		[SerializeField] private float parSpacing = 20f;

		[SerializeField] private float topPadding = 20f;
		[SerializeField] private float bottomPadding = 20f;

		public void RebuildLayout(int featureCount, int stringFeatureCount)
		{
			float y = -topPadding;

			SetAndMove(nameText.rectTransform, ref y, parSpacing);

			SetAndMove(buyText.rectTransform, ref y, 0f, false);
			SetAndMove(buyPrice.rectTransform, ref y, 0f, false);
			SetAndMove(sellText.rectTransform, ref y, 0f, false);
			SetAndMove(sellPrice.rectTransform, ref y, parSpacing);

			SetAndMove(descriptionText.rectTransform, ref y, parSpacing);

			for (int i = 0; i<featureTexts.Count; i++)
			{
				featureTexts[i].gameObject.SetActive(i < featureCount * 2);
				if (i < featureCount * 2)
				{
					bool isEven = i % 2 == 0;
					SetAndMove(featureTexts[i].rectTransform, ref y, isEven ? 0f : spacing, !isEven);
				}
			}

			SetSpace(ref y, parSpacing);
			for (int i = 0; i < stringFeatureTexts.Count; i++)
			{
				stringFeatureTexts[i].gameObject.SetActive(i < stringFeatureCount);
				if (i < stringFeatureCount)
				{
					SetAndMove(stringFeatureTexts[i].rectTransform, ref y, spacing, true);
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
			{
				y -= height + spacing;
				Debug.Log("Y IS :" + y);
			}
		}

		private void SetSpace(ref float y, float spacing)
		{
			y -= spacing;
		}
	}
}
