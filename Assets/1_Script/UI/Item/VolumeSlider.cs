using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.UI.Item
{
	[RequireComponent(typeof(Slider))]
	public class VolumeSlider : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI valueTmp;

		private Slider slider;

		private void Awake()
		{
			slider = GetComponent<Slider>();
			slider.onValueChanged.AddListener(OnValueChanged);
		}

		public void OnValueChanged(float value)
		{
			valueTmp.text = value.ToString("F2");
		}
	}
}