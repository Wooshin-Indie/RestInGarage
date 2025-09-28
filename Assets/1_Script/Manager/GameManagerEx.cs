using DG.Tweening;
using Garage.Controller;
using Garage.Environment;
using Garage.Structs;
using Garage.UI.MainScene;
using Garage.Utils;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
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

        public MapData CurMapData
        {
            get
            {
                int mapIdx = GameSynchronizer.Instance.MapIdx.Value;
                return (mapIdx < 0) ? null :
                    Managers.Resource.GetData<MapData>(mapIdx);
            }
        }
        public int CurStageIdx => GameSynchronizer.Instance.CurrentStage.Value;

        public bool IsDay { get => GameSynchronizer.Instance.IsDay.Value; }
        public ulong MyClientId { get => myClientId; set => myClientId = value; }
        public Dictionary<ulong, PlayerInfo> playerInfo = new Dictionary<ulong, PlayerInfo>();

		public Action OnDisconnectedAction { get; set; }			// Network Session 끊기면 호출
		public Action OnBeforeStageStartAction { get; set; }		// StartNextStage 호출 직후 실행
		public Action OnAfterStageStartAction { get; set; }			// 
		public Action OnTimeoutAction { get; set; }					// 스테이지 타임아웃시 실행
		public Action OnBeforeStageEndAction { get; set; }			// EndStage 호출 직후 실행
		public Action<int> OnAfterStageEndAction { get; set; }		// 
		public Action<Lobby> OnLobbyEnteredAction { get; set; }		// 
		public Action<int> OnStartGameAction { get; set; }			// 인게임 진입시 실행

        [SerializeField] private GameObject meetingPointPrefab;
        [SerializeField] private float stageTimer;

        private MeetingPoint meetingPoint;

        private float startStageDuration = 3f;
        private float timeoutDuration = 3f;
        private float endStageDuration = 2f;

        /// <summary>
        /// Stage를 시작할 때 호출하는 함수
        /// </summary>
        public void StartNextStage()
        {
            if (!isHost) return;

            SunManager.Instance.SetTimePhase(TimePhase.Morning, startStageDuration);

            OnBeforeStageStartAction?.Invoke();

            Invoke(nameof(OnStageStart), startStageDuration);
        }

        /// <summary>
        /// startStageDuration 후에 호출되는 함수
        /// ex. 타이머 시작, BuildingManager Init
        /// </summary>
        private void OnStageStart()
        {
            float stageTime = CurMapData.StageTime[CurStageIdx];
            GameSynchronizer.Instance.SetGameTimer(stageTime);
            SunManager.Instance.SetTimePhase(TimePhase.Afternoon, stageTime);
            TrafficManager.Instance.OnStageStart(0);    // TODO - 맵 여러개 생기면 MapIdx 고치기
            BuildingManager.Instance.OnStageStart();

            NetworkTransmission.instance.PlayStageBGMClientRPC();
        }

        
        public void EndStage()
        {
            SunManager.Instance.SetTimePhase(TimePhase.Night, endStageDuration);
            AllPlayersAwayFromLanesOnStageEnd();

			OnBeforeStageEndAction?.Invoke();

            DOVirtual.DelayedCall(awayMoveTime, () =>
            {
                TrafficManager.Instance.OnStageEnd();
            });

            if (GameSynchronizer.Instance.CurrentStage.Value != 0)
                Invoke(nameof(OnStageEnd), endStageDuration);
            else OnStageEnd();

            foreach (var player in NetworkManager.Singleton.ConnectedClients)
            {
                player.Value.PlayerObject.GetComponent<PlayerController>().EndAllInteraction();
            }
        }


        private void OnStageEnd()
        {
            if (!isHost) return;

            if (meetingPoint == null)
            {
                GameObject go = Managers.Spawn.SpawnInCurrentScene(meetingPointPrefab);
                meetingPoint = go.GetComponent<MeetingPoint>();
                meetingPoint.transform.position = new Vector3(-4f, 0f, -10f);
                meetingPoint.StartMeetClientRPC(0);
            }

            if (GameSynchronizer.Instance.IsDay.Value) return;
            if (GameSynchronizer.Instance.CurrentStage.Value == 0) return;

        }

        private void OnAfterStageEnd()
        {
			Managers.Sound.PlaySfx(SFXType.ShopCar, () =>
			{
				Managers.Sound.PlaySfx(SFXType.ShopPop);
				BuildingManager.Instance.OnStageEnd(GameSynchronizer.Instance.CurrentStage.Value);
				OnAfterStageEndAction?.Invoke(GameSynchronizer.Instance.CurrentStage.Value);
			});
		}

        private void OnUpdateTimer()
        {
            if (!isHost) return;
            if (GameSynchronizer.Instance.RemainedTime.Value <= 0f) return;

            GameSynchronizer.Instance.RemainedTime.Value -= Time.deltaTime;

            if (CurMapData.BossWaveInfos[CurStageIdx].isBossExist &&
                GameSynchronizer.Instance.RemainedTime.Value < CurMapData.BossWaveInfos[CurStageIdx].appearingTime)
            {
                StartBossFight(CurMapData.BossWaveInfos[CurStageIdx].bossType);
            }

            if (IsDay && GameSynchronizer.Instance.RemainedTime.Value <= 0f)
			{
				GameSynchronizer.Instance.TimeOutClientRPC();
                InputLockToAllPlayers();
                Invoke(nameof(EndStage), timeoutDuration);
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
            int mapIdx = GameSynchronizer.Instance.MapIdx.Value;
            OnStageEnd();

            OnStartGameAction.Invoke(mapIdx);
            BuildingManager.Instance.OnGameStarted();
        }

        public void HostCreated()
        {
            isHost = true;
            isConnected = true;
        }

        // 로비에서 게임 시작
        public void OnGameStartInLobby_HostOnly()
        {
            if (!isHost) return;

            foreach (ulong clientId in playerInfo.Keys)
            {
                PlayerController pc = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerController>();
                pc.PlayerID.Value = playerInfo[clientId].playerId; // 호스트 로컬에 있는 PlayController들에 clientId 할당
            }

            GameSynchronizer.Instance.CurrentStage.Value = 0;
            BuildingManager.Instance.OnGameStart();
            BuildingManager.Instance.OnStageEnd(GameSynchronizer.Instance.CurrentStage.Value);

            SunManager.Instance.OnChangedToNight();
            OnHostCreated();
        }

        public void OnHostCreated()
        {
            GameSynchronizer.Instance.IsDay.Value = false;
            SunManager.Instance.SetTimePhase(TimePhase.Night, 2f);
        }

        public void ConnectedAsClient()
        {
            isHost = false;
            isConnected = true;
        }

        public void Disconnected()
        {
            if (isHost)
                GameSynchronizer.Instance.MapIdx.Value = 0;
            playerInfo.Clear();

            OnDisconnectedAction.Invoke();

            SceneEnum curScene = Managers.Scene.CurrentScene.SceneEnum;

            switch (curScene)
            {
                case SceneEnum.Main:
                    GameObject[] playercards = GameObject.FindGameObjectsWithTag(Constants.TAG_PCARD);
                    foreach (GameObject card in playercards)
                    {
                        Destroy(card);
                    }
                    UIManager.Main.GoToPage(PageEnum.Main);
                    break;
                case SceneEnum.Game:
                    Managers.Scene.ChangeScene(SceneEnum.Main);
                    break;
            }
            isHost = false;
            isConnected = false;
        }

        public void AddPlayerToDictionary(ulong clientId, string steamName, ulong steamId, bool isReady = false)
        {
            if (!playerInfo.ContainsKey(clientId))
            {
                bool[] isExist = new bool[4] { false, false, false, false };
                foreach (var info in playerInfo)
                {
                    isExist[info.Value.playerId] = true;
                }
                int idx = -1;
                for (int i = 0; i < isExist.Length; i++)
                {
                    if (!isExist[i])
                    {
                        idx = i; break;
                    }
                }

                if (isHost)
                {
                    // NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerController>().PlayerID.Value = idx;
                }
                PlayerInfo pi = new PlayerInfo(steamName, steamId, idx);
                playerInfo.Add(clientId, pi);
                UIManager.Main.LobbyPage.OnAddPlayerToDictionary(clientId, pi);
            }
        }

        public void UpdateClients()
        {
            foreach (KeyValuePair<ulong, PlayerInfo> player in playerInfo)
            {
                ulong steamId = player.Value.steamId;
                string steamName = player.Value.steamName;
                ulong clientId = player.Key;
                bool isReady = player.Value.isReady;

                NetworkTransmission.instance.UpdateClientsPlayerInfoClientRPC(steamId, steamName, clientId, isReady);
            }
        }

        public void RemovePlayerFromDictionary(ulong steamId)
        {
            PlayerInfo value = null;
            ulong key = 100;
            foreach (KeyValuePair<ulong, PlayerInfo> player in playerInfo)
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
                UIManager.Main.LobbyPage.OnRemovePlayerFromDictionary(key);
            }
        }

        public ulong GetClientIDBySteamID(ulong steamId)
        {
            ulong clientId = ulong.MaxValue;
            foreach (KeyValuePair<ulong, PlayerInfo> player in playerInfo)
            {
                if (player.Value.steamId == steamId)
                {
                    clientId = player.Key;
                }
            }

            return clientId;
        }

        public void UpdatePlayerIsReady(bool isReady, ulong clientId)
        {
            foreach (KeyValuePair<ulong, PlayerInfo> player in playerInfo)
            {
                if (player.Key == clientId)
                {
                    player.Value.isReady = isReady;
                    UIManager.Main.LobbyPage.OnUpdatePlayerReady(isReady, player.Value.steamId);
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

        private float awayMoveTime = 3f;
        private void AllPlayersAwayFromLanesOnStageEnd()
        {
            List<ulong> clientIds = NetworkManager.Singleton.SpawnManager.GetConnectedPlayers();

            foreach (ulong clientId in clientIds)
            {
                NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId).
                    GetComponent<PlayerController>().AwayFromLanesOnStageEnd_HostOnly(awayMoveTime);
            }
        }
        private void InputLockToAllPlayers()
        {
            List<ulong> clientIds = NetworkManager.Singleton.SpawnManager.GetConnectedPlayers();

            foreach (ulong clientId in clientIds)
            {
                NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId).
                    GetComponent<PlayerController>().InputLockToPlayer_HostOnly();
            }
        }

		public void Quit()
		{
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

        #region Bosses
        public void StartBossFight(BossType bossType)
        {
            if (BossManager.Instance.IsBossAppeared) return;

            Debug.Log("Start BossFight, GameManagerEX");
            BossManager.Instance.StartBossFight(bossType);
        }

        #endregion

        #region Event

        public void StartEvent_HostOnly()
        {
			GameSynchronizer.Instance.StartEventServerRPC();
        }

        public void EndEvent()
        {
            GameSynchronizer.Instance.EndEvent();
        }

        public void OnEndEvent()
        {
            Camera.main.GetComponent<CameraController>().EndEvent();
            UIManager.Event.EndResultEvent();
            OnAfterStageEnd();
        }

		#endregion
	}

}