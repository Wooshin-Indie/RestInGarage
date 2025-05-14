using Garage.Controller;
using Garage.Environment;
using Garage.Structs;
using Garage.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Manager
{
    public class GameManagerEx : MonoBehaviour
    {
		#region Singleton
		private static GameManagerEx instance;
		public static GameManagerEx Instance { get => instance; }

		void Awake()
		{
			Init();
		}

		private void Init()
		{
			if (null == instance)
			{
				instance = this;
				DontDestroyOnLoad(this.gameObject);
			}
			else
			{
				Destroy(this.gameObject);
			}
		}
		#endregion

		private bool isConnected;
		private bool isGame;
		private bool isHost;
		private ulong myClientId;

		public bool IsDay { get => GameSynchronizer.Instance.IsDay.Value; }
		public ulong MyClientId { get => myClientId; set => myClientId = value;}
		public Dictionary<ulong, PlayerInfo> playerInfo = new Dictionary<ulong, PlayerInfo>();
		public Action OnDisconnected { get; set; }

		[SerializeField] private MeetingPoint meetingPoint;


		public void StartNextStage()
		{
			meetingPoint.EndMeet();

			SunManager.Instance.SetTimePhase(TimePhase.Morning, 3f);
			UIManager.Game.PopupStageStart(3f);
			Invoke(nameof(OnStageStart), 3f);

			GameSynchronizer.Instance.IsDay.Value = true;
			GameSynchronizer.Instance.CurrentStage.Value++;

			BuildingManager.Instance.OnStageStart();
		}

		public void OnStageStart()
		{
			GameSynchronizer.Instance.SetGameTimer(20f);
			SunManager.Instance.SetTimePhase(TimePhase.Afternoon, 20f);
			BuildingManager.Instance.OnStageStart();
		}

		public void EndStage()
		{
			SunManager.Instance.SetTimePhase(TimePhase.Night, 2f);
			Invoke(nameof(OnStageEnd), 2f);

			GameSynchronizer.Instance.IsDay.Value = false;
		}

		public void OnStageEnd()
		{
			if (GameSynchronizer.Instance.CurrentStage.Value != 0)
				meetingPoint.StartMeet();

			if (GameSynchronizer.Instance.IsDay.Value) return;
			BuildingManager.Instance.OnStageEnd(GameSynchronizer.Instance.CurrentStage.Value);
		}


		private void OnUpdateTimer()
		{
			if (!isHost) return;
			if (GameSynchronizer.Instance.RemainedTime.Value < 0f) return;

			GameSynchronizer.Instance.RemainedTime.Value -= Time.deltaTime;

			if (IsDay && GameSynchronizer.Instance.RemainedTime.Value < 0f)
			{
				UIManager.Game.PopupStageEnd(3f);
				Invoke(nameof(EndStage), 3f);
			}
		}

		private void Update()
		{
			OnUpdateTimer();
		}


		public void SendMessageToChat(string text, ulong fromwho, bool server)
		{
			string name = Constants.NAME_SERVER;

			if (!server && playerInfo.ContainsKey(fromwho))
			{
				name = playerInfo[fromwho].steamName;
			}

			UIManager.Lobby.SendMessageToUI(name, text);
		}

		public void GameStarted()
		{
			UIManager.Instance.OnGameStart();
			StartNextStage();
		}

		public void GameEnded()
		{
			Managers.Scene.ChangeSceneServer(Utils.SceneEnum.Lobby);
		}

		public void HostCreated()
		{
			Managers.Scene.ChangeSceneServer(SceneEnum.Lobby);
			isHost = true;
			isConnected = true;

			GameSynchronizer.Instance.CurrentStage.Value = 0;

			BuildingManager.Instance.OnGameStart();
			BuildingManager.Instance.OnStageEnd(0);

			SunManager.Instance.OnChangedToNight();
			EndStage();
		}

		public void ConnectedAsClient()
		{
			Managers.Scene.UnloadCurrentSceneServer();

			isHost = false;
			isConnected = true;
		}

		public void Disconnected()
		{
			playerInfo.Clear();
			GameObject[] playercards = GameObject.FindGameObjectsWithTag(Constants.TAG_PCARD);
			foreach(GameObject card in playercards)
			{
				Destroy(card);
			}

			OnDisconnected.Invoke();

			Managers.Scene.ChangeSceneServer(SceneEnum.Main);
			isHost = false;
			isConnected = false;
		}

		public void AddPlayerToDictionary(ulong clientId, string steamName, ulong steamId)
		{
			if (!playerInfo.ContainsKey(clientId))
			{
				bool[] isExist = new bool[4] { false, false, false, false };
				foreach(var info in playerInfo)
				{
					isExist[info.Value.playerId] = true;
				}
				int idx = -1;
				for(int i=0;i <isExist.Length;i++)
				{
					if (!isExist[i])
					{
						idx = i; break;
					}
				}

				if (isHost)
				{
					NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerController>().PlayerID.Value = idx;
				}
				PlayerInfo pi = new PlayerInfo(steamName, steamId, idx);
				playerInfo.Add(clientId, pi);
				UIManager.Lobby.OnAddPlayerToDictionary(pi);
			}
		}

		public void UpdateClients()
		{
			foreach(KeyValuePair<ulong, PlayerInfo> player in playerInfo)
			{
				ulong steamId = player.Value.steamId;
				string steamName = player.Value.steamName;
				ulong clientId = player.Key;

				NetworkTransmission.instance.UpdateClientsPlayerInfoClientRPC(steamId, steamName, clientId);
			}
		}

		public void RemovePlayerFromDictionary(ulong steamId)
		{
			PlayerInfo value = null;
			ulong key = 100;
			foreach(KeyValuePair<ulong, PlayerInfo> player in playerInfo)
			{
				if (player.Value.steamId == steamId)
				{
					value = player.Value;
					key = player.Key;
				}
			}
			if (key != 100)
			{
				playerInfo.Remove(key);
			}
			UIManager.Lobby.OnRemovePlayerFromDictionary(value);
		}

		public void UpdatePlayerIsReady(bool isReady, ulong clientId)
		{
			foreach (KeyValuePair<ulong, PlayerInfo> player in playerInfo)
			{
				if (player.Key == clientId)
				{
					player.Value.isReady = isReady;
					UIManager.Lobby.OnUpdatePlayerReady(isReady, player.Value.steamId);
				}
			}
		}

		public bool IsAllPlayerReady()
		{
			foreach (KeyValuePair<ulong, PlayerInfo> player in playerInfo)
			{
				if (!player.Value.isReady)
				{
					return false;
				}
			}

			return true;
		}

		public void Quit()
		{
			Application.Quit();
		}
	}
}