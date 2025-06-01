using Garage.Manager;
using Garage.Utils;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.UI.Item
{
    public class LobbyListItem : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image lockImage;
        [SerializeField] private TextMeshProUGUI lobbyName;
        [SerializeField] private TextMeshProUGUI lobbyMembers;


        public void SetLobbyInfo(Lobby lobby)
        {
            lobbyName.text = $"{lobby.GetData(Constants.KEY_LOBBYNAME)}";
            lobbyMembers.text = $"{lobby.MemberCount}/{Constants.MAX_PLAYERS}";

            GetComponent<Button>().onClick.AddListener(() =>
            {
                GameNetworkManager.Instance.JoinLobby(lobby);
            });
        }
    }
}