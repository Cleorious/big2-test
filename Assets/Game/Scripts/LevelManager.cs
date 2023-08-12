using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Game;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Suit
{
    Diamond,
    Clover,
    Heart,
    Spade,
    COUNT,
}

public class PlayerData
{
    public int playerIndex;
    public int characterIndex;
    public List<CardData> handCardDatas;

    public PlayerData(int playerIndexIn)
    {
        handCardDatas = new List<CardData>();
        playerIndex = playerIndexIn;
    }

    // public int money;
}

public class LevelManager : MonoBehaviour
{
    public Transform[] playerHandRefs;

    StateMachine gameLoopSm;
    
    GameManager gameManager;
    [HideInInspector] public AssetManager assetManager;

    bool levelStarted;

    List<int> usedCharacterIndexes;

    public List<PlayerData> sessionPlayerDatas;
    public List<CardData> cardDatas;
    
    //!board data
    CardCombination currBoardCardCombination;

    public Transform rootCardObjectsPool;
    [ReadOnly, SerializeField] List<CardObject> cardObjectsPool;

    bool gameViewReady;

    Coroutine distributeDeckCoroutine;
    int currTurnPlayerIndex;
    int roundTurnCount;
    int roundCount;

    List<CardData> turnSelectedCards;
    CardCombination turnSelectedCardCombination;


    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        assetManager = gameManager.assetManager;
        usedCharacterIndexes = new List<int>();
        sessionPlayerDatas = new List<PlayerData>();
        turnSelectedCards = new List<CardData>();

        //!pooling CardObjects
        cardObjectsPool = new List<CardObject>();
        CardObject cardObjectPrefab = gameManager.assetManager.GetCardObjectPrefab();
        for(int i = 0; i < Parameter.CARD_OBJECT_INITIAL_COUNT; i++)
        {
            CardObject cardObject = MonoBehaviour.Instantiate(cardObjectPrefab, rootCardObjectsPool);
            cardObject.Init(this);
            cardObjectsPool.Add(cardObject);
        }
    }

    public void DoUpdate(float dt)
    {
        if(levelStarted)
        {
            gameLoopSm.Tick();
        }
    }

    public void StartLevel(int playerCount)
    {
        gameLoopSm = new StateMachine();

        //!NOTE: currently player count is always 4
        sessionPlayerDatas.Clear();
        PlayerData playerData = new PlayerData(0);
        playerData.characterIndex = gameManager.userData.selectedCharIndex;
        sessionPlayerDatas.Add(playerData);
        usedCharacterIndexes.Add(playerData.characterIndex);
        for(int i = 1; i < playerCount; i++)
        {
            PlayerData botData = new PlayerData(i);
            botData.characterIndex = GetUniqueCharacterIndex();
            usedCharacterIndexes.Add(botData.characterIndex);
            sessionPlayerDatas.Add(botData);
        }

        GameStart gameStartState = new GameStart(this);
        DistributeDeck distributeDeckState = new DistributeDeck(this);
        StarterPlayerSearch starterPlayerSearchState = new StarterPlayerSearch(this);
        RoundWinnerSearch roundWinnerSearchState = new RoundWinnerSearch();
        PlayerTurn playerTurnState = new PlayerTurn();
        NonPlayerTurn nonPlayerTurnState = new NonPlayerTurn();
        GameEnd gameEndState = new GameEnd();
        

        Func<bool> HUDReady() => () => gameViewReady;
        Func<bool> DistributionDone() => () => distributeDeckState.distributionDone;
        
        Func<bool> PlayerTurnFinishedAndHasNext() => () => playerTurnState.turnFinished && HasNextPlayer();
        Func<bool> PlayerTurnFinished() => () => playerTurnState.turnFinished;
        
        Func<bool> NonPlayerTurnFinishedAndHasNext() => () => nonPlayerTurnState.allFinished && HasNextPlayer();
        Func<bool> NonPlayerTurnFinished() => () => nonPlayerTurnState.allFinished;
        
        Func<bool> ShouldGoToPlayerTurn() => () => (roundWinnerSearchState.finished || starterPlayerSearchState.finished) && roundWinnerSearchState.nextPlayerIndex == 0;
        Func<bool> ShouldGoToNonPlayerTurn() => () => (roundWinnerSearchState.finished || starterPlayerSearchState.finished) && roundWinnerSearchState.nextPlayerIndex != 0;
        
        At(gameStartState, distributeDeckState, HUDReady());
        At(distributeDeckState, starterPlayerSearchState, DistributionDone());
        
        At(starterPlayerSearchState, playerTurnState, ShouldGoToPlayerTurn());
        At(starterPlayerSearchState, nonPlayerTurnState, ShouldGoToNonPlayerTurn());
        
        At(playerTurnState, nonPlayerTurnState, PlayerTurnFinishedAndHasNext());
        At(playerTurnState, roundWinnerSearchState, PlayerTurnFinished());
        
        At(nonPlayerTurnState, playerTurnState, NonPlayerTurnFinishedAndHasNext());
        At(nonPlayerTurnState, roundWinnerSearchState, NonPlayerTurnFinished());
        
        At(roundWinnerSearchState, playerTurnState, ShouldGoToPlayerTurn());
        At(roundWinnerSearchState, nonPlayerTurnState, ShouldGoToNonPlayerTurn());

        gameLoopSm.SetState(gameStartState);

        levelStarted = true;
    }

    bool HasNextPlayer()
    {
        int remainingActivePlayers = 0;
        int count = sessionPlayerDatas.Count;
        for(int i = 0; i < count; i++)
        {
            if(sessionPlayerDatas[i].handCardDatas.Count > 0)
            {
                remainingActivePlayers++;
            }
        }

        return roundTurnCount < remainingActivePlayers;
    }

    void At(IState from, IState to, Func<bool> condition)
    {
        gameLoopSm.AddTransition(from, to, condition);
    }

    int GetUniqueCharacterIndex()
    {
        int ret = 0;

        ret = Random.Range(0, Parameter.CHARACTER_COUNT);

        while(usedCharacterIndexes.Contains(ret))
        {
            ret = Random.Range(0, Parameter.CHARACTER_COUNT);
        }
        
        return ret;
    }

    public void PrepareGame()
    {
        roundCount = 0;
        gameManager.uiManager.gameplayView.SetupPlayers(sessionPlayerDatas);
        gameViewReady = true;
    }

    public void RefreshHUD()
    {
        Util.Log("test");
    }

    public CardObject GetCardObject()
    {
        CardObject ret = null;
        
        List<CardObject> poolObjects = cardObjectsPool;
        if(poolObjects.Count > 0)
        {
            ret = poolObjects[0];
            poolObjects.RemoveAt(0);
            ret.gameObject.SetActive(true);
        }
        else
        {
            //! instantiate new 
            CardObject prefab = gameManager.assetManager.GetCardObjectPrefab();
            ret = MonoBehaviour.Instantiate(prefab, rootCardObjectsPool);
            ret.Init(this);
        }

        return ret;
    }

    public void ReturnCardObject(CardObject cardObject)
    {
        cardObjectsPool.Add(cardObject);
        cardObject.gameObject.SetActive(false);
        cardObject.transform.SetParent(rootCardObjectsPool);
    }



    public void RefreshPlayerHandPositions(PlayerData playerData, CardData exemptedCard = null)
    {
        List<CardData> handCardDatas = playerData.handCardDatas;
        handCardDatas.Sort(); //!TODO: ENSURE THIS IS SORTED ASCENDING
        int handCount = handCardDatas.Count;
        for(int i = 0; i < handCount; ++i)
        {
            handCardDatas[i].cardObject.transform.SetParent(playerHandRefs[0]);

            if(exemptedCard != null)
            {
                if(handCardDatas[i] == exemptedCard)
                {
                    continue;
                }
            }
        
            float xPos = 0;
            
            xPos = (i) * Parameter.CARD_POOL_X_STACKING_OFFSET + Parameter.CARD_POOL_X_DEFAULT_OFFSET;
        
            float zPos = -Parameter.CARD_Z_OFFSET * (i + 1);
            Vector3 newPos = new Vector3(xPos, Parameter.CARD_POOL_Y_POS, zPos);
        
            handCardDatas[i].cardObject.AnimatePoolPosition(newPos);
        }
    }

    public void PrepNewRound()
    {
        roundCount++;
        roundTurnCount = 0;
        currBoardCardCombination = null;
    }

    public void SearchStarterPlayerAndStart()
    {
        List<PlayerData> playerDatas = sessionPlayerDatas;
        PlayerData starterPlayer = null;

        
        int count = playerDatas.Count;
        for(int i = 0; i < count; i++)
        {
            List<CardData> handCardDatas = playerDatas[i].handCardDatas;
            int handCount = handCardDatas.Count;
            for(int j = 0; j < handCount; j++)
            {
                if(handCardDatas[j].val == Parameter.CARD_DIAMOND_THREE_VAL)
                {
                    starterPlayer = playerDatas[i];
                    break;
                }
            }

            if(starterPlayer != null)
            {
                break;
            }
        }

        currTurnPlayerIndex = starterPlayer.playerIndex;
        
    }

    public int GetNextPlayerTurn()
    {
        return 0;

    }

    public void GetPlayableCards(List<CardData> cardDatas)
    {
        // switch(currBoardState)
        // {
        //
        //     case BoardState.Doubles:
        //         break;
        //     case BoardState.Doubles:
        //         break;
        //     case BoardState.Doubles:
        //         break;
        // }
    }

    public void OnCardPressed(CardData cardData)
    {
        //!check if state is valid
        if(gameLoopSm.currentState is PlayerTurn)
        {
            //check if card is selected
            bool isSelected;
            if(turnSelectedCards.Contains(cardData))
            {
                turnSelectedCards.Remove(cardData);
                isSelected = false;
            }
            else
            {
                turnSelectedCards.Add(cardData);
                isSelected = true;
            }

            cardData.cardObject.SetSelected(isSelected);

            turnSelectedCardCombination = CreateCardCombination(sessionPlayerDatas[0], turnSelectedCards);
            bool validPlay = CheckPlayValidity(turnSelectedCardCombination);
            
            gameManager.uiManager.gameplayView.RefreshSubmitButton(validPlay);
        }
    }

    CardCombination CreateCardCombination(PlayerData owner, List<CardData> combiCardDatas)
    {
        CardCombination ret = null;
        
        CardCombination temp = new Single(owner, combiCardDatas);
        if(temp.IsValid())
        {
            ret = temp;
        }

        if(ret == null)
        {
            temp = new Double(owner, combiCardDatas);
            if(temp.IsValid())
            {
                ret = temp;
            }
        }
        
        if(ret == null)
        {
            temp = new Triple(owner, combiCardDatas);
            if(temp.IsValid())
            {
                ret = temp;
            }
        }
        
        if(ret == null)
        {
            temp = new StraightFlush(owner, combiCardDatas);
            if(temp.IsValid())
            {
                ret = temp;
            }
        }
        
        if(ret == null)
        {
            temp = new Straight(owner, combiCardDatas);
            if(temp.IsValid())
            {
                ret = temp;
            }
        }
        
        if(ret == null)
        {
            temp = new Flush(owner, combiCardDatas);
            if(temp.IsValid())
            {
                ret = temp;
            }
        }

        if(ret == null)
        {
            temp = new FullHouse(owner, combiCardDatas);
            if(temp.IsValid())
            {
                ret = temp;
            }
        }

        if(ret == null)
        {
            temp = new FourOfAKind(owner, combiCardDatas);
            if(temp.IsValid())
            {
                ret = temp;
            }
        }
			
        return ret;
    }

    bool CheckPlayValidity(CardCombination cardCombination)
    {
        bool valid = false;
        
        if(cardCombination != null)
        {
            //!compare with current cardcombination on board
            if(currBoardCardCombination == null)
            {
                if(roundCount == Parameter.ROUND_STARTER)
                {
                    valid = cardCombination.HasSpecificCard(Parameter.CARD_DIAMOND_THREE_VAL);
                }
                else
                {
                    valid = true;
                }
            }
            else
            {
                valid = cardCombination.IsStrongerPlay(currBoardCardCombination);
            }
        }

        return valid;
    }

    public void SubmitCardCombination()
    {
        bool valid = CheckPlayValidity(turnSelectedCardCombination);
        if(valid)
        {
            //TODO: possible ux improvements to show the previous played cards instead of immediately destroying it
            //! remove last card combination
            if(currBoardCardCombination != null)
            {
                List<CardData> cardsToRemove = currBoardCardCombination.GetCards();
                int removeCount = cardsToRemove.Count;
                for(int i = 0; i < removeCount; i++)
                {
                    ReturnCardObject(cardsToRemove[i].cardObject);
                }
            }

            currBoardCardCombination = turnSelectedCardCombination;

            List<CardData> cardsToPlay = currBoardCardCombination.GetCards();
            int playCount = cardsToPlay.Count;
            for(int i = 0; i < playCount; i++)
            {
                float xPos = (i) * Parameter.CARD_BOARD_X_STACKING_OFFSET + Parameter.CARD_BOARD_X_DEFAULT_OFFSET;
        
                float zPos = -Parameter.CARD_Z_OFFSET * (i + 1);
                Vector3 newPos = new Vector3(xPos, Parameter.CARD_BOARD_Y_POS, zPos);
                
                cardsToPlay[i].cardObject.AnimatePlayCard(newPos);
            }
        }

    }

    public void PassTurn()
    {
        
    }
}
