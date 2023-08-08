using System.Collections;
using System.Collections.Generic;
using Game;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

public class AssetManager : MonoBehaviour
{
    [ReadOnly, SerializeField] List<Sprite> characterSprites;
    [ReadOnly, SerializeField] List<Sprite> suitSprites;
    
    [SerializeField] CardObject cardObjectPrefab;

#if UNITY_EDITOR
    [Button]
    void LoadCharacterAssetSprites()
    {
        characterSprites = new List<Sprite>();
        SpriteAtlas spriteAtlas = Resources.Load<SpriteAtlas>("Atlas/UIAtlas");
        int spriteIdx = 0;
        for(int i = 0; i < Parameter.CHARACTER_COUNT; i++)
        {
            int count = (int)CharacterExpression.COUNT;
            for(int j = 0; j < count; j++)
            {
                string spriteName = Parameter.CHARACTER_SPRITE_NAME + ((i * Parameter.CHARACTER_COUNT) + j);
                Sprite sprite = spriteAtlas.GetSprite(spriteName);
                characterSprites.Add(sprite);
            }
        }
    }
    
    [Button]
    void LoadCardSuitSprites()
    {
        suitSprites = new List<Sprite>();
        SpriteAtlas spriteAtlas = Resources.Load<SpriteAtlas>("Atlas/GameplayAtlas");
        int spriteIdx = 0;
        for(int i = 0; i < (int) Suit.COUNT; i++)
        {
            string spriteName = Parameter.CARD_SUIT_SPRITE_NAME + i;
            Sprite sprite = spriteAtlas.GetSprite(spriteName);
            suitSprites.Add(sprite);
        }
    }
#endif
    
    //!TODO: consider using addressables/asset bundles for actual game to optimize asset caching in memory
    
    public Sprite GetCharacterSprite(int charIndex, CharacterExpression characterExpression = CharacterExpression.Default)
    {
        int targetIndex = (charIndex * Parameter.CHARACTER_COUNT) + (int)characterExpression;
        Sprite ret = characterSprites[targetIndex];

        return ret;
    }

    public CardObject GetCardObjectPrefab()
    {
        return cardObjectPrefab;
    }

    public Sprite GetCardSuitSprite(int val)
    {
        Suit cardSuit = Util.GetCardSuit(val);

        return suitSprites[(int)cardSuit];
    }
}
