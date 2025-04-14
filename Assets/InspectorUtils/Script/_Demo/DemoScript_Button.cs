using UnityEngine;

namespace IUtil._Demo
{
    public class DemoScript_Button : MonoBehaviour
	{
		public string Func1Param1;

		[Space(10)]

		public int Func2Param1;
		public float Func2Param2;

		[Button]
		public void Func0()
		{
			Debug.Log($"Func0 Excuted");
		}

		[Button(nameof(Func1Param1))]
		public void Func1(string param1)
		{
			Debug.Log($"Func1 Executed \nParam1 : {param1}");
		}

		[Button(nameof(Func2Param1), nameof(Func2Param2))]
		public void Func2(int param1, float param2)
		{
			Debug.Log($"Func2 Executed! \nParam1 : {param1}, Param2 : {param2}");
		}

		[SerializeField]
		private string str1 = "Hello World!";

		[Button(nameof(str1))]
		public void TypeErrorFunc(float param1)
		{
			Debug.Log($"TypeErrorFunc Executed! \nParam1 : {param1}");
		}

		[Button(nameof(str1))]
		public void ParamCountErrorFunc1()
		{
			Debug.Log($"ParamCountErrorFunc1 Executed!");
		}

		[Button(nameof(str1))]
		public void ParamCountErrorFunc2(string param1, int param2)
		{
			Debug.Log($"ParamCountErrorFunc2 Executed! \nParam1 : {param1}");
		}
	}
}