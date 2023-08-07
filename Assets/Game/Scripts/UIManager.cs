using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

public enum CharacterExpression
{
    Happy,
    Angry,
    Default,
    Embarassed,
    COUNT
}

public class UIManager : MonoBehaviour
{
    public MenuView menuView;
    public GameplayView gameplayView;

    [ReadOnly, SerializeField] List<Sprite> characterSprites;

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
#endif

    public void Init(GameManager gameManager)
    {
        menuView.Init(gameManager);
        gameplayView.Init(gameManager);
    }

    public Sprite GetCharacterSprite(int charIndex, CharacterExpression characterExpression = CharacterExpression.Default)
    {
        int targetIndex = (charIndex * Parameter.CHARACTER_COUNT) + (int)characterExpression;
        Sprite ret = characterSprites[targetIndex];

        return ret;
    }
}
