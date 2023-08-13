using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class DistributeDeck : IState
{
    //!TODO: distribute cards equally among number of players

    LevelManager levelManager;
    public bool distributionDone;
    public DistributeDeck(LevelManager levelManagerIn)
    {
        levelManager = levelManagerIn;
    }
    
    public void Tick(float dt)
    {
    }

    public void OnEnter()
    {
        Util.Log("Entered State:{0}", this.GetType().ToString());

        List<CardData> cardDatas = new List<CardData>();
        int maxIndex = Parameter.DECK_CARD_COUNT + Parameter.DECK_CARD_START_INDEX;
        for(int i = Parameter.DECK_CARD_START_INDEX; i < maxIndex; i++)
        {
            CardData cardData = new CardData(i);
            cardData.cardObject = levelManager.GetCardObject();
            cardData.cardObject.SetCard(cardData);
            cardDatas.Add(cardData);
        }
        
        cardDatas.Shuffle(); //!Consider using Seeded Randomizer for actual game(easier testing, reproducibility, synchronization)

        int cardCountPerPlayer = Mathf.FloorToInt((float)Parameter.DECK_CARD_COUNT / Parameter.PLAYER_COUNT);
        Util.Log("Card Count per Player: {0}", cardCountPerPlayer);
        int currPlayerIndex = 0;
        for(int i = 0; i < Parameter.DECK_CARD_COUNT; i++)
        {
            if(i % cardCountPerPlayer == 0 && i != 0)
            {
                currPlayerIndex++;
            }
            
            PlayerData playerData = levelManager.sessionPlayerDatas[currPlayerIndex];
            playerData.handCardDatas.Add(cardDatas[i]);
        }
        
        //!TODO: handle if starting player less than 4
        levelManager.StartCoroutine(DoDistributeCards());

        //!TODO: need to check to ensure tick() doesn't run while running on enter function?
    }
    
    public IEnumerator DoDistributeCards()
    {
        //!TODO: animate players' cards distribution
        List<PlayerData> sessionPlayerDatas = levelManager.sessionPlayerDatas;
        PlayerData playerData = sessionPlayerDatas[0];
        List<CardData> handCardDatas = playerData.handCardDatas;
        
        levelManager.RefreshPlayerHandPositions(playerData);

        int handCount = handCardDatas.Count;
        Vector3[] playerHandPos = new Vector3[handCount];
        Vector3 startPos = Parameter.INTRO_POS_START_BOTTOM;
        for(int i = 0; i < handCount; i++)
        {
            playerHandPos[i] = handCardDatas[i].cardObject.transform.localPosition;
            handCardDatas[i].cardObject.transform.localPosition = startPos;
        }

        yield return null;
        
        for(int i = 0; i < handCount; i++)
        {
            handCardDatas[i].cardObject.AnimateIntroHand(playerHandPos[i], Parameter.INTRO_CARD_DELAY * i);
        }
        SoundManager.Instance.PlaySfx(SFX.CardIntro, 0.25f);
        
        //!TODO: animate bots cards too?
        int botCount = Parameter.PLAYER_COUNT - 1;
        for(int i = 1; i < botCount + 1; i++)
        {
            int playerIndex = i;
            PlayerData botData = sessionPlayerDatas[playerIndex];
            handCardDatas = botData.handCardDatas;
            handCount = handCardDatas.Count;
            for(int j = 0; j < handCount; j++)
            {
                handCardDatas[j].cardObject.transform.SetParent(levelManager.playerHandRefs[playerIndex]);
                handCardDatas[j].cardObject.transform.localPosition = Vector3.zero;
                handCardDatas[j].cardObject.transform.localRotation = Quaternion.Euler(Parameter.CARD_FACEDOWN_ROT);
            }
        }

        distributionDone = true;
    }
    
    

    public void OnExit()
    {
    }
}