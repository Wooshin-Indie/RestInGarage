using DG.Tweening;
using Garage.Manager;
using Garage.Structs;
using Garage.UI.LobbyScene.Items;
using Garage.UI.MainScene;
using Garage.Utils;
using Netcode.Transports.Facepunch;
using NUnit.Framework;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Garage.UI.Item
{
    public class LobbyPageUI : PageUI
    {
        [SerializeField] private Transform itemParent;
        [SerializeField] private GameObject playerCardPrefab;
        [SerializeField] private Button startButton;
        [SerializeField] private Button inviteFriendsButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button convertLobbyTypeLeftButton;
        [SerializeField] private Button convertLobbyTypeRightButton;
        [SerializeField] private Button page10BackButton;
        [SerializeField] private TextMeshProUGUI lobbyTypeText;

        private Dictionary<ulong, PlayerCard> playerCardDict = new();
        private Lobby? curLobby;
        public LobbyType CurLobbyType = LobbyType.None;

        private void Awake()
        {
            GameManagerEx.Instance.OnDisconnected += OnDisconnected;
        }

        private void Start()
        {
            // backButton은 MainSceneUI 에서 관리
            startButton.onClick.AddListener(() =>
            {
                UIManager.Transition.StartTransition(.5f);
                DOVirtual.DelayedCall(.5f, () =>
                {
                    GameNetworkManager.Instance.StartGameInLobby();
                });
                // TODO - MainUI 초기화 코드 필요
            });
            inviteFriendsButton.onClick.AddListener(() =>
            {
                // TODO - 친구초대기능 추가
            });
            readyButton.onClick.AddListener(() =>
            {
                NetworkTransmission.instance.IsTheClientReadyServerRPC(
                    !(GameManagerEx.Instance.playerInfo[GameManagerEx.Instance.MyClientId].isReady), 
                    GameManagerEx.Instance.MyClientId);
            });
            convertLobbyTypeLeftButton.onClick.AddListener(() =>
            {
                LeftRotateLobbyType();
            });
            convertLobbyTypeRightButton.onClick.AddListener(() =>
            {
                RightRotateLobbyType();
            });
            page10BackButton.onClick.AddListener(() =>
            {
                // TODO - 로비 파괴
                GameNetworkManager.Instance.Disconnected();
            });
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }
        protected override void Update()
        {
            base.Update();
        }

        public void InitLobbyDatas_Host()
        {
            curLobby = GameNetworkManager.Instance.currentLobby;
            if (curLobby == null)
            {
                Debug.Log("CurLobby doesn't exist");
                return;
            }
            Debug.Log("Init Lobby");

            startButton.gameObject.SetActive(true);
            convertLobbyTypeLeftButton.gameObject.SetActive(true);
            convertLobbyTypeRightButton.gameObject.SetActive(true);

            // friends only인지 public인지 private인지


        }
        public void InitLobbyDatas_Client(ulong clientId)
        {
            if (clientId == GameManagerEx.Instance.MyClientId)

            curLobby = GameNetworkManager.Instance.currentLobby;
            if (curLobby == null)
            {
                Debug.Log("CurLobby doesn't exist");
                return;
            }
            Debug.Log("Init Lobby");

            startButton.gameObject.SetActive(false);
            convertLobbyTypeLeftButton.gameObject.SetActive(false);
            convertLobbyTypeRightButton.gameObject.SetActive(false);

            SyncReadyCheckBoxes();
        }
        public void SyncLobbyDatas()
        {
            if(curLobby == null)
            {
                Debug.Log("CurLobby doesn't exist");
            }

        }
        private void LeftRotateLobbyType()
        {
            if (!NetworkManager.Singleton.IsHost) return;
            
            switch (CurLobbyType)
            {
                case LobbyType.Public:
                    CurLobbyType = LobbyType.FriendsOnly;
                    break;
                case LobbyType.Private:
                    CurLobbyType = LobbyType.Public;
                    break;
                case LobbyType.FriendsOnly:
                    CurLobbyType = LobbyType.Private;
                    break;
            }
            NetworkTransmission.instance.UpdateLobbyTypeServerRPC(CurLobbyType);
        }
        private void RightRotateLobbyType()
        {
            if (!NetworkManager.Singleton.IsHost) return;

            switch (CurLobbyType)
            {
                case LobbyType.Public:
                    CurLobbyType = LobbyType.Private;
                    break;
                case LobbyType.Private:
                    CurLobbyType = LobbyType.FriendsOnly;
                    break;
                case LobbyType.FriendsOnly:
                    CurLobbyType = LobbyType.Public;
                    break;
            }
            NetworkTransmission.instance.UpdateLobbyTypeServerRPC(CurLobbyType);
        }
        public void UpdateLobbyType(LobbyType lobbyType)
        {
            CurLobbyType = lobbyType;

            switch (CurLobbyType)
            {
                case LobbyType.Public:
                    lobbyTypeText.text = "Public";
                    break;
                case LobbyType.Private:
                    lobbyTypeText.text = "Private";
                    break;
                case LobbyType.FriendsOnly:
                    lobbyTypeText.text = "FriendsOnly";
                    break;
                default:
                    Debug.Log("Invalid LobbyType was set");
                    break;
            }
        }

        public void OnAddPlayerToDictionary(ulong clientId, PlayerInfo pi)
        {
            PlayerCard pc = Instantiate(playerCardPrefab, itemParent).GetComponent<PlayerCard>();
            pc.SetPlayerCard(pi);
            playerCardDict.Add(clientId, pc);
        }
        public void OnRemovePlayerFromDictionary(ulong clientId)
        {
            if (playerCardDict.ContainsKey(clientId))
            {
                Destroy(playerCardDict[clientId].gameObject);
                playerCardDict.Remove(clientId);
            }
            else
            {
                Debug.Log("Invalid Removing clientId...");
            }

            CheckCanStartGame();
        }

        public void OnUpdatePlayerReady(bool isReady, ulong steamId)
        {
            foreach (PlayerCard card in playerCardDict.Values)
            {
                if (card.steamId == steamId)
                {
                    card.readyImage.SetActive(isReady);
                }
            }

            CheckCanStartGame();
        }

        private void SyncReadyCheckBoxes()
        {
            foreach (KeyValuePair<ulong, PlayerInfo> pi in GameManagerEx.Instance.playerInfo)
            {
                playerCardDict[pi.Key].readyImage.SetActive(pi.Value.isReady);
            }
        }

        private void CheckCanStartGame()
        {
            if (!NetworkManager.Singleton.IsHost) return;
            
            if (GameManagerEx.Instance.IsAllPlayerReady())
            {
                startButton.enabled = true;
                startButton.GetComponent<UnityEngine.UI.Image>().color = UnityEngine.Color.white;
                startButton.GetComponentInChildren<TextMeshProUGUI>().color = UnityEngine.Color.white;
            }
            else
            {
                startButton.enabled = false;
                startButton.GetComponent<UnityEngine.UI.Image>().color = UnityEngine.Color.clear;
                startButton.GetComponentInChildren<TextMeshProUGUI>().color = UnityEngine.Color.grey;
            }
        }

        public void OnDisconnected()
        {
            foreach (PlayerCard card in playerCardDict.Values)
            {
                Destroy(card.gameObject);
            }
            playerCardDict.Clear();
        }

        public void OnDisable()
        {
            foreach (PlayerCard card in playerCardDict.Values)
            {
                Destroy(card.gameObject);
            }
            playerCardDict.Clear();
            // disconnect 구현해야됨
        }
    }
}
