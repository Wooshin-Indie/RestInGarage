using Garage.Manager;
using IUtil;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Garage.UI.Item
{

	public abstract class OptionSelector : MonoBehaviour
	{
		[SerializeField] protected TextMeshProUGUI optionLabel;
		[SerializeField] private Button leftButton;
		[SerializeField] private Button rightButton;
		[SerializeField, ReadOnly] protected int currentIndex = 0;

		private void Awake()
		{
			GetComponent<Button>().onClick.AddListener(() => { ApplySetting(); });
			leftButton.onClick.AddListener(() => { OnLeftButton(); });
			rightButton.onClick.AddListener(() => { OnRightButton(); });
		}

		void Update()
		{
			if (EventSystem.current.currentSelectedGameObject != gameObject) return;

			if (Managers.Input.Control.UI.Left.WasPressedThisFrame())
			{
				OnLeftButton();
			}

			if (Managers.Input.Control.UI.Right.WasPressedThisFrame())
			{
				OnRightButton();
			}
		}

		public abstract void ApplySetting();
		public abstract void SetUIAsCurrentSetting();
		protected abstract void OnLeftButton();
		protected abstract void OnRightButton();
		protected abstract void UpdateLabel();

	}
}