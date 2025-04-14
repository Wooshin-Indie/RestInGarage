using Garage.Structs;
using Garage.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Garage.Manager
{
	public class SceneManagerEx
	{
		public SceneBase CurrentScene { get { return GameObject.FindFirstObjectByType<SceneBase>(); } }

		public void Init()
		{
			LoadScene(SceneEnum.Main);
		}
		public void ChangeScene(SceneEnum sceneEnum)
		{
			Debug.Log("CHANGE SCENE");
			UnloadCurrentScene();
			LoadScene(sceneEnum);
			UIManager.Instance.OnSceneChanged(sceneEnum);
		}

		private async void LoadScene(SceneEnum sceneEnum)
		{
			CurrentScene?.Clear();
			await SceneManager.LoadSceneAsync(sceneEnum.ToString() + "Scene", LoadSceneMode.Additive);
		}

		public async void UnloadCurrentScene()
		{
			if (CurrentScene.SceneEnum == SceneEnum.None) return;
			await SceneManager.UnloadSceneAsync(CurrentScene.SceneEnum.ToString() + "Scene");
		}

		
	}
}