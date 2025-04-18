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
        [Header("Buttons")]
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button notreadyButton;
        [SerializeField] private Button startButton;

        [Space(20)]
        [Header("Chating System")]
		[SerializeField] private int maxMessages = 20;
		[SerializeField] private Transform chatPanel;
        [SerializeField] private TMP_InputField chatInputField;
        [SerializeField] private GameObject chatPrefab;

        [Space(20)]
        [Header("Player Cards")]
        [SerializeField] private Transform playerFieldBox;
        [SerializeField] private GameObject playerCardPrefab;


		private List<Message> messageList = new List<Message>();
		private List<PlayerCard> cardList = new List<PlayerCard>();

		public class Message
		{
			public string text;
			public TMP_Text textObject;
		}

		private void Start()
		{

			disconnectButton.onClick.AddListener(GameNetworkManager.Instance.Disconnected);
			readyButton.onClick.AddListener(() => {
			}); 
		}

		private void OnDestroy()
		{

		}


		private void Update()
		{
			if (chatInputField.text != "")
			{
				if (Input.GetKeyDown(KeyCode.Return))
				{
					if (chatInputField.text == " ")
					{
						chatInputField.text = "";
						chatInputField.DeactivateInputField();
						return;
					}
					NetworkTransmission.instance.IWishToSendAChatServerRPC(chatInputField.text, NetworkManager.Singleton.LocalClientId);
					chatInputField.text = "";
				}
			}
			else
			{
				if (Input.GetKeyDown(KeyCode.Return))
				{
					chatInputField.ActivateInputField();
					chatInputField.text = " ";
				}
			}
		}

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

		public void OnAddPlayerToDictionary(PlayerInfo pi)
		{
			PlayerCard pc = Instantiate(playerCardPrefab, playerFieldBox).GetComponent<PlayerCard>();
			pc.SetPlayerCard(pi);
			cardList.Add(pc);
		}

		public void OnRemovePlayerFromDictionary(PlayerInfo pi)
		{
			for(int i=cardList.Count-1; i>=0; i--)
			{
				if (cardList[i].steamId == pi.steamId)
				{
					Destroy(cardList[i].gameObject);
					cardList.RemoveAt(i);
				}
			}
		}

		public void OnUpdatePlayerReady(bool isReady, ulong steamId)
		{
			bool isAllReady = true;
			foreach (PlayerCard card in cardList)
			{
				if (card.steamId == steamId)
				{
					card.readyImage.SetActive(isReady);
				}

				if (!card.readyImage.activeSelf) isAllReady = false;
			}

			if (SteamClient.SteamId == steamId)
			{
				readyButton.gameObject.SetActive(!isReady);
				notreadyButton.gameObject.SetActive(isReady);
			}

			// Host면 게임시작버튼 띄워야됨
			if (NetworkManager.Singleton.IsHost)				
			{
				startButton.gameObject.SetActive(isAllReady);
			}
		}
	}
}