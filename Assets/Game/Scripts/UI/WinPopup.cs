using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;

public class WinPopup : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] PlayerWinnerUiItem[] playerWinnerUiItems;

    [SerializeField] Button continueButton;
    
    GameManager gameManager;
    UIManager uiManager;

    public void Init(GameManager gameManagerIn)
    {
        gameManager = gameManagerIn;
        uiManager = gameManagerIn.uiManager;
        
        continueButton.onClick.AddListener(OnClickContinueButton);
    }

    void OnClickContinueButton()
    {
        Hide();
        gameManager.ReturnToMenu();
    }

    public void Show(List<PlayerData> playerDatas)
    {
        int playerCount = playerDatas.Count;
        for(int i = 0; i < playerCount; i++)
        {
            bool isWinner = playerDatas[i].winnerOrderIndex == Parameter.PLAYERDATA_WINNERORDER_FIRSTWINNER;
            Sprite characterSprite = gameManager.assetManager.GetCharacterSprite(playerDatas[i].characterIndex, isWinner ? CharacterExpression.Happy : CharacterExpression.Angry);
            playerWinnerUiItems[i].SetPlayerData(playerDatas[i], characterSprite);
            playerWinnerUiItems[i].gameObject.SetActive(true);
        }
        
        int uiCount = playerWinnerUiItems.Length;
        for(int i = playerCount; i < uiCount; i++)
        {
            playerWinnerUiItems[i].gameObject.SetActive(false);
        }
        
        animator.SetTrigger(Parameter.POPUP_DEFAULT_TRIGGER_SHOW);
    }

    public void Hide()
    {
        animator.SetTrigger(Parameter.POPUP_DEFAULT_TRIGGER_HIDE);
    }
}
