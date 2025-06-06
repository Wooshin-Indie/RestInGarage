using Garage.Manager;
using Garage.Structs;
using UnityEngine;

namespace Garage.Controller
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField] private float cameraBoomLength;
        [SerializeField] private Transform target; // 플레이어가 할당될 타겟
        [SerializeField] private float smoothSpeed = 10f; // 카메라 이동 부드러움 정도
        private Vector3 standardPoint;  // standard point를 저장해놔야됨
        private StageData curStageData;
        private float playerRangeX; // x+ 방향(윗방향) 플레이어 움직임 범위

        private void Awake()
        {
            curStageData = TrafficManager.Instance.CurStageData;
            standardPoint = curStageData.StandardPoint;
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
            transform.rotation = Quaternion.Euler(curStageData.CamRotation);
            playerRangeX = curStageData.PlayerRangeX;
        }

        [SerializeField] private float ratio = 3;

        private void OnUpdateCamera1()
        {
            float cameraBoomLength = 20;
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