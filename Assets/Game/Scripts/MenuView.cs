using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;

public class MenuView : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] ButtonCharaUIItem[] buttonCharaUIItems;

    GameManager gameManager;
    UIManager uiManager;

    public void Init(GameManager gameManagerIn)
    {
        gameManager = gameManagerIn;
        uiManager = gameManagerIn.uiManager;

        int count = buttonCharaUIItems.Length;
        for(int i = 0; i < count; i++)
        {
            int charIdx = i;
            buttonCharaUIItems[i].button.onClick.AddListener(() => OnClickCharacterButton(charIdx));
            buttonCharaUIItems[i].buttonImage.sprite = gameManager.assetManager.GetCharacterSprite(charIdx, CharacterExpression.Happy);
        }
    }

    void OnClickCharacterButton(int characterIndex)
    {
        gameManager.StartGame(characterIndex);
        Hide();
    }

    public void Show()
    {
        animator.SetTrigger(Parameter.POPUP_DEFAULT_TRIGGER_SHOW);
    }

    public void Hide()
    {
        animator.SetTrigger(Parameter.POPUP_DEFAULT_TRIGGER_HIDE);
    }
}
