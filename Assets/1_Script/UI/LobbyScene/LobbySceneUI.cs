using DG.Tweening;
using Garage.Manager;
using Garage.Structs;
using Garage.UI.LobbyScene.Items;
using Garage.Utils;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.UI.LobbyScene
{
    public class LobbySceneUI : MonoBehaviour
    {
        [Space(20)]
        [Header("Chating System")]
		[SerializeField] private int maxMessages = 20;
		[SerializeField] private Transform chatPanel;
        //[SerializeField] private TMP_InputField chatInputField;
        [SerializeField] private GameObject chatPrefab;

		private List<Message> messageList = new List<Message>();

		public class Message
		{
			public string text;
			public TMP_Text textObject;
		}

		private void Awake()
		{
			GameManagerEx.Instance.OnDisconnected += OnDisconnected;	
		}

		private void OnDestroy()
		{

		}

		public void OnGameStart()
		{
			
		}

		public void OnGameEnd()
		{
			
		}

		//private void Update()
		//{
		//	if (chatInputField.text != "")
		//	{
		//		if (Input.GetKeyDown(KeyCode.Return))
		//		{
		//			if (chatInputField.text == " ")
		//			{
		//				chatInputField.text = "";
		//				chatInputField.DeactivateInputField();
		//				return;
		//			}
		//			NetworkTransmission.instance.IWishToSendAChatServerRPC(chatInputField.text, NetworkManager.Singleton.LocalClientId);
		//			chatInputField.text = "";
		//		}
		//	}
		//	else
		//	{
		//		if (Input.GetKeyDown(KeyCode.Return))
		//		{
		//			chatInputField.ActivateInputField();
		//			chatInputField.text = " ";
		//		}
		//	}
		//}

		public void SendMessageToUI(string name, string text)
        {
			if (messageList.Count >= maxMessages)
			{
				Destroy(messageList[0].textObject.gameObject);
				messageList.Remove(messageList[0]);
			}
			Message newMessage = new Message();

			newMessage.text = name + ": " + text;
			GameObject newText = Instantiate(chatPrefab, chatPanel.transform);
			newMessage.textObject = newText.GetComponent<TMP_Text>();
			newMessage.textObject.text = newMessage.text;

			messageList.Add(newMessage);
		}

		private void ClearChat()
		{
			messageList.Clear();
			GameObject[] chat = GameObject.FindGameObjectsWithTag(Constants.TAG_CHAT);
			foreach (GameObject chit in chat)
			{
				Destroy(chit);
			}
		}

		public void OnDisconnected()
		{
			for (int i = 0; i < messageList.Count; i++)
			{
				Destroy(messageList[i].textObject);
			}
			messageList.Clear();
		}
	}
}