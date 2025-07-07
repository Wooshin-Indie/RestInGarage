
namespace Garage.Utils
{
	[System.Serializable]
	public enum AnimationType { Carry, Speed, Oil, Place, Tire, Hammer, Crouch, Kick, KnockBack, 
                                CarryMult, }
	public enum SceneEnum
    {
        None = -1,
        Main,
        Game
    }

    public enum PropType
    {
        None = -1,
        Tire,
        Oilgun,
        Wrench,
        Extinguisher
    }

    public enum VehicleDirection
    {
        None,
        Up,
        Down
    }

    public enum PartStatus
    {
        Fine,
        Broken
    }

    public enum CarParts
    {
        FLT, // Front Left Tire
        FRT, // Front Right Tire
        RLT, // Rear Left Tire
        RRT, // Rear Right Tire
        Engine,
        Oil,
        Fire
        /* 부품 하나 늘리려면 해야하는 것:
         * CarParts enum에 추가(여기 스크립트)
         * 정확한 위치가 필요하다면 CarController에 SerializeField로 부품위치 transform 추가
         * CarStatusUI에 switch-case에 case 추가
         * */
    }

    public enum KickDirection
    {
        // 치이는 차 기준
        Left,
        Right
    }

    // Steamworks.LobbyType 에서 가져옴
    public enum LobbyType
    {
        None = -1,
        Private,
        FriendsOnly,
        Public,
        Invisible,
        PrivateUnique
    }

    public enum PlayerState
    {
        Idle,
        Carry,
        Interact
    }
}