
namespace Garage.Utils
{
	[System.Serializable]
	public enum AnimationType { Carry, Speed, Oil }
	public enum SceneEnum
    {
        None = -1,
        Main,
        Lobby,
        Game
    }

    public enum PropType
    {
        None = -1,
        Tire,
        Oilgun,
    }

    public enum VehicleDirection
    {
        None,
        Up,
        Down
    }
}