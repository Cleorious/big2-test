using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayView : MonoBehaviour
{
    [SerializeField] PlayerInfoUIItem[] playerInfoUIItems;
    [SerializeField] Button submitButton;
    
    UIManager uiManager;
    
    public void Init(GameManager gameManager)
    {
        uiManager = gameManager.uiManager;
        gameObject.SetActive(false);
    }

    public void SetupPlayers(List<PlayerData> playerDatas)
    {
        gameObject.SetActive(true);
        
        //!NOTE: player is always index 0
        int count = playerDatas.Count;
        for(int i = 0; i < count; i++)
        {
            playerInfoUIItems[i].SetPlayerData(playerDatas[i]);
            Sprite sprite = uiManager.GetCharacterSprite(playerDatas[i].characterIndex);
        }
    }
}
