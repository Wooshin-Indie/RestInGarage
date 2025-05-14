using UnityEngine;
using Unity.Mathematics;
using DG.Tweening;
using Garage.Utils;
using Unity.Netcode;
using IUtil;
using UnityEditor.UIElements;

namespace Garage.Manager
{
	public enum TimePhase
	{
		Night,   
		Morning, 
		Afternoon
	}
	public class SunManager : NetworkBehaviour
    {
        
        [SerializeField] private float sunIntensity;
        [SerializeField] private Color dayColour;
        [SerializeField] private Color eveningColour;
        [SerializeField] private Vector3 startPos;
        [SerializeField] private Vector3 mainStartRot;
        [SerializeField] private Vector3 mainEndRot;

        [Header("Lobby Light")]
		[SerializeField] private float rotationSpeed = 0.1f;

		[ReadOnly] public float currentTime = 0f;
		private float targetTime = 0.5f;

		private Light light;
        private SceneEnum curScene;

        #region Singleton
        private static SunManager instance;
        public static SunManager Instance { get => instance; }

        void Awake()
        {
            Init();
            light = GetComponent<Light>();
            OnSceneChanged(SceneEnum.Main);
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
        }
        #endregion

        private void Update()
        {
            OnUpdateLightSetting();

			if (curScene == SceneEnum.Lobby)
			{
				if (Mathf.Abs(currentTime - targetTime) > 0.001f)
				{
					float delta = (targetTime - currentTime + 1f) % 1f;
					float direction = rotationSpeed >= 0 ? 1f : -1f;
					float step = rotationSpeed * Time.deltaTime;

					float move = Mathf.Abs(step);
					if (move >= delta)
					{
						currentTime = targetTime;
					}
					else
					{
						currentTime = (currentTime + direction * move + 1f) % 1f;
					}
				}

				float fullRotation = Mathf.Lerp(0f, 360f, (currentTime + 0.75f) % 1f);
				transform.localEulerAngles = new Vector3(fullRotation, 0f, 0f);
			}
		}

		private void OnUpdateLightSetting()
        {
            float dotProduct = Vector3.Dot(-transform.forward, Vector3.up);
            float clampedDot = Mathf.Clamp((dotProduct + 0.9f), 0, 1);
            float topDot = (1 - Mathf.Clamp01(dotProduct)) * Mathf.Clamp01(Mathf.Sign(dotProduct));
            float bottomDot = (1 - Mathf.Clamp01(-dotProduct)) * Mathf.Clamp01(Mathf.Sign(-dotProduct));
            topDot = Mathf.Pow(math.smoothstep(0f, 0.9f, topDot), 5);
            bottomDot = Mathf.Pow(bottomDot, 5);

            light.intensity = Mathf.Lerp(0.1f, sunIntensity, Mathf.Pow(clampedDot, 5));
            light.color = Color.Lerp(dayColour, eveningColour, topDot + bottomDot);

            RenderSettings.ambientIntensity = Mathf.Lerp(1f, 1.7f, Mathf.Pow(clampedDot, 5));
        }
        public void OnSceneChanged(SceneEnum scene)
        {
            curScene = scene;
            switch (scene)
            {
                case SceneEnum.None:
                    break;
                case SceneEnum.Main:
                    transform.position = startPos; // 처음 위치
                    transform.localEulerAngles = mainStartRot; // 처음 각도
                    transform.DORotate(mainEndRot, 600f);
                    break;
                case SceneEnum.Lobby:
                    transform.DOKill();
                    break;
            }
        }

        public void OnChangedToNight()
		{
			currentTime = targetTime = 0f;
		}
		public void OnChangedToMorning()
		{
			currentTime = targetTime = .33f;
		}

		private TimePhase curTimePhase = TimePhase.Night;
        public void SetTimePhase(TimePhase phase, float time)
		{
			switch (phase)
			{
				case TimePhase.Night:
                    SetTimeTarget(0f, currentTime > .5f ? time : -time);
					break;
				case TimePhase.Morning:
                    SetTimeTarget(.33f, time);
					break;
				case TimePhase.Afternoon:
                    SetTimeTarget(.66f, time);
                    break;
			}
			curTimePhase = phase;
		}
		private void SetTimeTarget(float newTime, float time)
		{
			targetTime = Mathf.Repeat(newTime, 1f);
            rotationSpeed = .33f / time;
		}
	}
}

