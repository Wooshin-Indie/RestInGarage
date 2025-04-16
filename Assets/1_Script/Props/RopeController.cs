using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

namespace Garage.Props
{
    public class RopeController : MonoBehaviour
    {
        [SerializeField] private Rope rope;
        [SerializeField] private float ropeLengthMult;

        void Update()
        {
            float dis = Vector3.Distance(rope.StartPoint.position, rope.EndPoint.position);
            rope.ropeLength = dis * ropeLengthMult;
        }
    }
}