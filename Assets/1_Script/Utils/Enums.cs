
namespace Garage.Utils
{
	[System.Serializable]
	public enum AnimationType { Carry, Speed, Oil, Place, Tire, Hammer, Kick, KnockBack, 
                                CarryMult, Fix, TireRoll}
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
        Left,
        Right
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

    public enum LocalFourDirection
    {
        // VFX의 파티클이 뿜어져나올 y축 회전값을 정하는 용도로 만듬
        // 차량오브젝트의 로컬기준 4방향을 나타내는 용도로도 사용
        Front,
        Right,
        Rear,
        Left
    }

    // 타이어 및 차량의 사이즈
	public enum TireSize
	{
		Small = 0,
		Big,
	}

	// TODO - 필요한 기록 데이터 추가 필요
	public enum RuntimeRecordType
	{
		FixGage,
		FixCount,
		MoveDistance,
	}
}