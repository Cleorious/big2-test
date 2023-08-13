using System.Collections;
using System.Collections.Generic;
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
        int count = playerDatas.Count;
        for(int i = 0; i < count; i++)
        {
            playerInfoUIItems[i].SetPlayerData(playerDatas[i]);
            playerInfoUIItems[i].SetThinking(i == currTurnPlayerIndex);
            playerInfoUIItems
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
