using DG.Tweening;
using Garage.Manager;
using Garage.Structs;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.UI.GameScene
{
	public class TimerText : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI tmpText;
		[SerializeField] private Image background;
		private void Awake()
		{

		}

		private void OnEnable()
		{
			tmpText.text = "";
			background.fillAmount = 0f;
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
					Managers.Sound.PlaySfx(SFXType.Tick, 1f, 1f);
				}
			}
			else
			{
				tmpText.color = Color.white;
			}

			tmpText.text = (curTime < 0) ? "" : curTime.ToString();
			// HACK - MaxTime은 나중에 SO에 넣어야됨 (MapData에)
			background.fillAmount = current / 20f;
		}
	}
}
