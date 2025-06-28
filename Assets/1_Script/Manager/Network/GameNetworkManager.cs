using UnityEngine;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using Netcode.Transports.Facepunch;
using System.Threading.Tasks;
using Garage.Utils;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Garage.UI.MainScene;
using Garage.Structs;

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
        private void Update()
        {
			if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient)
            {
                ulong ping = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId);
				Debug.Log("PingRtt: " + ping + "ms");
            }
        }

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
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadedInNetwork;

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
            StartHost();
		}
        private void SteamMatchmaking_OnLobbyEntered(Lobby lobby)
		{
			Debug.Log("Lobby entered");
			if (lobby.Owner.Id == SteamClient.SteamId) return;

            currentLobby = lobby;
			StartClient(lobby.Owner.Id);
		}
		private void SteamMatchmaking_OnLobbyJoined(Lobby lobby, Friend friend)
		{
			Debug.Log("member join");
		}

		// Only Lobby Owner
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
            Debug.Log("START HOST...");
            NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectedCallback;
            GameManagerEx.Instance.MyClientId = NetworkManager.Singleton.LocalClientId;
            // 이거 순서 바꾸자. 버튼으로 CreateLobby를 호출 한 다음에 StartHost에서 네트워크매니저StartHost하고 쭉 콜백넣고 해서 정리
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

            Managers.Scene.ChangeSceneServer(SceneEnum.Game);

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadedInNetwork;
        }
		private void OnSceneLoadedInNetwork(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
		{
            Debug.Log("Scene loaded by Server");
            if (sceneName == "GameScene")
			{

                if (!NetworkManager.Singleton.IsHost) return;

                Debug.Log("Game Scene loaded on all clients. Spawning players...");

                foreach (ulong clientId in GameManagerEx.Instance.playerInfo.Keys)
                {
                    NetworkTransmission.instance.SpawnPlayer(clientId, GetRandomSpawnPosition());
                }

                GameManagerEx.Instance.OnGameStartInLobby_HostOnly();

                NetworkTransmission.instance.StartGameServerRPC();
            }
        }
		private void OnSceneLoadedInLocal()
        {
            //if (sceneName == "MainScene")
                UIManager.Main.GoToPage(PageEnum.Main);
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
			Debug.Log("DISCONNECTED");
			NetworkTransmission.instance.EndHeartbeat();
			// PlayerSpawner.Instance.DespawnPlayerServerRPC(NetworkManager.Singleton.LocalClientId);
			if (NetworkManager.Singleton.IsHost)
			{
				NetworkTransmission.instance.DisconnectAllClientRPC();
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
            GameManagerEx.Instance.Disconnected();
            Debug.Log("Disconnected.");
            NetworkManager.Singleton.Shutdown(true);
            Debug.Log("Shutdown.");
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

        // Both Server and Client
		// 근데 이거 Client에서는 실행이 안되네
        private void Singleton_OnClientDisconnectedCallback(ulong clientId)
		{
			Debug.Log("Client Disconnected, ClientID: " + clientId);
			if (clientId == NetworkManager.Singleton.LocalClientId)
			{
				Disconnected();
                NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectedCallback;
            }
		}
		
		public void OpenInviteWindow()
		{
			SteamFriends.OpenGameInviteOverlay(currentLobby.Value.Id);
		}

		// Both Server and Client
		private void Singleton_OnClientConnectedCallback(ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost) return;
			Debug.Log("Client Synchronization Mode: " + NetworkManager.Singleton.SceneManager.ClientSynchronizationMode);
            GameManagerEx.Instance.ConnectedAsClient();
            Managers.Scene.UnloadCurrentScene();

            NetworkTransmission.instance.AddMeToDictionayServerRPC(SteamClient.SteamId, SteamClient.Name, clientId); 
			GameManagerEx.Instance.MyClientId = clientId;

			NetworkTransmission.instance.IsTheClientReadyServerRPC(false, clientId);
			NetworkTransmission.instance.SyncLobbyTypeServerRPC(clientId);
            Debug.Log($"Client has connected : {clientId}");

			NetworkTransmission.instance.StartHeartbeat();

            UIManager.Main.LobbyPage.InitLobbyDatas_Client(clientId);
            UIManager.Main.GoToPage(UI.MainScene.PageEnum.Lobby);
        }
		private void Singleton_OnServerStarted()
		{
			Debug.Log("OnServerStarted Callback...");
			GameManagerEx.Instance.HostCreated();

            // 로비UI 띄우고 초기화
            UIManager.Main.LobbyPage.InitLobbyDatas_Host();
            UIManager.Main.GoToPage(UI.MainScene.PageEnum.Lobby);

            NetworkTransmission.instance.AddMeToDictionayServerRPC(SteamClient.SteamId, SteamClient.Name, NetworkManager.Singleton.LocalClientId);
        }

        private void OnSceneUnloaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
		{
			Debug.Log("Unload Complete! Curscene: " + Managers.Scene.CurrentScene);
        }
		private void OnSceneloaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
		{
			Debug.Log("Load Complete! Curscene: " + Managers.Scene.CurrentScene);
            UIManager.Transition.EndTransition(1f, .5f);
        }

    }

}