using System.Collections;
using System.Collections.Generic;
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
        
    }

    public void SetCard(CardData cardData)
    {
        this.cardData = cardData;
        
        valBigText.SetText("test");
    }
}
