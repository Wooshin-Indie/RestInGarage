using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.UI
{
	public class TransitionUI : MonoBehaviour
	{
		[SerializeField] private Image backgroundImage;

		public void Awake()
		{
			backgroundImage.gameObject.SetActive(false);
		}

		void Start()
		{
			float aspect = (float)Screen.width / Screen.height;
			backgroundImage.material.SetFloat("_ScreenAspect", aspect);
		}

		public void StartTransition(float duration)
		{
			backgroundImage.material.SetFloat("_Cutoff", 1f);
			backgroundImage.gameObject.SetActive(true);
			backgroundImage.material.DOFloat(0f, "_Cutoff", duration);
		}

		public void EndTransition(float waitDuration, float fadeDuration)
		{
			backgroundImage.material
				.DOFloat(1f, "_Cutoff", fadeDuration)
				.SetDelay(waitDuration)
				.OnComplete(() =>
				{
					backgroundImage.gameObject.SetActive(false);
				});
		}
	}
}
