using Garage.Manager;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Garage.UI.Item
{
	public class PageUI : MonoBehaviour
	{
		[SerializeField] protected RectTransform arrowUI;
		[SerializeField] protected List<Selectable> buttons = new();

		void Start()
		{
			int count = buttons.Count;
			for (int i = 0; i < count; i++)
			{
				Navigation nav = buttons[i].navigation;
				nav.mode = Navigation.Mode.Explicit;

				nav.selectOnDown = buttons[(i + 1) % count];
				nav.selectOnUp = buttons[(i - 1 + count) % count];

				buttons[i].navigation = nav;
				if(buttons[i].transform.childCount > 0 && buttons[i].transform.GetChild(0)?.GetComponent<TextMeshProUGUI>() != null){
					buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
				}
			}
		}

		protected virtual void OnEnable()
		{
			EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
			targetPosX = buttons[0].gameObject.transform.position.x - ((RectTransform)buttons[0].gameObject.transform).rect.width/2;
			targetPosY = buttons[0].gameObject.transform.position.y;

			arrowUI.transform.position = new Vector3(targetPosX, targetPosY, 0f);
		}

		private GameObject prevSelectedObject = null;
		private RectTransform rect;
		private float targetPosY;
		private float targetPosX;

		private void Update()
		{
			if (prevSelectedObject != null && EventSystem.current.currentSelectedGameObject != prevSelectedObject)
			{
				if (prevSelectedObject.transform.childCount > 0 && prevSelectedObject.transform.GetChild(0)?.GetComponent<TextMeshProUGUI>() != null)
				{
					prevSelectedObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
				}
			}

			if(Managers.Input.Control.UI.Up.WasPressedThisFrame() ||
				Managers.Input.Control.UI.Down.WasPressedThisFrame())
			{
				if (EventSystem.current.currentSelectedGameObject == null)
				{
					EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
				}
			}

			if (EventSystem.current.currentSelectedGameObject != null)
			{
				if (!arrowUI.gameObject.activeSelf) arrowUI.gameObject.SetActive(true);
				if (rect == null || rect.gameObject != EventSystem.current.currentSelectedGameObject)
					rect = EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>();

				if (rect.childCount > 0 && rect.GetChild(0).GetComponent<TextMeshProUGUI>() != null)
				{
					rect.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
				}

				targetPosX = rect.position.x - rect.rect.width/2;
				targetPosY = rect.position.y;
			}
			else
			{
				arrowUI.gameObject.SetActive(false);
			}

			arrowUI.transform.position = new Vector3(targetPosX, Mathf.Lerp(arrowUI.transform.position.y, targetPosY, .2f), 0f);
			prevSelectedObject = EventSystem.current.currentSelectedGameObject;
		}
	}
}
