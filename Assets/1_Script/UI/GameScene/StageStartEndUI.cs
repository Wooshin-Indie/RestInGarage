using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Garage.UI.GameScene
{
    public class StageStartEndUI : MonoBehaviour
    {
		private RectTransform rect;

        [Header("Stage Start")]
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform stageTmp;
        [SerializeField] private RectTransform gameStartTmp;

        [Header("Stage End")]
		[SerializeField] private RectTransform timeoutTmp;


		[Header("Stage Start Params")]
		public float screenWidthMultiplier = 1.2f;
		public float enterDuration = 0.5f;
		public float slowDuration = 1.0f;
		public float exitDuration = 0.5f;

		private void Awake()
		{
			rect = GetComponent<RectTransform>();

            background.gameObject.SetActive(false);
			stageTmp.gameObject.SetActive(false);
			gameStartTmp.gameObject.SetActive(false);

			timeoutTmp.gameObject.SetActive(false);
		}

		public void OnStageStart(int stage)
        {
            stageTmp.GetComponent<TextMeshProUGUI>().text = $"Stage {stage.ToString()}";
			float screenWidth = rect.rect.width * screenWidthMultiplier;
			float offset = 100f;

			Vector2 startX = new Vector2(-screenWidth, 0);
			Vector2 centerX = Vector2.zero;
			Vector2 centerRightX = centerX + new Vector2(offset, 0);
			Vector2 centerLeftX = centerX - new Vector2(offset, 0);
			Vector2 endX = new Vector2(screenWidth, 0);

			background.anchoredPosition = startX;
			background.sizeDelta = new Vector2(screenWidth, background.sizeDelta.y);
			stageTmp.anchoredPosition = startX;
			gameStartTmp.anchoredPosition = endX;

			background.gameObject.SetActive(true);
			stageTmp.gameObject.SetActive(true);
			gameStartTmp.gameObject.SetActive(true);

			Sequence seq = DOTween.Sequence();

			seq.Append(background.DOAnchorPos(centerX, enterDuration).SetEase(Ease.OutQuad));
			seq.Join(stageTmp.DOAnchorPos(centerX, enterDuration).SetEase(Ease.OutQuad));
			seq.Join(gameStartTmp.DOAnchorPos(centerX, enterDuration).SetEase(Ease.OutQuad));

			seq.Append(background.DOAnchorPos(centerRightX, slowDuration).SetEase(Ease.Linear));
			seq.Join(stageTmp.DOAnchorPos(centerRightX, slowDuration).SetEase(Ease.Linear));
			seq.Join(gameStartTmp.DOAnchorPos(centerLeftX, slowDuration).SetEase(Ease.Linear));

			seq.Append(background.DOAnchorPos(endX, exitDuration).SetEase(Ease.InQuad));
			seq.Join(stageTmp.DOAnchorPos(endX, exitDuration).SetEase(Ease.InQuad));
			seq.Join(gameStartTmp.DOAnchorPos(startX, exitDuration).SetEase(Ease.InQuad));

			seq.OnComplete(() =>
			{
				background.gameObject.SetActive(false);
				stageTmp.gameObject.SetActive(false);
				gameStartTmp.gameObject.SetActive(false);
			});

			seq.Play();
		}

		public void OnStageTimeout()
		{
			timeoutTmp.gameObject.SetActive(true);

			Sequence seq = DOTween.Sequence();
			seq.Append(timeoutTmp.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack))
			   .Append(timeoutTmp.DOShakeScale(2f, strength: 0.15f, vibrato: 20))
			   .Append(timeoutTmp.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));

			seq.OnComplete(() =>
			{
				timeoutTmp.gameObject.SetActive(false);
			});

			seq.Play();
		}
	}
}