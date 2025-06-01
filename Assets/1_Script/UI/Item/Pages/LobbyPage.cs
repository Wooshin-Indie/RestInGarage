using System.Collections.Generic;
using Steamworks.Data;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Garage.UI.Item
{
	public class LobbyPage : PageUI
	{
		[SerializeField] private Transform itemParent;
		[SerializeField] private GameObject listItemPrefab;
		[SerializeField] private Button backButton;

		private List<GameObject> itemList = new();

		protected override void OnEnable()
		{
			base.OnEnable(); // 버튼이 아예 없을수도 있음
		}

		public void StartLoading()
		{
			Debug.Log("Start Loading");
		}

		public void OnRevealLobbyData(Lobby[] lobbies)
		{
			for (int i = 0; i < lobbies.Length; i++)
			{
				Lobby lobby = lobbies[i];
				GameObject go = Instantiate(listItemPrefab, itemParent);
				go.GetComponent<LobbyListItem>().SetLobbyInfo(lobby);
				itemList.Add(go);
			}

			int count = itemList.Count;
			for (int i = 0; i < count; i++)
			{
				Button itemButton = itemList[i].GetComponent<Button>();
				Navigation nav = itemButton.navigation;
				nav.mode = Navigation.Mode.Explicit;

				nav.selectOnDown = (i+1 == count) ? backButton : itemList[(i + 1)%count].GetComponent<Button>();
				nav.selectOnUp = (i == 0) ? backButton : itemList[(i - 1 + count) % count].GetComponent<Button>();

				itemButton.navigation = nav;
				if (itemButton.transform.childCount > 0 && itemList[i].transform.GetChild(0)?.GetComponent<TextMeshProUGUI>() != null)
				{
					itemButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = UnityEngine.Color.white;
				}
			}

			if (count == 0) return;
			Navigation backNav = backButton.navigation;
			backNav.mode = Navigation.Mode.Explicit;
			backNav.selectOnDown = itemList[0].GetComponent<Button>();
			backNav.selectOnUp = itemList[^1].GetComponent<Button>();
			backButton.navigation = backNav;

			EventSystem.current.SetSelectedGameObject(itemList[0]);
		}

		protected override void Update()
		{
			base.Update();

			if (EventSystem.current.currentSelectedGameObject != backButton.gameObject)
			{
				var selected = EventSystem.current.currentSelectedGameObject;
				if (selected != null && itemList.Contains(selected))
				{
					CenterOnItem(selected);
				}
			}
		}

		public void OnDisable()
		{
			for(int i=0; i < itemList.Count; i++)
			{
				Destroy(itemList[i]);
			}
			itemList.Clear();
		}

		private void CenterOnItem(GameObject targetItem)
		{
			if (targetItem == null) return;

			RectTransform content = itemParent as RectTransform;
			RectTransform viewport = content.parent as RectTransform;
			RectTransform target = targetItem.transform as RectTransform;

			float itemY = target.anchoredPosition.y;
			float halfViewportHeight = viewport.rect.height / 2f;
			float halfItemHeight = target.rect.height / 2f;
			float targetContentY = -itemY - halfViewportHeight + halfItemHeight;

			Vector2 anchoredPos = content.anchoredPosition;
			anchoredPos.y = targetContentY;
			content.anchoredPosition = anchoredPos;
		}
	}
}
