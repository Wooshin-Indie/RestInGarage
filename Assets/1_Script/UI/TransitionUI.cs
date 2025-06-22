using DG.Tweening;
using Garage.Manager;
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
			if(duration > Mathf.Epsilon)
			{
				Managers.Sound.PlaySfx(SFXType.SlideUp, .7f, 1f);
			}

			Managers.Sound.BlockBGM(1f);
			backgroundImage.material.SetFloat("_Cutoff", 1f);
			backgroundImage.gameObject.SetActive(true);
			backgroundImage.material.DOFloat(0f, "_Cutoff", duration);
		}

		public void EndTransition(float waitDuration, float fadeDuration)
		{
			if (!backgroundImage.gameObject.activeSelf)
				return;
			Debug.Log("EndTransition");
            backgroundImage.material.
				DOFloat(1f, "_Cutoff", fadeDuration)
				.OnStart(() =>
				{
					Managers.Sound.ReleaseBGM(1f);
					Managers.Sound.PlaySfx(SFXType.SlideDown, .7f, 1f);
				})
				.SetDelay(waitDuration)
				.OnComplete(() =>
				{
					backgroundImage.gameObject.SetActive(false);
				});
		}
	}
}
