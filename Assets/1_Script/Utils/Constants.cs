using UnityEngine;

namespace Garage.Utils
{
	public static class Constants
    {
        /** PATHS **/
        public static readonly string PATH_SFX = "Sounds/SFX/";
        public static readonly string PATH_AMB = "Sounds/Ambient/";
        public static readonly string PATH_BGM = "Sounds/BGM/";

        /** TAGS **/
		public static readonly string TAG_CHAT = "Chat";
        public static readonly string TAG_PCARD = "PlayerCard";
        public static readonly string TAG_PLAYER = "Player";

        /** LAYERS **/
        public static readonly int INT_VEHICLE = 9;
        public static readonly int INT_PLAYER = 11;
          
        public static readonly int LAYER_VEHICLE = 1 << 9;
        public static readonly int LAYER_INTERACTABLE = 1 << 10;
        public static readonly int LAYER_PLAYER = 1 << 11;


        /** ANIM PARAMS **/
        public static readonly string ANIM_PARAM_SPEED = "Speed";
        public static readonly string ANIM_PARAM_CARRY = "IsCarry";
        public static readonly string ANIM_PARAM_OIL = "IsOil";
        public static readonly string ANIM_PARAM_PLACE = "IsPlace";
        public static readonly string ANIM_PARAM_TIREPUT = "IsTirePut";
        public static readonly string ANIM_PARAM_WRENCHREPAIR = "IsWrenchRepair";
        public static readonly string ANIM_PARAM_KICK = "IsKick";
        public static readonly string ANIM_PARAM_KNOCKBACK = "IsKnockBack";
        public static readonly string ANIM_PARAM_CARRY_MULT = "CarryMult";
        public static readonly string ANIM_PARAM_FIX = "IsFix";
        public static readonly string ANIM_PARAM_TIREROLL = "IsTireRoll";
        public static readonly string ANIM_PARAM_HAMMERREPAIR = "IsHammerRepair";
        public static readonly string ANIM_PARAM_WRENCHATTACK = "IsWrenchAttack";
        public static readonly string ANIM_PARAM_FALLBACK = "IsFallBack";
        public static readonly string ANIM_PARAM_OILSPRAY = "IsOilSpray";

        public static readonly int ANIM_LAYER_INDEX_LOWERBODY = 1;

	    /** Localization Tables **/
		public static readonly string TABLE_MAINUI = "MainUI";

        /** NETWORK SETTINGS **/
        public static readonly int MAX_PLAYERS = 4;
        public static readonly string NAME_SERVER = "_SERVER";

        public static readonly string KEY_LOBBYNAME = "LobbyName";
        public static readonly string KEY_GAMENAME = "GameName";
        public static readonly string KEY_PASSWORD = "Password";
        public static readonly string KEY_PASSWORDPROTECTED = "PasswordProtected";
        public static readonly string KEY_MAPIDX = "MapIdx";

        public static readonly string VALUE_GAMENAME = "RestInGarage";

        public static readonly Color[] COLOR_PLAYER =
        {
            Color.red,
            Color.yellow,
            Color.green,
            Color.blue
        };
	}
}