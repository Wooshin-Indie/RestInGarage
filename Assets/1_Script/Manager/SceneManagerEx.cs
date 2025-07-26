using DG.Tweening;
using Garage.Structs;
using Garage.Utils;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Garage.Manager
{
	public class SceneManagerEx
	{
		public SceneBase CurrentScene { get { return GameObject.FindFirstObjectByType<SceneBase>(); } }

		public void Init()
		{
			SceneManager.LoadScene("MainScene", LoadSceneMode.Additive);
			SceneManager.sceneLoaded += OnSceneLoaded;
        }

		public void ChangeSceneServer(SceneEnum sceneEnum)
		{
			if (CurrentScene.SceneEnum == SceneEnum.Main)
			{
                /*NetworkManager.Singleton.SceneManager.UnloadScene() 메소드가
				* NetworkManager.Singleton.SceneManager.LoadScene() 를 통해서 로드된것만 Unload 할 수 있어서
				* UnloadCurrentSceneClientRPC()로 MainScene 각자 Unload해줌*/
                NetworkTransmission.instance.UnloadCurrentSceneClientRPC();
            }
			else
            {
                UnloadCurrentSceneServer();
            }
            LoadSceneServer(sceneEnum, LoadSceneMode.Additive);

            NetworkTransmission.instance.OnSceneChangeStartedServerRPC(sceneEnum);
        }
		public void ChangeScene(SceneEnum sceneEnum)
		{
			UnloadCurrentSceneServer();

            OnSceneChangeStarted(sceneEnum);
        }
		public void LoadSceneServer(SceneEnum sceneEnum, LoadSceneMode mode)
		{
			CurrentScene?.Clear();
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.SceneManager?.LoadScene(sceneEnum.ToString() + "Scene", mode);
            }
			else
            {
                SceneManager.LoadScene(sceneEnum.ToString() + "Scene", mode);
            }
		}
		public void OnSceneChangeStarted(SceneEnum sceneEnum)
		{
            UIManager.Instance.OnSceneChangeStarted(sceneEnum);
            SunManager.Instance.OnSceneChangeStarted(sceneEnum);
        }

		public void UnloadCurrentSceneServer()
		{
			if (CurrentScene.SceneEnum == SceneEnum.None) return;

			if(CurrentScene.SceneEnum == SceneEnum.Game)
			{
				SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(CurrentScene.SceneEnum.ToString() + "Scene"));
			}
			else {
				if (NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName(CurrentScene.SceneEnum.ToString() + "Scene"));
                }
				else
					SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(CurrentScene.SceneEnum.ToString() + "Scene"));
			}
		}
		public void UnloadCurrentScene()
		{
            if (CurrentScene.SceneEnum == SceneEnum.None) return;

			Debug.Log("Unload Current Scene1: " + CurrentScene);
			SceneManager.UnloadSceneAsync(CurrentScene.SceneEnum.ToString() + "Scene");
            Debug.Log("Unload Current Scene2: " + CurrentScene);
        }

		private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
		{
            UIManager.Transition.EndTransition(1f, .5f);
        }
	}
}