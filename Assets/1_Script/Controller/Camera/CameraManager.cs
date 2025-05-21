using Garage.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Garage.Controller
{
    public class CameraManager : MonoBehaviour
    {
        private CameraController mainCam;

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
            GameObject mainCameraOb = GameObject.FindWithTag("MainCamera");
            if (mainCameraOb != null)
            {
                mainCam = mainCameraOb.GetComponent<CameraController>();
                if (mainCameraOb == null)
                    Debug.Log("Can't find 'CameraController' in MainCamera");
            }
            else
            {
                Debug.LogError("Can't find 'MainCamera' object");
            }
        }

        public void SetPlayerCameraTarget(Transform playerTransform)
        {
            if (mainCam != null)
            {
                mainCam.SetTarget(playerTransform);
            }
            else
            {
                // 한번 더 찾아봄
                FindAndAssignCameraInCurrentScene();
                if (mainCam != null)
                {
                    mainCam.SetTarget(playerTransform);
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
