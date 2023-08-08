using System.Collections;
using System.Collections.Generic;
using Game;
using TMPro;
using UnityEngine;

public class CardObject : MonoBehaviour
{

    [SerializeField] BoxCollider2D boxCollider2D;
    [SerializeField] TextMeshPro valBigText;
    [SerializeField] TextMeshPro valSmallText;
    [SerializeField] SpriteRenderer suitRenderer;

    LevelManager levelManager;

    CardData cardData;

    public void Init(LevelManager levelManager)
    {
        this.levelManager = levelManager;
        gameObject.SetActive(false);
    }

    public void SetCard(CardData cardData)
    {
        this.cardData = cardData;

        string cardValStr = Util.GetNormalizedCardString(cardData.val);
        valBigText.SetText(cardValStr);
        valSmallText.SetText(cardValStr);

        
        suitRenderer.sprite = levelManager.assetManager.GetCardSuitSprite(cardData.val);
        suitRenderer.color = Util.GetCardSuitColor(cardData.val);
    }
}
