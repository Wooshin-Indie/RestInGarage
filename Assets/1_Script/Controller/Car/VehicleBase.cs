using Garage.Controller;
using Garage.Manager;
using Garage.Utils;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;


namespace Garage.Vehicle
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(NetworkRigidbody))]
    public class VehicleBase : NetworkBehaviour
    {
        public Rigidbody rigid;

        // 닿으면 플레이어를 넉백시키는 상태인지 확인
        public bool IsKnockbackablePlayerOnCollision = false;

        public virtual void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }
        public void OnCollisionWithPlayer(Collision collision, float force)
        {
            if (!IsHost) return;
            Debug.Log("Collision to Player12");
            if (!IsKnockbackablePlayerOnCollision) return;
            Debug.Log("Collision to Player13");

            if (collision.gameObject.layer != Constants.INT_PLAYER) return;

            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            Vector3 playerPos = player.transform.position;
            Vector3 knockbackDirection = playerPos - transform.position;
            knockbackDirection = (knockbackDirection.x >= 0) ? Vector3.right : Vector3.left;
            //위나 아래쪽으로만 넉백되게

            player.KnockBackClientRPC(knockbackDirection, force);
        }

        public virtual void Move(Vector3 direction, float velocity) { }

    }
}