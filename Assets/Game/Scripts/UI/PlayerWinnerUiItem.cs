using System.Collections;
using System.Collections.Generic;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWinnerUiItem : MonoBehaviour
{
    [SerializeField] Image characterImg;
    [SerializeField] TextMeshProUGUI numberText;
    [SerializeField] Image crownImg;

    public void SetPlayerData(PlayerData playerData, Sprite characterSprite)
    {
        characterImg.sprite = characterSprite;
        crownImg.gameObject.SetActive(playerData.winnerOrderIndex == Parameter.PLAYERDATA_WINNERORDER_FIRSTWINNER);
        string numberStr = "1st";
        switch(playerData.winnerOrderIndex)
        {
        case 1:
            numberStr = "2nd";
            break;
        case 2:
            numberStr = "3rd";
            break;
        case 3:
        case -1:
            numberStr = "4th";
            break;
        }
        numberText.SetText(numberStr);
    }
}
