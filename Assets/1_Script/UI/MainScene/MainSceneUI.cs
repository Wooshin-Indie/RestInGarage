using Garage.Manager;
using Garage.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Garage.UI.MainScene
{
    public class MainSceneUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;

		private void Start()
		{
			hostButton.onClick.AddListener(() =>
			{
				GameNetworkManager.Instance.StartHost(Constants.MAX_PLAYERS);
			});
		}

		private void OnDestroy()
		{
			hostButton.onClick.RemoveAllListeners();
		}
	}
}