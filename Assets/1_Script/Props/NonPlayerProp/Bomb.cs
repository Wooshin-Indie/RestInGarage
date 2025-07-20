using Garage.Controller;
using Garage.Manager;
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Garage.Props
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(NetworkRigidbody))]
    public class Bomb : PropBase
    {
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private LayerMask targetLayer; // 차량 및 플레이어

        private bool hasExploded = false;
        public override void Awake()
        {
            base.Awake();
            rigid.isKinematic = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasExploded) return;

            if (other.GetComponentInParent<CarController>() != null ||
                other.GetComponent<PlayerController>() != null)
                Explode();
        }

        private Coroutine moveCoroutine;
        public void StartThrowing(Vector3 startPoint, Vector3 endPoint, float height, float duration)
        {
            moveCoroutine = StartCoroutine(MoveCoroutine(startPoint, endPoint, height, duration));
        }
        private IEnumerator MoveCoroutine(Vector3 startPoint, Vector3 endPoint, float height, float duration)
        {
            // 포물선의 정점(조절점) 계산
            Vector3 controlPoint = (startPoint + endPoint) / 2f;
            controlPoint.y += height;

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                // 시간에 따른 진행도(t) 계산 (0에서 1까지)
                float t = elapsedTime / duration;

                // 2차 베지어 곡선 공식으로 현재 위치 계산
                // B(t) = (1-t)²P₀ + 2(1-t)tP₁ + t²P₂
                Vector3 currentPos = Mathf.Pow(1 - t, 2) * startPoint +
                                     2 * (1 - t) * t * controlPoint +
                                     Mathf.Pow(t, 2) * endPoint;

                transform.position = currentPos;

                elapsedTime += Time.deltaTime;
                yield return null; // 다음 프레임까지 대기
            }

            // 목표 지점에 정확히 위치시키고 폭발
            transform.position = endPoint;
            Explode();
        }

        private void Explode()
        {
            if (hasExploded) return;

            // 폭발 로직이 시작되면, 가장 먼저 이동 코루틴을 중지시킵니다.
            // null 체크를 통해 코루틴이 이미 끝났거나 시작되지 않은 경우의 오류를 방지합니다.
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }

            hasExploded = true;
            OnBombExplodeClientRPC();

            Collider[] hitCars = Physics.OverlapSphere(transform.position, explosionRadius, targetLayer);

            foreach (Collider hitCar in hitCars)
            {
                CarController car = hitCar.GetComponentInParent<CarController>();
                if (car != null)
                {
                    car.OnFired();
                }

                // 폭발 시 살짝 차량이 들뜨게 하려함
                Rigidbody carRigid = hitCar.GetComponent<Rigidbody>();
                if (carRigid != null)
                {
                    // AddExplosionForce는 폭발 효과를 아주 쉽게 구현하게 해줍니다.
                    // carRigid.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }
            }
            Debug.Log("Bomb Exploded");

            GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject);
        }
        [ClientRpc]
        private void OnBombExplodeClientRPC()
        {
            VFXManager.Instance.PlayVFX(VFXType.BombExplosion, transform.position);
            Managers.Sound.PlaySfx(SFXType.Boom);
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}