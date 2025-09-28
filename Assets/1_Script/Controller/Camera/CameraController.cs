using Garage.Manager;
using Garage.Structs;
using System.Collections.Generic;
using System.Linq;
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

        private Dictionary<ulong, CinemachineCamera> vCameras = new();
		private CinemachineCamera currentVCam = null;
        private ulong currentPlayerNetId = ulong.MaxValue;

		private void Awake()
		{
			brain = GetComponent<CinemachineBrain>();
            vcamTopDown.Priority = 20;
            for (int i = 0; i < vcamPersonView.Count; i++) vcamPersonView[i].Priority = 0;
		}

		private void OnEnable()
		{
			var clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();
			for (int i = 0; i < clientIds.Count; i++)
            {
                vCameras[clientIds[i]] = vcamPersonView[i];
            }
		}

		private void OnDisable()
		{
            vCameras.Clear();
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

        public void EndEvent()
        {
            GameManagerEx.Instance.EndEvent();

			vcamPersonView[0].Priority = 0;
		}

        public void ConvertVirtualCamera(ulong netId)
        {
            if (currentVCam != null)
                currentVCam.Priority = 0;
            currentVCam = vCameras[netId];
            currentPlayerNetId = netId;

			currentVCam.Priority = 40;
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

            if (currentPlayerNetId != ulong.MaxValue)
			{
                Transform character = null;
				foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
				{
					var playerObj = client.PlayerObject;
					if (playerObj != null && playerObj.OwnerClientId == currentPlayerNetId)
					{
						character = playerObj.transform;
					}
				}
				Vector3 cameraPos = character.position + (character.forward * characterBoomLength) + Vector3.up * 2.5f;

				if (currentVCam != null)
				{
					currentVCam.transform.position = cameraPos;
					currentVCam.transform.LookAt(character.position);
					currentVCam.transform.position = cameraPos + Vector3.up;
				}
			}
		}
	}
}