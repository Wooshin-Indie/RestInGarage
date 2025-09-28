using Garage.Controller;
using Garage.Manager;
using Garage.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.UI.Event
{
	public class ResultEventUI : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI testTMP;
		[SerializeField] private float resultEventDuration;
		[SerializeField] private Button okButton;
		[SerializeField] private Button nextButton;

		private List<GameResultData> tmpInfo;

		private void OnEnable()
		{
			nextButton.onClick.AddListener(() =>
			{
				StartCoroutine(SetResultInfoCoroutine());
			});
			okButton.onClick.AddListener(() =>
			{
				GameManagerEx.Instance.EndEvent();
			});

			okButton.gameObject.SetActive(false);
			nextButton.gameObject.SetActive(false);
		}

		private void OnDisable()
		{
			nextButton.onClick.RemoveAllListeners();
			okButton.onClick.RemoveAllListeners();
		}

		private int currentIndex = -1;
		public void SetResultInfo(List<GameResultData> tmpInfo)
		{
			this.tmpInfo = tmpInfo;
			currentIndex = 0;
			StartCoroutine(SetResultInfoCoroutine());
		}

		private IEnumerator SetResultInfoCoroutine()
		{
			okButton.gameObject.SetActive(false);
			nextButton.gameObject.SetActive(false);

			if (tmpInfo.Count <= currentIndex) yield break;
			testTMP.text = tmpInfo[currentIndex].TmpText;
			Camera.main.GetComponent<CameraController>().ConvertVirtualCamera(tmpInfo[currentIndex].NetId);

			yield return new WaitForSeconds(1f);

			// TODO - UI 연출 넣기

			yield return new WaitForSeconds(resultEventDuration);

			if (tmpInfo.Count - 1 == currentIndex) okButton.gameObject.SetActive(true);
			else nextButton.gameObject.SetActive(true);

			currentIndex++;
		}
	}
}
