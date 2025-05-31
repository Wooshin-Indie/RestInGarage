using UnityEngine;

namespace Garage.Structs
{
	[CreateAssetMenu(fileName = "VehicleData", menuName = "SO/Vehicle Data")]
	public class VehicleData : ScriptableObject
	{
		[SerializeField] private Material carMaterial;

		public Material CarMaterial => carMaterial;

	}
}
