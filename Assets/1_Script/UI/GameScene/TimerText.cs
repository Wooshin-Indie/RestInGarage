using DG.Tweening;
using Garage.Manager;
using TMPro;
using UnityEngine;

namespace Garage.UI.GameScene
{
	public class TimerText : MonoBehaviour
	{
		private TextMeshProUGUI tmpText;

		private void Awake()
		{
			tmpText = GetComponent<TextMeshProUGUI>();
		}

		private void OnEnable()
		{
			tmpText.text = "";
		}

		public void SetTime(float prev, float current)
		{
			int prevTime = Mathf.FloorToInt(prev);
			int curTime = Mathf.FloorToInt(current);

			if (curTime <= 5 && curTime >= 0)
			{
				tmpText.color = Color.red;
				if(curTime != prevTime)
				{
					transform.DOShakePosition(.2f, 20f, 30);
					SoundManager.Instance.PlaySfx(SFXType.Tick, 1f, 1f);
				}
			}
			else
			{
				tmpText.color = Color.white;
			}

			tmpText.text = (curTime < 0) ? "" : curTime.ToString();
		}
	}
}
