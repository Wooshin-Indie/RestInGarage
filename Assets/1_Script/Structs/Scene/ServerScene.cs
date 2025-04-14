using Garage.Utils;

namespace Garage.Structs
{
	public class ServerScene : SceneBase
	{
		protected override void Init()
		{
			base.Init();
			SceneEnum = SceneEnum.None;
		}

		public override void Clear()
		{

		}
	}
}