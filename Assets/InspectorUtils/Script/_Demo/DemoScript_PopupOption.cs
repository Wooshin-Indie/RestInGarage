using UnityEngine;

namespace IUtil._Demo
{
    public class DemoScript_PopupOption : MonoBehaviour
	{
		private int[] intOptions = new int[] { 1, 2, 3 };
		private float[] floatOptions = new float[] { 5.0f, 6.0f, 7.0f };
		private string[] stringOptions = new string[] { "Option1", "Option2", "Option3" };

		[SerializeField, PopupOption(nameof(intOptions))]
		private int selectInt;

		[SerializeField, PopupOption(nameof(floatOptions), 1)]
		public float selectFloat;

		[SerializeField, PopupOption(nameof(stringOptions), 2)]
		public string selectString;
	}
}