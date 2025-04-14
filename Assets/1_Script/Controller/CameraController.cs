using UnityEngine;

namespace Garage.Controller
{
	public class TopDownCameraController : MonoBehaviour
	{
		[SerializeField] private float cameraBoomLength;
		[SerializeField] private Vector3 fixedRotation = new Vector3(75f, 0f, 0f);
		private void LateUpdate()
		{
			float phi = fixedRotation.y * Mathf.Deg2Rad;
			float theta = fixedRotation.x * Mathf.Deg2Rad;

			float x = cameraBoomLength * Mathf.Cos(theta) * Mathf.Sin(phi);
			float y = cameraBoomLength * Mathf.Sin(theta);
			float z = - cameraBoomLength * Mathf.Cos(theta) * Mathf.Cos(phi);

			transform.position = transform.parent.position +  new Vector3(x, y, z);
			transform.rotation = Quaternion.Euler(fixedRotation);
		}
	}
}