using DG.Tweening;
using Garage.Utils;
using UnityEngine;

namespace Garage.Structs
{
	public class MainScene : SceneBase
	{

		[SerializeField] private Light light;
		[SerializeField] private Vector3 startRot;
		[SerializeField] private Vector3 endRot;

		protected override void Init()
		{
			base.Init();
			SceneEnum = SceneEnum.Main;

			light.intensity = 0f;
			light.transform.rotation = Quaternion.Euler(startRot);

			Sequence seq = DOTween.Sequence();

			seq.AppendInterval(5f); 

			seq.Append(light.DOIntensity(1f, 7f));
			seq.Join(light.transform.DORotate(endRot, 7f));
			seq.OnComplete(() =>
			{
				// TODO - UIManager에서 UI불러오기
			});
		}

		public override void Clear()
		{

		}
	}
}