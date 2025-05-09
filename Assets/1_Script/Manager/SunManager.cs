using UnityEngine;
using Unity.Mathematics;
using DG.Tweening;
using Garage.Utils;

namespace Garage.Manager
{
    public class SunManager : MonoBehaviour
    {
        
        [SerializeField] private float sunIntensity;
        [SerializeField] private Color dayColour;
        [SerializeField] private Color eveningColour;
        [SerializeField] private Vector3 startPos;
        [SerializeField] private Vector3 mainStartRot;
        [SerializeField] private Vector3 mainEndRot;
        [SerializeField] private Vector3 lobbyDayRot;
        [SerializeField] private Vector3 lobbyNightRot;
        private Light light;
        private SceneEnum curScene;

        #region Singleton
        private static SunManager instance;
        public static SunManager Instance { get => instance; }

        void Awake()
        {
            Init();
            light = GetComponent<Light>();
            light.transform.position = startPos; // 처음 위치
            light.transform.localEulerAngles = mainStartRot; // 처음 각도
            light.transform.DORotate(mainEndRot, 600f);
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
            if (curScene == SceneEnum.Main)
                OnUpdateMainScene();
        }

        private void OnUpdateMainScene()
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
                    light.transform.position = startPos; // 처음 위치
                    light.transform.localEulerAngles = mainStartRot; // 처음 각도
                    light.transform.DORotate(mainEndRot, 600f);
                    break;
                case SceneEnum.Lobby:
                    light.transform.DOKill();
                    OnChangedToDay();
                    break;
                case SceneEnum.Game:
                    break;
            }
            
        }

        public void OnChangedToDay()
        {
            light.transform.localEulerAngles = new Vector3(63f, -33f, 0f);
            light.intensity = 1.5f;
            RenderSettings.ambientIntensity = 1.7f;

            float dotProduct = Vector3.Dot(-transform.forward, Vector3.up);
            float topDot = (1 - Mathf.Clamp01(dotProduct)) * Mathf.Clamp01(Mathf.Sign(dotProduct));
            float bottomDot = (1 - Mathf.Clamp01(-dotProduct)) * Mathf.Clamp01(Mathf.Sign(-dotProduct));
            topDot = Mathf.Pow(math.smoothstep(0f, 0.9f, topDot), 5);
            bottomDot = Mathf.Pow(bottomDot, 5);

            light.color = Color.Lerp(dayColour, eveningColour, topDot + bottomDot);
        }

        public void OnChangedToNight()
        {
            light.transform.localEulerAngles = new Vector3(-90f, -33f, 0f);
            light.intensity = 1f;
            RenderSettings.ambientIntensity = 1f;

            float dotProduct = Vector3.Dot(-transform.forward, Vector3.up);
            float topDot = (1 - Mathf.Clamp01(dotProduct)) * Mathf.Clamp01(Mathf.Sign(dotProduct));
            float bottomDot = (1 - Mathf.Clamp01(-dotProduct)) * Mathf.Clamp01(Mathf.Sign(-dotProduct));
            topDot = Mathf.Pow(math.smoothstep(0f, 0.9f, topDot), 5);
            bottomDot = Mathf.Pow(bottomDot, 5);

            light.color = Color.Lerp(dayColour, eveningColour, topDot + bottomDot);
        }
    }
}

