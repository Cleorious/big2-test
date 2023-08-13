using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;

public class GameplayView : MonoBehaviour
{
    [SerializeField] PlayerInfoUIItem[] playerInfoUIItems;
    [SerializeField] Button submitButton;
    [SerializeField] Button passButton;
    
    AssetManager assetManager;
    LevelManager levelManager;
    
    public void Init(GameManager gameManager)
    {
        assetManager = gameManager.assetManager;
        levelManager = gameManager.levelManager;
        gameObject.SetActive(false);
        submitButton.onClick.AddListener(OnSubmitButtonPressed);
        passButton.onClick.AddListener(OnPassButtonPressed);
    }

    void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetupPlayers(List<PlayerData> playerDatas)
    {
        Show();
        
        //!NOTE: player is always index 0
        int count = playerDatas.Count;
        for(int i = 0; i < count; i++)
        {
            playerInfoUIItems[i].SetPlayerData(playerDatas[i]);
            Sprite sprite = assetManager.GetCharacterSprite(playerDatas[i].characterIndex);
            playerInfoUIItems[i].SetPlayerPortrait(sprite);
            playerInfoUIItems[i].SetThinking(false);
        }
    }

    public void RefreshPlayerInfos(List<PlayerData> playerDatas, int currTurnPlayerIndex)
    {
        PlayerData currWinningPlayer = levelManager.GetCurrentlyWinningPlayer();
        
        int count = playerDatas.Count;
        for(int i = 0; i < count; i++)
        {
            PlayerData playerData = playerDatas[i];
            playerInfoUIItems[i].SetPlayerData(playerData);
            bool isThisPlayerTurn = i == currTurnPlayerIndex;
            playerInfoUIItems[i].SetThinking(isThisPlayerTurn);
            CharacterExpression characterExpression = isThisPlayerTurn ? CharacterExpression.Default : playerData == currWinningPlayer || playerData.winnerOrderIndex != Parameter.PLAYERDATA_WINNERORDER_STILLPLAYING ? CharacterExpression.Happy : CharacterExpression.Angry;
            Sprite sprite = assetManager.GetCharacterSprite(playerData.characterIndex, characterExpression);
            playerInfoUIItems[i].SetPlayerPortrait(sprite);
        }
    }

    void OnSubmitButtonPressed()
    {
        levelManager.SubmitCardCombination();
    }

    void OnPassButtonPressed()
    {
        levelManager.PassTurn();
    }

    public void RefreshSubmitButton(bool interactable)
    {
        submitButton.interactable = interactable;
    }
    
    
}
