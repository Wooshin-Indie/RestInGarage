using Garage.Manager;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Garage.UI.Item
{ 
    public class ButtonSFX : MonoBehaviour,
        IPointerEnterHandler,
		IPointerClickHandler
    {
        [SerializeField] private SFXType hoverSfx;
		[SerializeField, Range(.5f, 1.5f)] private float hoverVolume;
		[SerializeField, Range(.5f, 1.5f)] private float hoverPitch;

        [SerializeField] private SFXType pressSfx;
		[SerializeField, Range(.5f, 1.5f)] private float pressVolume;
		[SerializeField, Range(.5f, 1.5f)] private float pressPitch;

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			SoundManager.Instance.PlaySfx(pressSfx, pressVolume, pressPitch);
		}

		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
		{
			SoundManager.Instance.PlaySfx(hoverSfx, hoverVolume, hoverPitch);
		}
	}
}