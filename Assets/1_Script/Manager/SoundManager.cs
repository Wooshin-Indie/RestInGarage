using DG.Tweening;
using Garage.Utils;
using IUtil;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Garage.Manager
{
	public enum AMBType { Wind, Engine }
	public enum BGMType { None }
	public enum SFXType { None, Walk, Wrench, Hammer, FireEx, Put, Whoosh, Glug, Tape, Pop,
		EarnMoney, UseMoney, Alarm, StartUp, Complete, Horn1, Horn2, Wrong, SlideUp, SlideDown,
		Tick }
	public enum SoundType { Bgm, Sfx }
	/// <summary>
	/// 소리를 내는 매니저입니다.
	/// 게임 실행 시 동적으로 AudioSource를 생성합니다.
	/// audioClip 은 ResourceManager에서 동적으로 로드합니다.
	/// </summary>
	public class SoundManager : MonoBehaviour
	{
		private AudioSource[] audioSources = new AudioSource[(int)Enum.GetNames(typeof(SoundType)).Length];
		private AudioSource[] ambientSources = new AudioSource[(int)Enum.GetNames(typeof(AMBType)).Length];

		private float masterVolume = 1f;
		private float sfxVolume = 1f;
		private float bgmVolume = 1f;


		/** Properties **/
		public float SfxVolume
		{
			get { return sfxVolume; }
			set
			{
				sfxVolume = value;
			}
		}
		public float BgmVolume
		{
			get { return bgmVolume; }
			set
			{
				bgmVolume = value;
			}
		}
		public float MasterVolume
		{
			get => masterVolume;
			set
			{
				masterVolume = value;
			}
		}

		private static SoundManager instance;
		public static SoundManager Instance { get => instance; }

		public void Init()
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

			string[] soundTypeNames = Enum.GetNames(typeof(SoundType));
			string[] ambientTypeNames = Enum.GetNames(typeof(AMBType));

			for (int i = 0; i < soundTypeNames.Length; i++)
			{
				GameObject go = new GameObject { name = soundTypeNames[i] };
				audioSources[i] = go.AddComponent<AudioSource>();
				go.transform.parent = instance.transform;
			}

			for (int i = 0; i < ambientTypeNames.Length; i++)
			{
				GameObject go = new GameObject { name = ambientTypeNames[i] };
				ambientSources[i] = go.AddComponent<AudioSource>();
				go.transform.parent = instance.transform;
				ambientSources[i].loop = true;
				ambientSources[i].clip = GetAudioClip((AMBType)i);
			}

			audioSources[(int)SoundType.Bgm].loop = true;
			InitPool();
		}

		private void Awake()
		{
			Init();
		}

		private void Start()
		{
			PlayAmbient(AMBType.Engine);
			PlayAmbient(AMBType.Wind);
		}

		#region Object Pooling

		private Transform poolRoot;
		private Stack<AudioSource> poolingAudios = new Stack<AudioSource>();

		private void InitPool(int cnt = 10)
		{
			poolRoot = new GameObject { name = "_poolRoot" }.transform;
			poolRoot.parent = instance.transform;

			for (int i = 0; i < cnt; i++)
			{
				poolingAudios.Push(Create());
			}
		}

		private AudioSource Create()
		{
			GameObject go = new GameObject { name = "PoolableAudio" };
			go.AddComponent<AudioSource>();
			go.transform.parent = poolRoot;
			go.gameObject.SetActive(false);
			return go.GetComponent<AudioSource>();
		}
		private void Push(AudioSource source)
		{
			source.gameObject.SetActive(false);
			poolingAudios.Push(source);
		}
		private AudioSource Pop()
		{
			AudioSource source;
			if (poolingAudios.Count == 0) source = Create();
			else source = poolingAudios.Pop();

			source.gameObject.SetActive(true);
			return source;
		}

		private async Task PushAfterDelay(AudioSource source, float delay)
		{
			await Task.Delay((int)(delay * 1000));
			Push(source);
		}


		#endregion

		public void PlaySfx(SFXType sfxType)
		{
			if (sfxType == SFXType.None) return;

			PlaySfx(sfxType, 1f);
		}

		public void PlaySfx(SFXType sfxType, float volume)
		{
			if (sfxType == SFXType.None) return;

			audioSources[(int)SoundType.Sfx].PlayOneShot(GetAudioClip(sfxType), volume * sfxVolume * masterVolume);
		}

		public AudioSource PlaySfx(SFXType sfxType, float volume, float pitch)
		{
			if (sfxType == SFXType.None) return null;

			AudioSource audioSource = Pop();
			audioSource.clip = GetAudioClip(sfxType);
			audioSource.pitch = pitch;
			audioSource.volume = volume * masterVolume * sfxVolume;
			audioSource.Play();
			PushAfterDelay(audioSource, audioSource.clip.length);
			return audioSource;
		}

		public AudioSource PlaySfx(SFXType sfxType, float volume, float pitch, float duration)
		{
			if (sfxType == SFXType.None) return null;

			AudioSource audioSource = Pop();
			audioSource.clip = GetAudioClip(sfxType);
			audioSource.pitch = pitch;
			audioSource.volume = volume * masterVolume * sfxVolume;
			audioSource.Play();
			PushAfterDelay(audioSource, Mathf.Min(audioSource.clip.length, duration));
			return audioSource;
		}

		private AudioClip GetAudioClip(SFXType type)
		{
			return Resources.Load<AudioClip>(Constants.PATH_SFX + type.ToString());
		}
		private AudioClip GetAudioClip(AMBType type)
		{
			return Resources.Load<AudioClip>(Constants.PATH_AMB + type.ToString());	
		}


		public void PlayAmbient(AMBType type)
		{
			ambientSources[(int)type].Play();
		}
		public void StopAmbient(AMBType type)
		{
			ambientSources[(int)type].Stop();
		}

		public void BlockBGM(float duration)
		{
			for(int i=0; i<ambientSources.Length; i++)
			{
				ambientSources[i].DOFade(0f, duration);
			}
			audioSources[(int)SoundType.Bgm].DOFade(0f, duration);
		}

		public void ReleaseBGM(float duration)
		{
			switch (Managers.Scene.CurrentScene.SceneEnum) {
				case SceneEnum.Main:
					PlayAmbient(AMBType.Engine);
					break;
				case SceneEnum.Lobby:
					StopAmbient(AMBType.Engine);
					break;
			}
			for (int i = 0; i < ambientSources.Length; i++)
			{
				ambientSources[i].DOFade(1f, duration);
			}
			audioSources[(int)SoundType.Bgm].DOFade(1f, duration);
		}




		[Header("TEST")]
		public SFXType testSFXType;
		
		[Button(nameof(testSFXType))]
		private void TestSFX(SFXType type)
		{
			PlaySfx(type, 1f, 1f);
		}

	}
}