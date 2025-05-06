using System.Resources;
using UnityEngine;

namespace Garage.Manager
{
	public class Managers : MonoBehaviour
	{
		private static Managers instance;
		public static Managers Instance { get => instance; }

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

			_scene.Init();
			_input.Init();
		}

		private static ResourceManager _resource = new ResourceManager();
		private static SceneManagerEx _scene = new SceneManagerEx();
		private static InputManager _input = new InputManager();


		public static ResourceManager Resource { get => _resource; }
		public static SceneManagerEx Scene { get => _scene; }
		public static InputManager Input { get => _input; }
	}
}