using Garage.Utils;

namespace Garage.Structs
{
	public class GameScene : SceneBase
	{
		protected override void Init()
		{
			base.Init();
			SceneEnum = SceneEnum.Game;
		}

		public override void Clear()
		{

		}
	}
}