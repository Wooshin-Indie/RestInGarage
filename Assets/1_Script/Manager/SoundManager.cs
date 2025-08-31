using DG.Tweening;
using Garage.Controller;
using Garage.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

namespace Garage.Manager
{
	public enum AMBType { Wind, Engine }
	public enum BGMType { None, Main1, Main2, Stage1 }
	public enum SFXType { None, Walk, Wrench, Hammer, FireEx, Put, Whoosh, Glug, Tape, Pop,
		EarnMoney, UseMoney, Alarm, StartUp, Complete, Horn1, Horn2, Wrong, SlideUp, SlideDown,
		Tick, Boom, ShopCar, ShopPop, BossWarning, Voice_ThankYou, Voice_ThrowBomb, SwingArm, 
		PropHold, PropPutdown, CarDriving }
	public enum SoundType { Bgm, Sfx }
	/// <summary>
	/// 소리를 내는 매니저입니다.
	/// 게임 실행 시 동적으로 AudioSource를 생성합니다.
	/// audioClip 은 ResourceManager에서 동적으로 로드합니다.
	/// </summary>
	public class SoundManager
	{
		private AudioSource[] audioSources = new AudioSource[(int)Enum.GetNames(typeof(SoundType)).Length];
		private AudioSource[] ambientSources = new AudioSource[(int)Enum.GetNames(typeof(AMBType)).Length];
        private AudioSource[] bgmSources = new AudioSource[(int)Enum.GetNames(typeof(BGMType)).Length];

        private float masterVolume = 1f;
		private float ambientVolume = 1f;
		private float sfxVolume = 1f;
		private float bgmVolume = 1f;

		/** Properties **/
		public float MasterVolume
		{
			get => masterVolume;
			set
			{
				masterVolume = value;
				BgmVolume = bgmVolume;
				AmbientVolume = ambientVolume;
			}
		}
		public float AmbientVolume
		{
			get => ambientVolume;
			set
			{
				ambientVolume = value;
				for (int i = 0; i < ambientSources.Length; i++)
				{
					ambientSources[i].volume = masterVolume * ambientVolume;
				}
			}
		}
		public float BgmVolume
		{
			get { return bgmVolume; }
			set
			{
				bgmVolume = value;
                for (int i = 0; i < bgmSources.Length; i++)
                {
                    bgmSources[i].volume = masterVolume * ambientVolume;
                }
			}
		}
		public float SfxVolume
		{
			get { return sfxVolume; }
			set
			{
				sfxVolume = value;
			}
		}

		Transform root = null;
		public void Init()
		{
			Transform root = new GameObject { name = "@SoundManager" }.transform;


			string[] soundTypeNames = Enum.GetNames(typeof(SoundType));
			string[] ambientTypeNames = Enum.GetNames(typeof(AMBType));
			string[] bgmTypeNames = Enum.GetNames(typeof(BGMType));

			for (int i = 0; i < soundTypeNames.Length; i++)
			{
				GameObject go = new GameObject { name = soundTypeNames[i] };
				audioSources[i] = go.AddComponent<AudioSource>();
				go.transform.parent = root;
			}

			for (int i = 0; i < ambientTypeNames.Length; i++)
			{
				GameObject go = new GameObject { name = ambientTypeNames[i] };
				ambientSources[i] = go.AddComponent<AudioSource>();
				go.transform.parent = root;
				ambientSources[i].loop = true;
				ambientSources[i].clip = GetAudioClip((AMBType)i);
			}

            for (int i = 0; i < bgmTypeNames.Length; i++)
            {
                GameObject go = new GameObject { name = bgmTypeNames[i] };
                bgmSources[i] = go.AddComponent<AudioSource>();
                go.transform.parent = root;
                bgmSources[i].loop = true;
                bgmSources[i].clip = GetAudioClip((BGMType)i);
            }

            audioSources[(int)SoundType.Bgm].loop = true;
			InitPool();

			PlayAmbient(AMBType.Engine);
			PlayAmbient(AMBType.Wind);
            PlayBGM(BGMType.Main2, 0.7f);
        }

		#region Object Pooling

		private Transform poolRoot;
		private Stack<AudioSource> poolingAudios = new Stack<AudioSource>();

		private void InitPool(int cnt = 10)
		{
			poolRoot = new GameObject { name = "_poolRoot" }.transform;
			poolRoot.parent = root;

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

		public void PlaySfx(SFXType sfxType, Action onComplete)
		{
            if (sfxType == SFXType.None) return;

            audioSources[(int)SoundType.Sfx].PlayOneShot(GetAudioClip(sfxType), sfxVolume * masterVolume);

			DOVirtual.DelayedCall(GetAudioClip(sfxType).length, () => {
				onComplete?.Invoke();
				});
        }

        private AudioClip GetAudioClip(SFXType type)
		{
			return Resources.Load<AudioClip>(Constants.PATH_SFX + type.ToString());
		}
		private AudioClip GetAudioClip(AMBType type)
		{
			return Resources.Load<AudioClip>(Constants.PATH_AMB + type.ToString());	
		}
        private AudioClip GetAudioClip(BGMType type)
        {
            return Resources.Load<AudioClip>(Constants.PATH_BGM + type.ToString());
        }


        public void PlayAmbient(AMBType type)
		{
			ambientSources[(int)type].Play();
		}
		public void StopAmbient(AMBType type)
		{
			ambientSources[(int)type].Stop();
		}

		public void PlayBGM(BGMType type, float volume)
		{
			bgmSources[(int)type].volume = volume * masterVolume * bgmVolume;
            bgmSources[(int)type].Play();
        }
        public void StopBGM(BGMType type)
        {
			bgmSources[(int)type].Stop();
        }
        public void StopBGM(BGMType type, float fadeDuration)
		{
			bgmSources[(int)type].DOFade(0f, fadeDuration)
				.OnComplete(() => bgmSources[(int)type].Stop());
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
					PlayBGM(BGMType.Main2, 0.7f);
					break;
				case SceneEnum.Game:
					StopAmbient(AMBType.Engine);
                    StopBGM(BGMType.Main2);
                    break;
			}
			for (int i = 0; i < ambientSources.Length; i++)
			{
				ambientSources[i].DOFade(1f, duration);
			}
			audioSources[(int)SoundType.Bgm].DOFade(1f, duration);
		}

        #region CarSfx
        private Dictionary<CarController, AudioSource> carDrivingDict = new Dictionary<CarController, AudioSource>();
		/// <summary>
		/// 차량 생성될 때 개별 Sfx 할당해줌
		/// </summary>
		private float originMinDistance = 0f;
		private float originMaxDistance = 0f;
		public void InitCarDrivingSfx(CarController car)
		{
            AudioSource audioSource = Pop();
            audioSource.clip = GetAudioClip(SFXType.CarDriving);
			audioSource.spatialBlend = 0.7f;
			audioSource.minDistance = 10f;
			audioSource.maxDistance = 30f;

			audioSource.transform.SetParent(car.transform, false);

            carDrivingDict.Add(car, audioSource);
        }
        public void PlayCarDrivingSfx(CarController car, float volume, float pitch)
        {
            if (!carDrivingDict.ContainsKey(car)) return;

            AudioSource audioSource = carDrivingDict[car];

			audioSource.DOKill();
            audioSource.pitch = pitch;
            audioSource.volume = volume * masterVolume * sfxVolume;
            audioSource.Play();
			Debug.Log("SFX Play: CarDriving");
        }
		public void StopCarDrivingSfx(CarController car)
        {
            if (!carDrivingDict.ContainsKey(car)) return;

            AudioSource audioSource = carDrivingDict[car];

            audioSource.DOFade(0f, 0.1f).
				OnComplete(() => audioSource.Stop());
        }
		/// <summary>
		/// 차량 삭제될 때 호출해서 딕셔너리에서 없애주기
		/// </summary>
		public void RemoveCarDrivingSfxInDict(CarController car)
		{
            if (!carDrivingDict.ContainsKey(car)) return;

            AudioSource audioSource = carDrivingDict[car];

            audioSource.spatialBlend = 0f;

            audioSource.transform.SetParent(poolRoot);
            Push(audioSource);
            carDrivingDict.Remove(car);
        }
        #endregion
    }
}