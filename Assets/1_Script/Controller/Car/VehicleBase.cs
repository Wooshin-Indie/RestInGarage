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
        public bool IsKnockbackOnHumanCollision = false;

        public virtual void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        public virtual void Move(Vector3 direction, float velocity) { }

    }
}