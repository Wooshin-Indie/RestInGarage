using System.Collections.Generic;
using Steamworks.Data;
using UnityEngine;

namespace Garage.UI.Item
{
	public class LobbyPage : PageUI
	{
		[SerializeField] private Transform itemParent;
		[SerializeField] private GameObject listItemPrefab;

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
				GameObject go = Instantiate(listItemPrefab, itemParent);
				go.GetComponent<LobbyListItem>().SetLobbyInfo(lobbies[i]);
				itemList.Add(go);
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
	}
}
