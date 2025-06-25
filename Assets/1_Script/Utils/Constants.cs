using UnityEngine;

namespace Garage.Utils
{
    public static class Constants
    {
        /** PATHS **/
        public static readonly string PATH_SFX = "Sounds/SFX/";
        public static readonly string PATH_AMB = "Sounds/Ambient/";

        /** TAGS **/
		public static readonly string TAG_CHAT = "Chat";
        public static readonly string TAG_PCARD = "PlayerCard";
        public static readonly string TAG_PLAYER = "Player";

        /** LAYERS **/
        public static readonly int INT_VEHICLE = 9;
          
        public static readonly int LAYER_VEHICLE = 1 << 9;
        public static readonly int LAYER_INTERACTABLE = 1 << 10;
        public static readonly int LAYER_PLAYER = 1 << 11;

        /** ANIM PARAMS **/
        public static readonly string ANIM_PARAM_SPEED = "Speed";
        public static readonly string ANIM_PARAM_CARRY = "IsCarry";
        public static readonly string ANIM_PARAM_OIL = "IsOil";
        public static readonly string ANIM_PARAM_PLACE = "IsPlace";
        public static readonly string ANIM_PARAM_TIREPUT = "IsTirePut";
        public static readonly string ANIM_PARAM_HAMMER = "IsHammering";
        public static readonly string ANIM_PARAM_CROUCH = "IsCrouch";
        public static readonly string ANIM_PARAM_KICK = "IsKick";
        public static readonly string ANIM_PARAM_KNOCKBACK = "IsKnockBack";
        public static readonly string ANIM_PARAM_CARRY_MULT = "CarryMult";

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