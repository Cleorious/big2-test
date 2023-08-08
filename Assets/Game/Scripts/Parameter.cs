using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{




public static class Parameter
{
    public const string SAVE_KEY = "userdata";

    public const int PLAYER_COUNT = 4;

    public const int CHARACTER_COUNT = 4;

    public const int LOCAL_PLAYER_INDEX = 0;

    public const string CHARACTER_SPRITE_NAME = "Hero_01_Faceset_";
    public const string CARD_SUIT_SPRITE_NAME = "suit_";

    public const string POPUP_DEFAULT_TRIGGER_SHOW = "show";
    public const string POPUP_DEFAULT_TRIGGER_HIDE = "hide";

    public const int DECK_CARD_COUNT = 52;
    public const int DECK_CARD_START_INDEX = 1;
    public const int DECK_CARD_ROYAL_INDEX_START = 11;

    public const int CARD_OBJECT_INITIAL_COUNT = 25;

    public static Vector3 POSITION_OUT_OF_SCREEN = new Vector3(999f, 999f, 999f);
}

}
