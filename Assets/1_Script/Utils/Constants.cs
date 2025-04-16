using UnityEngine;

namespace Garage.Utils
{
    public static class Constants
    {
        public static readonly string TAG_CHAT = "Chat";
        public static readonly string TAG_PCARD = "PlayerCard";

        public static readonly int LAYER_INTERACTABLE = 1 << 10;

        public static readonly string ANIM_PARAM_SPEED = "Speed";
        public static readonly string ANIM_PARAM_CARRY = "IsCarry";
        public static readonly string ANIM_PARAM_OIL = "IsOil";

        public static readonly int MAX_PLAYERS = 4;
        public static readonly string NAME_SERVER = "_SERVER";

        public static readonly Color[] COLOR_PLAYER =
        {
            Color.red,
            Color.yellow,
            Color.green,
            Color.blue
        };
	}
}