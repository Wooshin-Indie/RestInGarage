using Garage.Manager;
using Garage.Structs;
using IUtil;
using System.Collections.Generic;
using System.Security;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Controller
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField] private float cameraBoomLength;
        [SerializeField] private Transform target;          // 플레이어가 할당될 타겟
        [SerializeField] private float smoothSpeed = 10f;   // 카메라 이동 부드러움 정도

        [Header("Cinemachines")]
        [SerializeField] private CinemachineCamera vcamTopDown;
		[SerializeField] private List<CinemachineCamera> vcamPersonView = new();
        [SerializeField] private float characterBoomLength;

        private MapData stageData = null;
		private CinemachineBrain brain = null;

        private Vector3 standardPoint;                      // standard point를 저장해놔야됨
        private float playerRangeX;                         // x+ 방향(윗방향) 플레이어 움직임 범위

		private void Awake()
		{
			brain = GetComponent<CinemachineBrain>();
            vcamTopDown.Priority = 20;
            for (int i = 0; i < vcamPersonView.Count; i++) vcamPersonView[i].Priority = 0;
		}

		private void Start()
		{

        }

		private void Update()
		{
            if (target == null) return;

            OnUpdateCamera();
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetStageInfo(int idx)
        {
			stageData = Managers.Resource.GetData<MapData>(idx);
			standardPoint = stageData.StandardPoint;
			playerRangeX = stageData.PlayerRangeX;
            transform.rotation = Quaternion.Euler(stageData.CamRotation);
        }

        [SerializeField] private float ratio = 3;

        [Button]
        private void EndEvent()
        {
            GameManagerEx.Instance.EndEvent();

			vcamPersonView[0].Priority = 0;
		}

        private int currentViewingPersonIdx = -1;
        public void ConvertVirtualCamera(int idx)
        {
            currentViewingPersonIdx = idx;
			vcamPersonView[0].Priority = 40;
		}

        private void OnUpdateCamera()
        {
            Vector3 defaultPos = standardPoint + cameraBoomLength * (-transform.forward);
            // 플레이어 z가 +6 or -6일 때 z축 화면이동 멈춰야됨
            float zOffset = (target.position.z) / ratio;
            if (zOffset * ratio > 9)
                zOffset = 9 / ratio;
            else if (zOffset * ratio < -9)
                zOffset = -9 / ratio;

            if (target.position.x < -15) // 아래쪽
            {
                float xOffset = target.position.x > 15 ? target.position.x - 15 : target.position.x + 15;
                Vector3 desiredPosition = new Vector3(defaultPos.x + xOffset, defaultPos.y, defaultPos.z + zOffset);
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
				vcamTopDown.transform.position= smoothedPosition;
            }
            else if (target.position.x > playerRangeX) // 위쪽 상점 공간 진입
            {
                Vector3 desiredPosition = new Vector3(defaultPos.x + 13, defaultPos.y, defaultPos.z + zOffset);
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
				vcamTopDown.transform.position = smoothedPosition;
            }
            else
            {
                Vector3 desiredPosition = new Vector3(defaultPos.x, defaultPos.y, defaultPos.z + zOffset);
                if (!Mathf.Approximately(transform.position.x, defaultPos.x))
                {
                    Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
					vcamTopDown.transform.position = smoothedPosition;
                }
                else
                {
					vcamTopDown.transform.position = desiredPosition;
                }
            }

            if (stageData != null)
                vcamTopDown.transform.rotation = Quaternion.Euler(stageData.CamRotation);

            if (currentViewingPersonIdx != -1)
			{
				Transform character = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().transform;
				Vector3 cameraPos = character.position + (character.forward * characterBoomLength) + Vector3.up * 2.5f;

				vcamPersonView[0].transform.position = cameraPos;
				vcamPersonView[0].transform.LookAt(character.position);
				vcamPersonView[0].transform.position = cameraPos + Vector3.up;

			}
		}
	}
}