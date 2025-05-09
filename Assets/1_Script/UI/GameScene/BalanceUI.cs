using DG.Tweening;
using TMPro;
using UnityEngine;
using Garage.Utils;

namespace Garage.UI.GameScene
{
	public class BalanceUI : MonoBehaviour
	{
		[SerializeField] private GameObject addBalancePrefab;
		[SerializeField] private float tweenDuration = 0.5f;
		[SerializeField] private float expandedFontSize;
		[SerializeField] private float normalFontSize;

		private TextMeshProUGUI balanceText;
		private int currentBalance = 0;
		private int destBalance = 0;

		private void Awake()
		{
			balanceText = GetComponent<TextMeshProUGUI>();
			if (int.TryParse(balanceText.text, out var result))
			{
				destBalance = result;
			}
			else
			{
				destBalance = 0;
				balanceText.text = "0";
			}
		}

		public void SetBalance(int balance)
		{
			int diff = balance - destBalance;
			destBalance = balance;
			if (diff != 0)
			{
				ShowAddBalanceEffect(diff);
			}

			DOTween.To(() => currentBalance, x =>
			{
				currentBalance = x;
				balanceText.text = x.ToString();
			}, destBalance, tweenDuration).SetEase(Ease.OutCubic);

			Sequence fontSizeSeq = DOTween.Sequence();
			fontSizeSeq.Append(balanceText.DOFontSize(expandedFontSize, tweenDuration * 0.3f).SetEase(Ease.OutSine));
			fontSizeSeq.Append(balanceText.DOFontSize(normalFontSize, tweenDuration * 0.7f).SetEase(Ease.InSine));
		}

		private void ShowAddBalanceEffect(int diff)
		{
			GameObject instance = Instantiate(addBalancePrefab, transform);
			RectTransform rect = instance.GetComponent<RectTransform>();
			TextMeshProUGUI text = instance.GetComponent<TextMeshProUGUI>();

			if (text != null)
			{
				text.text = (diff > 0 ? "+" : "-") + diff.ToString();
			}

			rect.anchoredPosition = new Vector2(0, -30);

			Sequence fxSeq = DOTween.Sequence();
			fxSeq.Append(rect.DOAnchorPosY(-50, tweenDuration).SetEase(Ease.InOutSine));
			fxSeq.Join(text.DOFade(0, tweenDuration));
			fxSeq.OnComplete(() => Destroy(instance));
		}
	}
}