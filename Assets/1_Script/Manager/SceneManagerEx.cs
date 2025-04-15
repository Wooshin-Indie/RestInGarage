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
		}
		public void ChangeSceneServer(SceneEnum sceneEnum)
		{
			Debug.Log("CHANGE SCENE");
			UnloadCurrentSceneServer();
			LoadSceneServer(sceneEnum);
			UIManager.Instance.OnSceneChanged(sceneEnum);
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
				Debug.Log("SCENE LOAD :" + sceneEnum.ToString());
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