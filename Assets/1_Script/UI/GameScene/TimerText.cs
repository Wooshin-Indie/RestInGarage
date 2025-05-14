using TMPro;
using UnityEngine;

namespace Assets._1_Script.UI.GameScene
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

		public void SetTime(int time)
		{
			// TODO - 시간에 따라 째깍째깍 소리 or 글씨 색/크기 변하도록
			tmpText.text = (time <= 0) ? "" : time.ToString();
		}
	}
}
