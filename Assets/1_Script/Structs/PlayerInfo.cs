
namespace Garage.Structs
{
	public class PlayerInfo
	{
		public PlayerInfo(string name, ulong id, int playerId)
		{
			steamName = name;
			steamId = id;
			this.playerId = playerId;
			isReady = false;
		}

		public string steamName;
		public ulong steamId;
		public int playerId;
		public bool isReady;
	}
}