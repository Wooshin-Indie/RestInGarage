using Garage.Props;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Environment
{
    public class OilSprayPS : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float spawnChance = 0.1f;
        [SerializeField] private float minDistance = 0.5f;
        [SerializeField] private float heightOffset = 0.02f;

        private OilPump oilPump = null;
        private ParticleSystem part;
        private List<ParticleCollisionEvent> collisionEvents;

        // 로컬에서만 체크하는 거리 제한 (RPC 낭비 방지용)
        private List<Vector3> recentSpawnPositions = new List<Vector3>();

        private void Awake()
        {
            oilPump = GetComponentInParent<OilPump>();
            part = GetComponent<ParticleSystem>();
            collisionEvents = new List<ParticleCollisionEvent>();
        }

        private void OnParticleCollision(GameObject other)
        {
            // 소유권 확인: 내 캐릭터가 쏜 파티클만 내가 계산함
            if (!(oilPump.IsOwner)) return;

            if ((groundLayer.value & (1 << other.layer)) == 0) return;

            int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

            for (int i = 0; i < numCollisionEvents; i++)
            {
                // 확률 체크 (네트워크 트래픽 절약을 위해 여기서 미리 거름)
                //if (Random.value > spawnChance) continue;

                Vector3 pos = collisionEvents[i].intersection;
                //Vector3 normal = collisionEvents[i].normal;

                // 너무 가까운 위치인지 로컬에서 먼저 체크
                if (IsTooClose(pos)) continue;

                SpawnPuddleServerRpc(pos, Vector3.up);

                // 위치 기록
                AddToHistory(pos);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnPuddleServerRpc(Vector3 pos, Vector3 normal)
        {
            SpawnPuddleClientRpc(pos, normal);
        }

        // [ClientRpc]: 서버 -> 모든 클라이언트 실행
        [ClientRpc]
        private void SpawnPuddleClientRpc(Vector3 pos, Vector3 normal)
        {
            // 각 클라이언트는 자신의 '로컬 풀'에서 웅덩이를 꺼내 배치
            SpawnLocalPuddle(pos, normal);
        }

        private void SpawnLocalPuddle(Vector3 position, Vector3 normal)
        {
            if (PuddlePool.Instance == null) return;

            GameObject puddle = PuddlePool.Instance.GetPuddle();

            Vector3 spawnPos = position + (normal * heightOffset);
            Quaternion spawnRot = Quaternion.FromToRotation(Vector3.up, normal);

            puddle.transform.position = spawnPos;
            puddle.transform.rotation = spawnRot;

            // 랜덤 회전
            //puddle.transform.Rotate(Vector3.up, Random.Range(0, 360), Space.Self);
        }

        private bool IsTooClose(Vector3 pos)
        {
            foreach (var spawnedPos in recentSpawnPositions)
            {
                if (Vector3.Distance(pos, spawnedPos) < minDistance)
                    return true;
            }
            return false;
        }

        private void AddToHistory(Vector3 pos)
        {
            recentSpawnPositions.Add(pos);
            if (recentSpawnPositions.Count > 20) recentSpawnPositions.RemoveAt(0);
        }
    }
}