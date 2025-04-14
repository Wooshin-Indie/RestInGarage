using Garage.Utils;

namespace Garage.Structs
{
	public class MainScene : SceneBase
	{
		protected override void Init()
		{
			base.Init();
			SceneEnum = SceneEnum.Main;
		}

		public override void Clear()
		{

		}
	}
}