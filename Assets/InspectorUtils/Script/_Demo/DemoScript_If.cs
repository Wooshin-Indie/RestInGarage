using UnityEngine;

namespace IUtil._Demo
{
    public class DemoScript_If : MonoBehaviour
	{
		public bool isReadonly;
		[ReadOnlyIf(nameof(isReadonly))]
		public int readonlyValue;

		public bool isHide;
		[HideIf(nameof(isHide))]
		public float hideValue;

		public bool isShow;
		[ShowIf(nameof(isShow))]
		public float showValue;

	}
}