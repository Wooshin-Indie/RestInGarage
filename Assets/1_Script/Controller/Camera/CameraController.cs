using DG.Tweening;
using System.Security.Cryptography;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Garage.Controller
{
	public class CameraController : MonoBehaviour
	{
		[SerializeField] private float cameraBoomLength;
		[SerializeField] private Vector3 fixedRotation = new Vector3(75f, 0f, 0f);
        [SerializeField] private Transform target; // 플레이어가 할당될 타겟
        [SerializeField] private float smoothSpeed = 10f; // 카메라 이동 부드러움 정도

        private void Update()
		{
            if (target == null) return;

            OnUpdateCameraTest1();
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
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

        [SerializeField] private Vector3 defaultPos;
        private void OnUpdateCameraTest1()
        {
            Vector3 cameraBackward = -transform.forward;
            transform.rotation = Quaternion.Euler(fixedRotation);
            transform.position = defaultPos + cameraBoomLength * cameraBackward;
            
            if (target.position.x > 15 || target.position.x < -15)
            {
                float xOffset = target.position.x > 15 ? target.position.x - 15 : target.position.x + 15;
                Vector3 tp = transform.position;
                Vector3 desiredPosition = new Vector3(tp.x + xOffset / 2, tp.y, tp.z);
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                transform.position = smoothedPosition;
            }
        }
	}
}