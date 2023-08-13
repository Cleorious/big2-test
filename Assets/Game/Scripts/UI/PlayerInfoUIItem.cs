using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUIItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI cardCountText;
    [SerializeField] TextMeshProUGUI stateText;
    [SerializeField] Image charImg;
    [SerializeField] RectTransform inactiveOverlay;

    public void SetPlayerData(PlayerData playerData)
    {
        cardCountText.SetText(playerData.handCardDatas.Count.ToString());
        inactiveOverlay.gameObject.SetActive(playerData.hasPassed);
    }

    public void SetThinking(bool show)
    {
        stateText.gameObject.SetActive(show);
    }

    public void SetPlayerPortrait(Sprite sprite)
    {
        charImg.sprite = sprite;
    }
}
