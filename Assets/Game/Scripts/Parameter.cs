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

    public const int CARD_OBJECT_INITIAL_COUNT = 52;

    public static Vector3 POSITION_OUT_OF_SCREEN = new Vector3(999f, 999f, 999f);
    
    public static Vector3 INTRO_POS_START_BOTTOM = new Vector3(0, -10f, 0f);
    public static Vector3 CARD_FACEDOWN_ROT = new Vector3(0f, 180f, 0f);
    public const float INTRO_CARD_DELAY = 0.05f;
    
    public static float CARD_POOL_X_STACKING_OFFSET = 0.3f;
    public static float CARD_POOL_X_DEFAULT_OFFSET = -1.7f;
    public static float CARD_Z_OFFSET = -0.1f;
    public static float CARD_POOL_Y_POS = -4.1f;


}

}
