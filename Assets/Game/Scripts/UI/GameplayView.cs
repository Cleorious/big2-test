using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayView : MonoBehaviour
{
    [SerializeField] PlayerInfoUIItem[] playerInfoUIItems;
    [SerializeField] Button submitButton;
    
    AssetManager assetManager;
    
    public void Init(GameManager gameManager)
    {
        assetManager = gameManager.assetManager;
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
            Sprite sprite = assetManager.GetCharacterSprite(playerDatas[i].characterIndex);
            playerInfoUIItems[i].SetPlayerPortrait(sprite);
        }
    }
}
