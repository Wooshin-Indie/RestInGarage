using Garage.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Garage.Controller
{
    public class CameraManager : MonoBehaviour
    {
        private CameraController mainCam = null;
        private Transform targetTransform = null;

        #region Singleton
        private static CameraManager instance;
        public static CameraManager Instance { get => instance; }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (null == instance)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);

                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
        #endregion

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"New Scene Loaded: {scene.name}. Find Camera Again.");
            FindAndAssignCameraInCurrentScene();
        }

        // 현재 씬에서 메인 카메라를 찾아 CameraController 스크립트 참조를 할당하는 메소드
        private void FindAndAssignCameraInCurrentScene()
        {
            //GameObject mainCameraOb = GameObject.FindWithTag("MainCamera");
            GameObject mainCameraOb = Camera.main != null ? Camera.main.gameObject : null;

            if (mainCameraOb != null)
            {
                mainCam = mainCameraOb.GetComponent<CameraController>();
                if (mainCam != null)
                {
                    Debug.Log("Detected 'CameraController' in MainCamera: " + mainCam);
                    mainCam.SetTarget(targetTransform);
                }
                else if (mainCam == null)
                {
                    Debug.Log("Can't find 'CameraController' in MainCamera");
                }

                if (targetTransform == null)
                    Debug.Log("CurrentTarget: null");
                else
                    Debug.Log("CurrentTarget: " + targetTransform);
            }
            else
            {
                Debug.LogError("Can't find 'MainCamera' object");
            }
        }

        public void SetTargetPlayer(Transform playerTransform)
        {
            targetTransform = playerTransform;

            if (mainCam != null)
            {
                mainCam.SetTarget(targetTransform);
            }
            else
            {
                // 한번 더 찾아봄
                FindAndAssignCameraInCurrentScene();
                if (mainCam != null)
                {
                    mainCam.SetTarget(targetTransform);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }
    }
}
