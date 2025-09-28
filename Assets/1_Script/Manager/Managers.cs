using Manager;
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

			_resource.Init();
			_scene.Init();
			_sound.Init();
			_input.Init();
			_data.Init();
			_record.Init();
			_spawn.Init();
        }

		private void Start()
		{
			_spawn.Start();
			_record.Start();
		}

		private static ResourceManager _resource = new ResourceManager();
		private static SceneManagerEx _scene = new SceneManagerEx();
		private static InputManager _input = new InputManager();
		private static DataManager _data = new DataManager();
		private static SoundManager _sound = new SoundManager();
		private static SpawnManager _spawn = new SpawnManager();
		private static RuntimeRecordManager _record = new RuntimeRecordManager();



		public static ResourceManager Resource => _resource;
		public static SceneManagerEx Scene => _scene;
		public static InputManager Input => _input;
		public static DataManager Data => _data;
		public static SoundManager Sound => _sound;
		public static SpawnManager Spawn => _spawn;	
		public static RuntimeRecordManager Record => _record;
	}
}