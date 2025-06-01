using DG.Tweening;
using Garage.Structs;
using Garage.Utils;
using Unity.Netcode;
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
			SceneManager.sceneLoaded += ((scene, sceneMode) => {
				UIManager.Transition.EndTransition(1f, .5f);
				});
		}

		public void ChangeSceneServer(SceneEnum sceneEnum)
		{
			UnloadCurrentSceneServer();
			LoadSceneServer(sceneEnum);
			UIManager.Instance.OnSceneChanged(sceneEnum);
			SunManager.Instance.OnSceneChanged(sceneEnum);
		}

		public void ChangeScene(SceneEnum sceneEnum)
		{
			UnloadCurrentSceneServer();

			UIManager.Instance.OnSceneChanged(sceneEnum);
			SunManager.Instance.OnSceneChanged(sceneEnum);
		}
		public void LoadSceneServer(SceneEnum sceneEnum)
		{
			CurrentScene?.Clear();
			if (sceneEnum == SceneEnum.Main)
			{
				SceneManager.LoadScene("MainScene", LoadSceneMode.Additive);
			}
			else
			{
				NetworkManager.Singleton.SceneManager.LoadScene(sceneEnum.ToString() + "Scene", LoadSceneMode.Additive);
            }
		}

		public void UnloadCurrentSceneServer()
		{
			if (CurrentScene.SceneEnum == SceneEnum.None) return;

			if(CurrentScene.SceneEnum == SceneEnum.Main)
			{
				SceneManager.UnloadSceneAsync("MainScene");
			}
			else{
				NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName(CurrentScene.SceneEnum.ToString() + "Scene"));
			}
		}

		
	}
}