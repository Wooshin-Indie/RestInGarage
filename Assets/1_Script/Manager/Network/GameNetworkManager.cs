using UnityEngine;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using Netcode.Transports.Facepunch;
using System.Threading.Tasks;
using Garage.Utils;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Garage.UI.Item;

namespace Garage.Manager
{
	public class GameNetworkManager : MonoBehaviour
	{
		private FacepunchTransport transport = null;

		public Lobby? currentLobby { get; private set; } = null;

        #region Singleton
        public static GameNetworkManager Instance { get => instance; }
        private static GameNetworkManager instance = null;
        private void Awake()
		{
			if (instance == null)
			{
				instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
				return;
			}
		}
        #endregion

        private void Start()
		{
			transport = GetComponent<FacepunchTransport>();

			SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
			SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
			SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyJoined;
			SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyLeaved;
			SteamMatchmaking.OnLobbyInvite += SteamMatchMaking_OnLobbyInvite;
			SteamMatchmaking.OnLobbyGameCreated += SteamMatchmaking_OnLobbyGameCreated;
			SteamFriends.OnGameLobbyJoinRequested += SteamFriends_OnGameLobbyJoinRequested;
        }
		private void OnDestroy()
		{
			SteamMatchmaking.OnLobbyCreated -= SteamMatchmaking_OnLobbyCreated;
			SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
			SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmaking_OnLobbyJoined;
			SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyLeaved;
			SteamMatchmaking.OnLobbyInvite -= SteamMatchMaking_OnLobbyInvite;
			SteamMatchmaking.OnLobbyGameCreated -= SteamMatchmaking_OnLobbyGameCreated;
			SteamFriends.OnGameLobbyJoinRequested -= SteamFriends_OnGameLobbyJoinRequested;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnGameSceneLoaded;

            if (NetworkManager.Singleton == null) return;

			NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
			NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
			NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectedCallback;
		}
		private void OnApplicationQuit()
		{
			Disconnected();
		}

        #region Lobby Callbacks
        private void SteamMatchmaking_OnLobbyCreated(Result result, Lobby lobby)
		{
			if (result != Result.OK)
			{
				Debug.Log("Lobby was not created, result: " + result);
				// TODO - 로비 생성 실패 팝업?
				return;
			}

            // 로비 데이터 초기화
            lobby.SetPublic();
			UIManager.Main.LobbyPage.UpdateLobbyType(LobbyType.Public);
            lobby.SetJoinable(true);
            Debug.Log($"Lobby created : {lobby.Owner.Name}");

            // Host 시작
            Debug.Log("START HOST");
            NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
            NetworkManager.Singleton.StartHost();
            GameManagerEx.Instance.MyClientId = NetworkManager.Singleton.LocalClientId;

			// 로비UI 띄우고 초기화
			UIManager.Main.LobbyPage.InitLobbyDatas_Host();
			UIManager.Main.GoToPage(UI.MainScene.PageEnum.Lobby);

            NetworkTransmission.instance.AddMeToDictionayServerRPC(SteamClient.SteamId, SteamClient.Name, NetworkManager.Singleton.LocalClientId);
			// PlayerSpawner.Instance.SpawnPlayerServerRPC(NetworkManager.Singleton.LocalClientId);
		}
        private void SteamMatchmaking_OnLobbyEntered(Lobby lobby)
		{
			Debug.Log("Lobby entered");
			if (NetworkManager.Singleton.IsHost) return;
            // 이거 Host방어 가끔 뚫리는데 IsHost가 True로 되기 전에 입장이 돼서 그런 것 같음
            // IsHost = true 되는 시점은 NetworkManager.Singleton.StartHost() 호출 시.

            currentLobby = lobby;
			GameManagerEx.Instance.ConnectedAsClient();
			StartClient(lobby.Owner.Id);
		}
		private void SteamMatchmaking_OnLobbyJoined(Lobby lobby, Friend friend)
		{
			Debug.Log("member join");
		}
		private void SteamMatchmaking_OnLobbyLeaved(Lobby lobby, Friend friend)
		{
			Debug.Log("member leave");
			if(friend.Id == lobby.Owner.Id)
			{
				Debug.Log("HOST LEAVED");
			}
			GameManagerEx.Instance.SendMessageToChat($"{friend.Name} has left", friend.Id, true);
			NetworkTransmission.instance.RemoveMeFromDictionaryServerRPC(friend.Id);
        }
		private void SteamMatchMaking_OnLobbyInvite(Friend friend, Lobby lobby)
		{
			Debug.Log($"Invite from {friend.Name}");
		}
		private void SteamMatchmaking_OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId)
		{
			Debug.Log("LobbyGame created");
			GameManagerEx.Instance.SendMessageToChat($"LobbyGame created : ", NetworkManager.Singleton.LocalClientId, true);
		}

		// Accept the invice or join on a friend
		private async void SteamFriends_OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
		{
			RoomEnter joinedLobby = await lobby.Join();

			if (joinedLobby != RoomEnter.Success)
			{
				Debug.Log("Failed to create lobby");
			}
			else
			{
				Debug.Log("Joined Lobby");
			}
		}
        #endregion

        #region FromLobbyToGame Sequences
        public void StartHost()
		{
			Debug.Log("Start host...");
			CreateLobby();
        }
		public async void CreateLobby()
        {
            Debug.Log("Create lobby...");
            currentLobby = await SteamMatchmaking.CreateLobbyAsync(Constants.MAX_PLAYERS);
            currentLobby.Value.SetData(Constants.KEY_LOBBYNAME, $"{SteamClient.Name}'s lobby");
            currentLobby.Value.SetData(Constants.KEY_GAMENAME, Constants.VALUE_GAMENAME);
        }
		public void StartClient(SteamId steamId)
		{
            Debug.Log("Start client...");
            NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
			NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectedCallback;
			transport.targetSteamId = steamId.Value;
            GameManagerEx.Instance.MyClientId = NetworkManager.Singleton.LocalClientId;

			//UIManager.Transition.StartTransition(0f);
			if (NetworkManager.Singleton.StartClient())
			{
				Debug.Log("StartClient...");
			}
		}
		public void StartGameInLobby()
		{
			if (!NetworkManager.Singleton.IsHost) return;
			if (!GameManagerEx.Instance.IsAllPlayerReady()) return;

            NetworkManager.Singleton.SceneManager.OnUnloadEventCompleted += OnSceneUnloaded;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneloaded;
            currentLobby.Value.SetGameServer(currentLobby.Value.Owner.Id);
            Debug.Log("Start Game in lobby...");
            LockLobby();

            Managers.Scene.ChangeSceneServer(SceneEnum.Lobby);

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnGameSceneLoaded;
        }
		private void OnGameSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
		{
			if (sceneName != "LobbyScene") return;

			if (!NetworkManager.Singleton.IsHost) return;

            Debug.Log("Game Scene loaded on all clients. Spawning players...");

            foreach (ulong clientId in GameManagerEx.Instance.playerInfo.Keys)
            {
                NetworkTransmission.instance.SpawnPlayer(clientId, GetRandomSpawnPosition());
            }

            GameManagerEx.Instance.OnGameStartInLobby_HostOnly();

			NetworkTransmission.instance.StartGameServerRPC();

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnGameSceneLoaded;
        }

        private Vector3 GetRandomSpawnPosition()
		{
			Vector3 spawnPos = Vector3.zero;
			spawnPos.x = UnityEngine.Random.Range(-1,1);
			spawnPos.z = UnityEngine.Random.Range(-1,1);

			return spawnPos;
		}
		// TODO - 나중에 인게임 메뉴 버튼에 할당해야됨
		public async void Disconnected()
		{

			NetworkTransmission.instance.EndHeartbeat();
			// PlayerSpawner.Instance.DespawnPlayerServerRPC(NetworkManager.Singleton.LocalClientId);
			if (NetworkManager.Singleton.IsHost)
			{
				NetworkTransmission.instance.DisconnectAllClientRPC();
				await Task.Delay(500);
			}

			currentLobby?.Leave();
			if (NetworkManager.Singleton == null) return;

			if (NetworkManager.Singleton.IsHost)
			{
				NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
			}
			else
			{
				NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
			}
			NetworkManager.Singleton.Shutdown(true);
            Debug.Log("Shutdown.");
            GameManagerEx.Instance.Disconnected();
            Debug.Log("Disconnected.");
        }
		public async void FindLobbiesWithCallback(System.Action<Lobby[]> callback)
		{
			var query = SteamMatchmaking.LobbyList
				.WithKeyValue(Constants.KEY_GAMENAME, Constants.VALUE_GAMENAME)
				.FilterDistanceClose();

			var lobbies = await query.RequestAsync();

			if (lobbies == null) return;

			callback.Invoke(lobbies);
			return;
		}
		public async void JoinLobby(Lobby lobby)
		{
			try
			{
				await lobby.Join();
			}
			catch (System.Exception e)
			{
				Debug.LogWarning($"Lobby enter failed : {e.Message}");
			}
		}
        public void LockLobby()
        {
            currentLobby.Value.SetJoinable(false);
        }
        public void UnlockLobby()
        {
            currentLobby.Value.SetJoinable(true);
        }
		#endregion

        private void Singleton_OnClientDisconnectedCallback(ulong clientId)
		{
			NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectedCallback;
			Debug.Log("Client Disconnected");
			if (clientId == 0)
			{
				Disconnected();
			}
		}
		private void Singleton_OnClientConnectedCallback(ulong clientId)
        {
			Managers.Scene.UnloadCurrentScene();

            NetworkTransmission.instance.AddMeToDictionayServerRPC(SteamClient.SteamId, SteamClient.Name, clientId); 
			GameManagerEx.Instance.MyClientId = clientId;

			NetworkTransmission.instance.IsTheClientReadyServerRPC(false, clientId);
			NetworkTransmission.instance.SyncLobbyTypeServerRPC(clientId);
            Debug.Log($"Client has connected : {clientId}");

			NetworkTransmission.instance.StartHeartbeat();


			if (NetworkManager.Singleton.IsHost) return;

            UIManager.Main.GoToPage(UI.MainScene.PageEnum.Lobby);
            UIManager.Main.LobbyPage.InitLobbyDatas_Client(clientId);
        }
		private void Singleton_OnServerStarted()
		{
			Debug.Log("Host started");
			GameManagerEx.Instance.HostCreated();
		}

        private void OnSceneUnloaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
		{
			Debug.Log("Unload Complete! Curscene: " + Managers.Scene.CurrentScene);
		}
		private void OnSceneloaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
		{
			Debug.Log("Load Complete! Curscene: " + Managers.Scene.CurrentScene);
		}

    }
}