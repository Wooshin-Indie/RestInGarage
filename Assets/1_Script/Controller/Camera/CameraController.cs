using DG.Tweening;
using Garage.Manager;
using Garage.Structs;
using System.Security.Cryptography;
using UnityEditor.Overlays;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Garage.Controller
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField] private float cameraBoomLength;
		[SerializeField] private Vector3 fixedRotation;
        [SerializeField] private Transform target; // 플레이어가 할당될 타겟
        [SerializeField] private float smoothSpeed = 10f; // 카메라 이동 부드러움 정도
        private StageData curStageData;
        private float playerRangeX; // x+ 방향(윗방향) 플레이어 움직임 범위

        private void Awake()
        {
            curStageData = TrafficManager.Instance.CurStageData;
            playerRangeX = curStageData.PlayerRangeX;
            TrafficManager.Instance.OnStageStarted += SetStageInfo;
        }

        private void Update()
		{
            if (target == null) return;

            OnUpdateCamera1();
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void SetStageInfo(StageData stageData)
        {
            if (curStageData.PlayerRangeX != stageData.PlayerRangeX)
            {
                // TODO - 차선 개수 따라서 카메라를 가운데쪽으로 lerp
            }

            curStageData = stageData;
            playerRangeX = curStageData.PlayerRangeX;
        }

        // 원래 카메라 작동방식, 카메라가 플레이어의 자식에 있어야 함
        private void OnUpdateTopDownCamera()
		{
            float phi = fixedRotation.y * Mathf.Deg2Rad;
            float theta = fixedRotation.x * Mathf.Deg2Rad;

            float x = cameraBoomLength * Mathf.Cos(theta) * Mathf.Sin(phi);
            float y = cameraBoomLength * Mathf.Sin(theta);
            float z = -cameraBoomLength * Mathf.Cos(theta) * Mathf.Cos(phi);

            transform.position = transform.parent.position + new Vector3(x, y, z);
            transform.rotation = Quaternion.Euler(fixedRotation);
        }

        [SerializeField] private Vector3 defaultTestPos;
        private Vector3 UpperAreaPos;
        private void OnUpdateCameraTest()
        { // 카메라 각도, 위치 테스트 후 bake하는 용도
            Vector3 cameraBackward = -transform.forward;

            transform.rotation = Quaternion.Euler(fixedRotation);
            transform.position = defaultTestPos + cameraBoomLength * cameraBackward;
        }

        private void OnUpdateCamera()
        {
            float cameraBoomLength = 20;
            Vector3 standardPoint = new Vector3(-5, 0, 3); // standard point를 저장해놔야됨
            Vector3 defaultPos = standardPoint + cameraBoomLength * (- transform.forward);
            // 플레이어 z가 +6 or -6일 때 z축 화면이동 멈춰야됨
            float zOffset = target.position.z - 2;
            if (zOffset > 6)
                zOffset = 6;
            else if (zOffset < -6)
                zOffset = -6;

            if (target.position.x < -15) // 아래쪽
            {
                float xOffset = target.position.x > 15 ? target.position.x - 15 : target.position.x + 15;
                Vector3 desiredPosition = new Vector3(defaultPos.x + xOffset, defaultPos.y, defaultPos.z + zOffset);
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                transform.position = smoothedPosition;
            }
            else if (target.position.x > playerRangeX) // 위쪽 상점 공간 진입
            {
                Vector3 desiredPosition = new Vector3(defaultPos.x + 13, defaultPos.y, defaultPos.z + zOffset);
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                transform.position = smoothedPosition;
            }
            else
            {
                Vector3 desiredPosition = new Vector3(defaultPos.x, defaultPos.y, defaultPos.z + zOffset);
                if (!Mathf.Approximately(transform.position.x, defaultPos.x))
                {
                    Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                    transform.position = smoothedPosition;
                }
                else
                {
                    transform.position = desiredPosition;
                }
            }
        }
        [SerializeField] private float ratio = 3;
        private void OnUpdateCamera1()
        {
            float cameraBoomLength = 20;
            Vector3 standardPoint = new Vector3(-5, 0, 3); // standard point를 저장해놔야됨
            Vector3 defaultPos = standardPoint + cameraBoomLength * (-transform.forward);
            // 플레이어 z가 +6 or -6일 때 z축 화면이동 멈춰야됨
            float zOffset = (target.position.z - 2) / ratio;
            if (zOffset * ratio > 6)
                zOffset = 6 / ratio;
            else if (zOffset * ratio < -6)
                zOffset = -6 / ratio;

            if (target.position.x < -15) // 아래쪽
            {
                float xOffset = target.position.x > 15 ? target.position.x - 15 : target.position.x + 15;
                Vector3 desiredPosition = new Vector3(defaultPos.x + xOffset, defaultPos.y, defaultPos.z + zOffset);
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                transform.position = smoothedPosition;
            }
            else if (target.position.x > playerRangeX) // 위쪽 상점 공간 진입
            {
                Vector3 desiredPosition = new Vector3(defaultPos.x + 13, defaultPos.y, defaultPos.z + zOffset);
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                transform.position = smoothedPosition;
            }
            else
            {
                Vector3 desiredPosition = new Vector3(defaultPos.x, defaultPos.y, defaultPos.z + zOffset);
                if (!Mathf.Approximately(transform.position.x, defaultPos.x))
                {
                    Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                    transform.position = smoothedPosition;
                }
                else
                {
                    transform.position = desiredPosition;
                }
            }
        }
	}
}