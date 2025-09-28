using DG.Tweening;
using Garage.Utils;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Garage.UI.Event
{
	public class EventUI : MonoBehaviour
	{
		[Header("Event Basic")]
		[SerializeField] private float bandDuration;
		[SerializeField] private RectTransform topBand;
		[SerializeField] private RectTransform bottomBand;

		[Header("ResultEvent")]
		[SerializeField] private ResultEventUI resultEventUI;

		private void Awake()
		{
			topBand.gameObject.SetActive(false);
			bottomBand.gameObject.SetActive(false);
			resultEventUI.gameObject.SetActive(false);
		}

		private void StartEventBasic()
		{
			DOTween.Kill(bottomBand);
			DOTween.Kill(topBand);

			topBand.anchoredPosition = new Vector2(0f, topBand.sizeDelta.y);
			bottomBand.anchoredPosition = new Vector2(0f, -bottomBand.sizeDelta.y);

			topBand.gameObject.SetActive(true);
			bottomBand.gameObject.SetActive(true);

			topBand.DOAnchorPosY(0f, bandDuration).SetEase(Ease.OutCirc);
			bottomBand.DOAnchorPosY(0f, bandDuration).SetEase(Ease.OutCirc);
		}

		private void EndEventBasic()
		{
			DOTween.Kill(bottomBand);
			DOTween.Kill(topBand);

			topBand.anchoredPosition = new Vector2(0f, 0f);
			bottomBand.anchoredPosition = new Vector2(0f, 0f);

			topBand.DOAnchorPosY(topBand.sizeDelta.y, bandDuration).SetEase(Ease.OutCirc);
			bottomBand.DOAnchorPosY(-bottomBand.sizeDelta.y, bandDuration).SetEase(Ease.OutCirc)
				.OnComplete(() =>
				{
					topBand.gameObject.SetActive(false);
					bottomBand.gameObject.SetActive(false);
				});
		}

		public void StartResultEvent()
		{
			StartEventBasic();

			// HACK - 원래는 RecordManager에서 받아와야됨
			List<GameResultData> tmpInfo = new List<GameResultData>()
			{
				new GameResultData(GetRandomClientId(), "EXAMPLE1"),
				new GameResultData(GetRandomClientId(), "EXAMPLE2"),
				new GameResultData(GetRandomClientId(), "EXAMPLE3"),
				new GameResultData(GetRandomClientId(), "EXAMPLE4")
			};
			resultEventUI.gameObject.SetActive(true);
			resultEventUI.SetResultInfo(tmpInfo);
		}

		public void EndResultEvent()
		{
			EndEventBasic();
			resultEventUI.gameObject.SetActive(false);
		}
		
		// HACK - 테스트용
		public ulong GetRandomClientId()
		{
			var clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();

			int randIndex = Random.Range(0, clientIds.Count);
			return clientIds[randIndex];
		}
	}
}
