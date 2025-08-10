using DG.Tweening;
using Garage.Manager;
using Garage.Structs;
using Garage.UI.LobbyScene.Items;
using Garage.Utils;
using IUtil;
using Manager;
using Steamworks.Data;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.UI.Item
{
    public class LobbyPageUI : PageUI
    {
        [SerializeField] private Transform playerCardContent;
        [SerializeField] private GameObject playerCardPrefab;
        [FoldoutGroup("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button inviteFriendsButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private Button convertLobbyTypeLeftButton;
        [SerializeField] private Button convertLobbyTypeRightButton;
        [SerializeField] private Button page10BackButton;
        [FoldoutGroup("Others")]
        [SerializeField] private TextMeshProUGUI lobbyTypeText;
        [SerializeField] private List<PerkUI> perkUIList = new();

        private Dictionary<ulong, PlayerCard> playerCardDict = new();
        private Lobby? curLobby;
        public LobbyType CurLobbyType = LobbyType.None;

        private void Awake()
        {
            GameManagerEx.Instance.OnDisconnectedAction += OnDisconnected;
            InitPerkSetting();
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

            SyncReadyCheckboxes();
        }

        #region Perk Settings
        private Dictionary<StatEnum, PerkUI> perkDict = new();
        private KeyValuePair<StatEnum, float> nonePerk = new(StatEnum.None, 0f);
        private KeyValuePair<StatEnum, float> currentPerk = new(StatEnum.None, 0f);
        private void InitPerkSetting()
        {
            foreach (PerkUI perkUI in perkUIList)
            {
                perkDict.Add(perkUI.Stat, perkUI);
            }
        }

        public void SetCurrentPerk(KeyValuePair<StatEnum, float> perk)
        {
            if (Equals(perk, currentPerk))
            {
                // 이미 활성화된 perk 누르면 비활성화
                InactivatePerk(currentPerk);
                currentPerk = nonePerk;

                return;
            }

            if (!Equals(nonePerk, currentPerk))
            {
                InactivatePerk(currentPerk);
            }
            ActivatePerk(perk);
            currentPerk = perk;
        }

        private void ActivatePerk(KeyValuePair<StatEnum, float> perk)
        {
            StatManager.Instance.SetStat(perk.Key, perk.Value);

            perkDict[perk.Key].GetComponent<UnityEngine.UI.Image>().color = UnityEngine.Color.white;
        }
        private void InactivatePerk(KeyValuePair<StatEnum, float> perk)
        {
            StatManager.Instance.SetStat(perk.Key, 1f);
            perkDict[perk.Key].GetComponent<UnityEngine.UI.Image>().color = UnityEngine.Color.grey;
        }

        private void LockPerk(KeyValuePair<StatEnum, float> perk)
        {
            perkDict[perk.Key].LockPerk();
        }
        private void UnlockPerk(KeyValuePair<StatEnum, float> perk)
        {
            perkDict[perk.Key].UnlockPerk();
        }

        #endregion

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
            PlayerCard pc = Instantiate(playerCardPrefab, playerCardContent).GetComponent<PlayerCard>();
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

        private void SyncReadyCheckboxes()
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
