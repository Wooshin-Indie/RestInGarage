using Garage.UI.LobbyScene;
using Garage.UI.MainScene;
using Garage.UI.GameScene;
using Garage.Utils;
using UnityEngine;
using Garage.UI;
using System;

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

			transitionUI.SetActive(true);
			OnSceneChangeStarted(SceneEnum.Main);
		}
		#endregion

		[SerializeField] private GameObject mainUI;
		[SerializeField] private GameObject lobbyUI;
        [SerializeField] private GameObject gameUI;
        [SerializeField] private GameObject eventUI;
        [SerializeField] private GameObject transitionUI;

        public static MainSceneUI Main { get { return instance.mainUI.GetComponent<MainSceneUI>(); } }
		public static UI.LobbyScene.LobbySceneUI Lobby { get {  return instance.lobbyUI.GetComponent<UI.LobbyScene.LobbySceneUI>(); } }
		public static UI.GameScene.GameSceneUI Game { get { return instance.gameUI.GetComponent<UI.GameScene.GameSceneUI>(); } }
		public static UI.Event.EventUI Event { get { return instance.eventUI.GetComponent<UI.Event.EventUI>();} }
		public static TransitionUI Transition { get { return instance.transitionUI.GetComponent<TransitionUI>(); } }

		private void Start()
		{
			GameManagerEx.Instance.OnStartGameAction += ((int index) => {
				OnGameStart();
			});
		}

		// HACK - UI를 다 메모리에 올려놓는 방식임.
		// 메모리 부족하면 실시간으로 Instantiate 하는 방식으로 바꿔야됨
		public void OnSceneChangeStarted(SceneEnum scene)
		{
			mainUI.SetActive(false);
			lobbyUI.SetActive(false);
			gameUI.SetActive(false);
			eventUI.SetActive(false);

			switch (scene)
			{
				case SceneEnum.None:
					break;
				case SceneEnum.Main:
					mainUI.SetActive(true);
					break;
				case SceneEnum.Game:
					lobbyUI.SetActive(true);
                    lobbyUI.GetComponent<UI.LobbyScene.LobbySceneUI>().OnGameEnd();
					break;
			}
		}

		public void OnGameStart()
		{
            lobbyUI.GetComponent<UI.LobbyScene.LobbySceneUI>().OnGameStart();
			gameUI.SetActive(true);
			eventUI.SetActive(true);
		}
	}
}