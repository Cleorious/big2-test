using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    bool isAnimating;

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

    void SetAnimatingState(bool state)
    {
        isAnimating = state;
        boxCollider2D.enabled = !state;
    }

    public void AnimateIntroHand(Vector3 targetPos, float delay)
    {
        if(!isAnimating)
        {
            SetAnimatingState(true);

            transform.localRotation = Quaternion.Euler(Parameter.CARD_FACEDOWN_ROT);
            Sequence handIntroSeq = DOTween.Sequence();
            handIntroSeq.AppendInterval(delay);

            handIntroSeq.Append(transform.DOLocalMove(targetPos, 0.5f));
            handIntroSeq.AppendInterval(0.1f);
            handIntroSeq.OnComplete(() =>
            {
                SetAnimatingState(false);
                transform.localRotation = Quaternion.identity;
            });
        }
    }
    
    public void AnimatePoolPosition(Vector3 poolPos)
    {
        if(!isAnimating)
        {
            SetAnimatingState(true);
            transform.DOMove(poolPos, 0.2f)
                     .OnComplete(() => SetAnimatingState(false));
        }
    }
}
