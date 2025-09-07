using Garage.Utils;
using UnityEngine;
using Garage.Controller;
using System.Collections.Generic;
using Garage.Props;
using Unity.Netcode;
using Garage.Manager;
using UnityEngine.EventSystems;

namespace Garage.Vehicle
{
    public class BikerGang : BossVehicle
    {
        [SerializeField] private float velocity;
        [SerializeField] private float zPosRangeToThrowBomb; // 폭탄을 던지기 시작할 맵의 Z좌표 범위
        [SerializeField] private float carDetectingRange; // 폭탄 던지기 전 랜덤차량 받아올 때 차량탐색 범위
        [SerializeField] private LayerMask carLayer;
        [SerializeField] private GameObject bombPrefab;
        [SerializeField] private Transform smokeSpotTf;
        private bool hasBomb;
        private bool bombThrew;
        private float throwingBombPosZ;
        public override void Awake()
        {
            base.Awake();
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Init(Utils.Utility.Chance(1f));
        }

        public void Init(bool hasBomb)
        {
            if (!IsHost) return;

            IsKnockbackablePlayerOnCollision = true;

            this.hasBomb = hasBomb;
            if (hasBomb)
            {
                throwingBombPosZ = Random.Range(-zPosRangeToThrowBomb, zPosRangeToThrowBomb);
            }
            PlaySmokeVFXClientRPC();
        }

        private void FixedUpdate()
        {
            if (!IsHost) return;

            if (IsOutOfBoundary())
            {
                Despawn();
            }

            Move(-Vector3.forward, velocity);

            if (!hasBomb) return;

            // 폭탄 던지는 위치에 가까워졌고 폭탄 아직 안던졌으면 차량찾아서 폭탄던지기
            if (transform.position.z.Approximately(throwingBombPosZ, 0.3f) && !bombThrew)
            {
                randomTargetCar = FindRandomCarInRange(carDetectingRange);

                if (randomTargetCar == null)
                {
                    return;
                }

                ThrowBomb(randomTargetCar);
                OnThrowBombClientRPC();
                bombThrew = true;
            }
        }

        [SerializeField] private float knockbackForce = 20f;
        private void OnCollisionEnter(Collision collision)
        {
            OnCollisionWithPlayer(collision, knockbackForce);
            // 플레이어하고 충돌할 때만 
            if (collision.gameObject.CompareTag("Player"))
            {
                rigid.isKinematic = true;
            }
        }
        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                rigid.isKinematic = false;
            }
        }

        public override void Move(Vector3 direction, float velocity)
        {
            // TODO - 달려올 때 차량 소리 SFX
            if (rigid.isKinematic)
            {
                Vector3 targetPos = transform.position + direction * velocity * Time.fixedDeltaTime;
                rigid.MovePosition(targetPos);
            }
            else
            {
                rigid.linearVelocity = direction * velocity;
            }
        }

        private CarController randomTargetCar;
        private void ThrowBomb(CarController targetCar)
        {
            Debug.Log("Throw Bomb");
            Vector3 targetPos = targetCar.transform.position;

            Bomb bomb = Instantiate(bombPrefab, transform).GetComponent<Bomb>();
            bomb.GetComponent<NetworkObject>().Spawn();

            bomb.StartThrowing(transform.position, targetPos, 4f, 2f);
        }

        private Collider[] hitCars = new Collider[15];
        private List<CarController> foundCars = new List<CarController>();
        private CarController FindRandomCarInRange(float range)
        {
            foundCars.Clear();

            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, range, hitCars, carLayer);
            for (int i = 0; i < hitCount; i++)
            {
                // CarController 있는지 확인, 아직 안고쳐진 차량인지 확인
                CarController car = hitCars[i].GetComponentInParent<CarController>();
                if (car != null && car.IsAnyBroken)
                {
                    Debug.Log("Detected Car to throw bomb");
                    if (!foundCars.Contains(car))
                    {
                        foundCars.Add(car);
                    }
                }
                if (car == null)
                    Debug.Log("car is null");
            }

            // 리스트 안비어있으면 랜덤 차량 반환
            if (foundCars.Count > 0)
            {
                int randomIndex = Random.Range(0, foundCars.Count);
                return foundCars[randomIndex];
            }
            else
            {
                // 주변에 차량이 없으면 null 반환
                return null;
            }
        }

        private bool IsOutOfBoundary()
        {
            return transform.position.z < -TrafficManager.Instance.CurMapData.LaneLength;
        }
        private void Despawn()
        {
            GetComponent<NetworkObject>().Despawn(true);
            StopSmokeVFXClientRPC();
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, carDetectingRange);
        }

        [ClientRpc]
        private void OnThrowBombClientRPC()
        {
            Managers.Sound.PlaySfx(SFXType.Voice_ThrowBomb, 0.9f);
            Managers.Sound.PlaySfx(SFXType.SwingArm, 0.8f);
        }

        private int vfxId = int.MaxValue;
        [ClientRpc]
        private void PlaySmokeVFXClientRPC()
        {
            Vector3 localRotation = Vector3.zero;
            localRotation.y = 180f;
            vfxId = VFXManager.Instance.PlayLoopingVFX(VFXType.BikerGangSmoke, Vector3.zero, Quaternion.Euler(localRotation), smokeSpotTf);
        }
        [ClientRpc]
        private void StopSmokeVFXClientRPC()
        {
            VFXManager.Instance.StopLoopingVFX(vfxId);
        }
    }
}