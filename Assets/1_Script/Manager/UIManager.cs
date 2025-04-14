using Garage.UI.LobbyScene;
using Garage.UI.MainScene;
using Garage.Utils;
using UnityEngine;

namespace Garage.Manager
{
    public class UIManager : MonoBehaviour
	{
		#region Singleton
		private static UIManager instance;
		public static UIManager Instance { get => instance; }

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

			OnSceneChanged(SceneEnum.Main);
		}
		#endregion

		[SerializeField] private GameObject mainUI;
		[SerializeField] private GameObject lobbyUI;

		public static MainSceneUI Main { get { return instance.mainUI.GetComponent<MainSceneUI>(); } }
		public static LobbySceneUI Lobby { get {  return instance.lobbyUI.GetComponent<LobbySceneUI>(); } }

		// HACK - UI를 다 메모리에 올려놓는 방식임.
		// 메모리 부족하면 실시간으로 Instantiate 하는 방식으로 바꿔야됨
		public void OnSceneChanged(SceneEnum scene)
		{
			mainUI.SetActive(false);
			lobbyUI.SetActive(false);

			switch (scene)
			{
				case SceneEnum.None:
					break;
				case SceneEnum.Main:
					mainUI.SetActive(true);
					break;
				case SceneEnum.Lobby:
					lobbyUI.SetActive(true);
					break;
				case SceneEnum.Game:
					break;
			}
		}

	}
}