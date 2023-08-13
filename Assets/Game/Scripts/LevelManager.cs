using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Game;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
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
    public int winnerOrderIndex;
    public bool hasPassed;

    public PlayerData(int playerIndexIn)
    {
        handCardDatas = new List<CardData>();
        playerIndex = playerIndexIn;
        winnerOrderIndex = Parameter.PLAYERDATA_WINNERORDER_STILLPLAYING;
    }
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

    //!board data
    public CardCombination currBoardCardCombination;

    public Transform rootCardObjectsPool;
    [ReadOnly, SerializeField] List<CardObject> cardObjectsPool;

    bool gameViewReady;

    Coroutine distributeDeckCoroutine;
    [HideInInspector] public int currTurnPlayerIndex;
    [HideInInspector] public int currRoundTurnCount;
    [HideInInspector] public int roundCount;
    [HideInInspector] public int lastWinnerOrderIndex;

    List<CardData> turnSelectedCards;
    CardCombination turnSelectedCardCombination;

    public event Action onTurnEnd;


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
            gameLoopSm.Tick(dt);
        }
    }

    public void StartLevel(int playerCount)
    {
        //!NOTE: currently player count is always 4
        usedCharacterIndexes.Clear();
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
        
        if(gameLoopSm == null)
        {
            gameLoopSm = new StateMachine();
            GameStart gameStartState = new GameStart(this);
            DistributeDeck distributeDeckState = new DistributeDeck(this);
            ResolveTurn resolveTurnState = new ResolveTurn(this);
            PlayerTurn playerTurnState = new PlayerTurn(this);
            NonPlayerTurn nonPlayerTurnState = new NonPlayerTurn(this);
            GameEnd gameEndState = new GameEnd(this);
        

            Func<bool> HUDReady() => () => gameViewReady;
            Func<bool> DistributionDone() => () => distributeDeckState.distributionDone;
        
            Func<bool> PlayerTurnFinished() => () => playerTurnState.turnFinished;
        
            Func<bool> NonPlayerTurnFinished() => () => nonPlayerTurnState.turnFinished;
        
            Func<bool> ShouldGoToPlayerTurn() => () => resolveTurnState.finished && !resolveTurnState.gameEnded && resolveTurnState.nextPlayerIndex == 0;
            Func<bool> ShouldGoToNonPlayerTurn() => () => resolveTurnState.finished && !resolveTurnState.gameEnded && resolveTurnState.nextPlayerIndex > 0;
            Func<bool> ShouldEndGame() => () => resolveTurnState.finished && resolveTurnState.gameEnded;
            Func<bool> ShouldRestartGame() => () => levelStarted;
        
            At(gameStartState, distributeDeckState, HUDReady());
            At(distributeDeckState, resolveTurnState, DistributionDone());
        
            At(resolveTurnState, playerTurnState, ShouldGoToPlayerTurn());
            At(resolveTurnState, nonPlayerTurnState, ShouldGoToNonPlayerTurn());
            At(resolveTurnState, gameEndState, ShouldEndGame());
        
            At(playerTurnState, resolveTurnState, PlayerTurnFinished());
        
            At(nonPlayerTurnState, resolveTurnState, NonPlayerTurnFinished());
            At(gameEndState, gameStartState, ShouldRestartGame());
            
            gameLoopSm.SetState(gameStartState);
        }
        
        levelStarted = true;
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
        lastWinnerOrderIndex = 0;
        gameManager.uiManager.gameplayView.SetupPlayers(sessionPlayerDatas);
        gameViewReady = true;
    }

    public void EndGame()
    {
        //!TODO: show ui
        int count = sessionPlayerDatas.Count;
        for(int i = 0; i < count; i++)
        {
            if(sessionPlayerDatas[i].winnerOrderIndex == Parameter.PLAYERDATA_WINNERORDER_STILLPLAYING)
            {
                sessionPlayerDatas[i].winnerOrderIndex = Parameter.PLAYER_COUNT - 1; //! set winner order to last
                //!cleanup their remaining cards
                List<CardData> cardsToRemove = sessionPlayerDatas[i].handCardDatas;
                int removeCount = cardsToRemove.Count;
                for(int j = 0; j < removeCount; j++)
                {
                    ReturnCardObject(cardsToRemove[j].cardObject);
                }
                break;
            }
        }
        gameManager.uiManager.winPopup.Show(sessionPlayerDatas);
        levelStarted = false;
        CleanUpBoardCards();
    }

    public void RefreshHUD()
    {
        Util.Log("test");
        GameplayView gameplayView = gameManager.uiManager.gameplayView;
        gameplayView.RefreshPlayerInfos(sessionPlayerDatas, currTurnPlayerIndex);
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

    public void PrepareNewRound()
    {
        roundCount++;
        currRoundTurnCount = 0;
        if(currBoardCardCombination != null)
        {
            CleanUpBoardCards();
            currBoardCardCombination = null;
        }
        int count = sessionPlayerDatas.Count;
        for(int i = 0; i < count; i++)
        {
            sessionPlayerDatas[i].hasPassed = false;
        }
        
        RefreshHUD();
    }

    public void ResolveWinner()
    {
        int count = sessionPlayerDatas.Count;
        for(int i = 0; i < count; i++)
        {
            PlayerData playerData = sessionPlayerDatas[i];
            if(playerData.handCardDatas.Count == 0 && playerData.winnerOrderIndex == Parameter.PLAYERDATA_WINNERORDER_STILLPLAYING)
            {
                playerData.winnerOrderIndex = lastWinnerOrderIndex;
                lastWinnerOrderIndex++;
            }
        }
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
        StartCoroutine(DoSubmitCardCombination());
    }

    public IEnumerator DoSubmitCardCombination()
    {
        bool valid = CheckPlayValidity(turnSelectedCardCombination);
        if(valid)
        {
            //TODO: possible ux improvements to show the previous played cards instead of immediately destroying it
            //! remove last card combination
            CleanUpBoardCards();
            currBoardCardCombination = turnSelectedCardCombination;

            List<CardData> cardsToPlay = currBoardCardCombination.GetCards();
            PlayerData owner = currBoardCardCombination.GetOwner();
            int playCount = cardsToPlay.Count;
            for(int i = 0; i < playCount; i++)
            {
                float xPos = (i) * Parameter.CARD_BOARD_X_STACKING_OFFSET + Parameter.CARD_BOARD_X_DEFAULT_OFFSET;
        
                float zPos = -Parameter.CARD_Z_OFFSET * (i + 1);
                Vector3 newPos = new Vector3(xPos, Parameter.CARD_BOARD_Y_POS, zPos);
                
                cardsToPlay[i].cardObject.AnimateSubmitCard(newPos);
                owner.handCardDatas.Remove(cardsToPlay[i]);
            }

            yield return new WaitForSeconds(Parameter.CARD_SUBMIT_DURATION);
            
            onTurnEnd?.Invoke();
            turnSelectedCards.Clear();
        }

    }

    void CleanUpBoardCards()
    {
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
    }

    public void PassTurn()
    {
        bool cannotPass = roundCount == 1 &&
                          currRoundTurnCount == 1;
        if(!cannotPass)
        {
            PlayerData playerData = sessionPlayerDatas[currTurnPlayerIndex];
            playerData.hasPassed = true;
            onTurnEnd?.Invoke();
        }
    }

    public void DoBotPlay()
    {
        if(gameLoopSm.currentState is NonPlayerTurn)
        {
            int botIndex = currTurnPlayerIndex;
            Assert.IsTrue(botIndex > 0 && botIndex < Parameter.PLAYER_COUNT);

            PlayerData playerData = sessionPlayerDatas[botIndex];

            StartCoroutine(GetPossibleMoves(playerData, ProcessMoves));
        }
    }

    void ProcessMoves(List<CardCombination> possibleMoves)
    {
        //!TODO: potential improvement to have better logic at choosing possible moves, and not just randomly selecting from list
        //!TODO: e.g. have Bot remember certain cards already being played, and have them save higher value cards, etc

        int count = possibleMoves.Count;
        if(count > 0)
        {
            
            int randIdx = Random.Range(0, count);

            turnSelectedCardCombination = possibleMoves[randIdx];
            
            Util.Log("ProcessedMove, options: {0}, selectedIdx: {1}, selectedCombo: {2}", count, randIdx, turnSelectedCardCombination.GetCombinationType());
            SubmitCardCombination();
        }
        else
        {
            Util.Log("ProcessedMove, no possible moves found, passing..");
            PassTurn();
        }
    }

    IEnumerator GetPossibleMoves(PlayerData owner, Action<List<CardCombination>> processMoves)
    {
        List<CardCombination> possibleMoves = new List<CardCombination>();

        //!TODO: CONSIDER TURNING THIS INTO COROUTINE AND ADD DELAYS TO PREVENT OVERLOADED CALCULATIONS IN ONE FRAME-----------------
        
        List<CardData> handCardDatas = owner.handCardDatas;
        //!preliminary grouping to reduce amount of looping
        Dictionary<int, List<CardData>> rankCardGrouping = new Dictionary<int, List<CardData>>();
        Dictionary<Suit, List<CardData>> suitCardGrouping = new Dictionary<Suit, List<CardData>>();

        int count = handCardDatas.Count;
        for(int i = 0; i < count; i++)
        {
            CardData cardData = handCardDatas[i];
            int rank = cardData.rank;
            if(!rankCardGrouping.ContainsKey(rank))
            {
                rankCardGrouping.Add(rank, new List<CardData>(){cardData});
            }
            else
            {
                rankCardGrouping[rank].Add(cardData);
            }

            Suit suit = cardData.suit;
            if (!suitCardGrouping.ContainsKey(suit))
            {
                suitCardGrouping.Add(suit, new List<CardData>(){cardData});
            }
            else
            {
                suitCardGrouping[suit].Add(cardData);
            }
        }

        yield return new WaitForEndOfFrame();

        if(currBoardCardCombination != null)
        {
            CombinationType combinationType = currBoardCardCombination.GetCombinationType();
            switch(combinationType)
            {
            case CombinationType.Singles:
                List<CardCombination> singles = GetPossibleSingles(owner);
                possibleMoves.AddRange(singles);
        
                yield return new WaitForEndOfFrame();
                break;
            case CombinationType.Doubles:
                List<CardCombination> doubles = GetPossibleDoubles(owner, rankCardGrouping);
                possibleMoves.AddRange(doubles);

                yield return new WaitForEndOfFrame();
                break;
            case CombinationType.Triples:
                List<CardCombination> triples = GetPossibleTriples(owner, rankCardGrouping);
                possibleMoves.AddRange(triples);
        
                yield return new WaitForEndOfFrame();
                break;
            case CombinationType.Straight:
            case CombinationType.Flush:
            case CombinationType.StraightFlush:
            case CombinationType.FullHouse:
            case CombinationType.FourOfAKind:
                List<CardCombination> straights = GetPossibleStraightCombos(owner, rankCardGrouping);
                possibleMoves.AddRange(straights);
        
                yield return new WaitForEndOfFrame();
        
                List<CardCombination> flushes = GetPossibleFlushCombos(owner, suitCardGrouping, false);
                possibleMoves.AddRange(flushes);
        
                yield return new WaitForEndOfFrame();
        
                List<CardCombination> straightFlushes = GetPossibleFlushCombos(owner, suitCardGrouping, true);
                possibleMoves.AddRange(straightFlushes);
        
                yield return new WaitForEndOfFrame();
        
                List<CardCombination> fullHouses = GetPossibleFullHouseCombos(owner, rankCardGrouping);
                possibleMoves.AddRange(fullHouses);
        
                yield return new WaitForEndOfFrame();
        
                List<CardCombination> fourOfAKinds = GetPossibleFourOfAKindCombos(owner, rankCardGrouping);
                possibleMoves.AddRange(fourOfAKinds);
        
                yield return new WaitForEndOfFrame();
                break;

            }
            
            count = possibleMoves.Count;
            List<CardCombination> invalidMoves = new List<CardCombination>();
            for(int i = count - 1; i >= 0; i--)
            {
                if(currBoardCardCombination.IsStrongerPlay(possibleMoves[i]))
                {
                    invalidMoves.Add(possibleMoves[i]);
                }
            }

            count = invalidMoves.Count;
            for(int i = 0; i < count; i++)
            {
                possibleMoves.Remove(invalidMoves[i]);
            }
        }
        else
        {
            List<CardCombination> singles = GetPossibleSingles(owner);
            possibleMoves.AddRange(singles);

            yield return new WaitForEndOfFrame();

            List<CardCombination> doubles = GetPossibleDoubles(owner, rankCardGrouping);
            possibleMoves.AddRange(doubles);

            yield return new WaitForEndOfFrame();

            List<CardCombination> triples = GetPossibleTriples(owner, rankCardGrouping);
            possibleMoves.AddRange(triples);

            yield return new WaitForEndOfFrame();

            List<CardCombination> straights = GetPossibleStraightCombos(owner, rankCardGrouping);
            possibleMoves.AddRange(straights);

            yield return new WaitForEndOfFrame();

            List<CardCombination> flushes = GetPossibleFlushCombos(owner, suitCardGrouping, false);
            possibleMoves.AddRange(flushes);

            yield return new WaitForEndOfFrame();

            List<CardCombination> straightFlushes = GetPossibleFlushCombos(owner, suitCardGrouping, true);
            possibleMoves.AddRange(straightFlushes);

            yield return new WaitForEndOfFrame();

            List<CardCombination> fullHouses = GetPossibleFullHouseCombos(owner, rankCardGrouping);
            possibleMoves.AddRange(fullHouses);

            yield return new WaitForEndOfFrame();

            List<CardCombination> fourOfAKinds = GetPossibleFourOfAKindCombos(owner, rankCardGrouping);
            possibleMoves.AddRange(fourOfAKinds);

            yield return new WaitForEndOfFrame();
        }




        processMoves(possibleMoves);
    }

    List<CardCombination> GetPossibleSingles(PlayerData owner)
    {
        List<CardData> handCardDatas = owner.handCardDatas;
        List<CardCombination> cardCombinations = new List<CardCombination>();

        int count = handCardDatas.Count;
        for(int i = 0; i < count; i++)
        {
            CardCombination cardCombination = new Single(owner, new List<CardData>()
            {
                handCardDatas[i]
            });

            if(cardCombination.IsValid())
            {
                cardCombinations.Add(cardCombination);
            }
        }

        return cardCombinations;
    }

    List<CardCombination> GetPossibleDoubles(PlayerData owner, Dictionary<int, List<CardData>> rankCardGrouping)
    {
        List<CardCombination> cardCombinations = new List<CardCombination>();

        foreach(KeyValuePair<int,List<CardData>> grouping in rankCardGrouping)
        {
            if(grouping.Value.Count >= Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.Doubles])
            {
                //!TODO: possible improvement here, to create combinations based on all possibilities and not just single directional

                int count = grouping.Value.Count;
                for(int i = 1; i < count; i++)
                {
                    CardCombination cardCombination = new Double(owner, new List<CardData>(){grouping.Value[i], grouping.Value[i - 1]});
                    if(cardCombination.IsValid())
                    {
                        cardCombinations.Add(cardCombination);
                    }
                }
            }
        }

        return cardCombinations;
    }
    
    List<CardCombination> GetPossibleTriples(PlayerData owner, Dictionary<int, List<CardData>> rankCardGrouping)
    {
        List<CardCombination> cardCombinations = new List<CardCombination>();
        
        foreach(KeyValuePair<int,List<CardData>> grouping in rankCardGrouping)
        {
            if(grouping.Value.Count > Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.Triples])
            {
                //!TODO: possible improvement here, to create combinations based on all possibilities and not just single directional

                int count = grouping.Value.Count;
                for(int i = 2; i < count; i++)
                {
                    CardCombination cardCombination = new Triple(owner, new List<CardData>(){grouping.Value[i], grouping.Value[i - 1], grouping.Value[i - 2]});
                    if(cardCombination.IsValid())
                    {
                        cardCombinations.Add(cardCombination);
                    }
                }
            }
        }

        return cardCombinations;
    }

    List<CardCombination> GetPossibleStraightCombos(PlayerData owner, Dictionary<int, List<CardData>> rankCardGroupings)
    {
        List<CardCombination> cardCombinations = new List<CardCombination>();

        int minLength = Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.Straight];

        List<int> curatedRankList = new List<int>(rankCardGroupings.Keys);
        curatedRankList.Sort(); //!TODO: ENSURE THIS IS ASCENDING
        int count = curatedRankList.Count;
        for(int i = 0; i < count - minLength; i++)
        {
            int targetRank = curatedRankList[i];
            List<CardData> straightAttempt = new List<CardData> { rankCardGroupings[targetRank][0] }; //!TODO: Possible improvement here is to also include combinations for more than one result of targetRank
            for (int j = 1; j < minLength; j++)
            {
                int nextTargetRank = curatedRankList[j]; 
                if(targetRank == nextTargetRank + 1) //!TODO: ENSURE LIST IS ASCENDING AND THIS LOGIC IS CORRECT
                {
                    straightAttempt.Add(rankCardGroupings[nextTargetRank][0]);
                }
                else
                {
                    break; //! Not a valid straight
                }
            }

            if (straightAttempt.Count == minLength)
            {
                CardCombination cardCombination = new Straight(owner, straightAttempt);
                if(cardCombination.IsValid())
                {
                    cardCombinations.Add(cardCombination);
                }
            }
        }

        return cardCombinations;
    }
    
    List<CardCombination> GetPossibleFullHouseCombos(PlayerData owner, Dictionary<int, List<CardData>> rankCardGrouping)
    {
        List<CardCombination> cardCombinations = new List<CardCombination>();
        
        foreach (KeyValuePair<int, List<CardData>> grouping in rankCardGrouping)
        {
            if (grouping.Value.Count >= Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.Doubles]) //! At least two cards of the same rank for a pair
            {
                foreach (var secondGrouping in rankCardGrouping)
                {
                    if (secondGrouping.Value.Count >= Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.Triples] && secondGrouping.Key != grouping.Key)
                    {
                        List<CardData> fullHouseAttempt = new List<CardData>() { grouping.Value[0], grouping.Value[1]}; //!TODO: Possible improvement here is to also include combinations for more than one result of targetRank

                        fullHouseAttempt.AddRange(new List<CardData>() {secondGrouping.Value[0], secondGrouping.Value[1], secondGrouping.Value[2]});
                        CardCombination cardCombination = new FullHouse(owner, fullHouseAttempt);
                        if(cardCombination.IsValid())
                        {
                            cardCombinations.Add(cardCombination);
                        }
                    }
                }
            }
        }

        return cardCombinations;
    }
    
    List<CardCombination> GetPossibleFlushCombos(PlayerData owner, Dictionary<Suit, List<CardData>> suitCardGrouping, bool isStraightFlush)
    {
        List<CardCombination> cardCombinations = new List<CardCombination>();

        int minLength = Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.Flush];
        //! Extract flushes from the grouped cards
        foreach (KeyValuePair<Suit, List<CardData>> grouping in suitCardGrouping)
        {
            if (grouping.Value.Count >= minLength)
            {
                //!TODO: Possible improvement here is to also include combinations for more than one result of targetRank
                grouping.Value.Sort(); //!TODO: ENSURE THIS IS ASCENDING
                int count = grouping.Value.Count;
                if(isStraightFlush)
                {
                    //! need to ensure the cards are also valid as straights
                    for(int i = 0; i < count - minLength; i++)
                    {
                        int targetRank = grouping.Value[i].rank;
                        List<CardData> straightFlushAttempt = new List<CardData> { grouping.Value[i] }; //!TODO: Possible improvement here is to also include combinations for more than one result of targetRank
                        for (int j = 1; j < minLength; j++)
                        {
                            int nextTargetRank = grouping.Value[j].rank;
                            if(targetRank == nextTargetRank + 1)
                            {
                                straightFlushAttempt.Add(grouping.Value[j]);
                            }
                            else
                            {
                                break; //! Not a valid straight
                            }
                        }

                        if (straightFlushAttempt.Count == minLength)
                        {
                            CardCombination cardCombination = new StraightFlush(owner, straightFlushAttempt);
                            if(cardCombination.IsValid())
                            {
                                cardCombinations.Add(cardCombination);
                            }
                        }
                    }
                }
                else
                {
                    //! simply get the highest possible combination
                    List<CardData> flushAttempt = new List<CardData>();

                    for(int i = count - 1; i >= count - minLength; i--)
                    {
                        flushAttempt.Add(grouping.Value[i]);
                    }
                    
                    CardCombination cardCombination = new Flush(owner, flushAttempt);
                    if(cardCombination.IsValid())
                    {
                        cardCombinations.Add(cardCombination);
                    }
                }
                
                
            }
        }

        return cardCombinations;
    }
    
    List<CardCombination> GetPossibleFourOfAKindCombos(PlayerData owner, Dictionary<int, List<CardData>> rankCardGrouping)
    {
        List<CardCombination> cardCombinations = new List<CardCombination>();

        //! Extract four of a kinds from the grouped cards
        foreach (KeyValuePair<int, List<CardData>> grouping in rankCardGrouping)
        {
            if (grouping.Value.Count == 4) //! Four cards of the same rank
            {
                foreach (var secondGrouping in rankCardGrouping)
                {

                    if (secondGrouping.Value.Count >= 1 && secondGrouping.Key != grouping.Key)
                    {
                        //!TODO: Possible improvement here is to also include combinations for more than one result of targetRank
                        List<CardData> fourOfAKindAttempt = new List<CardData>(grouping.Value);

                        fourOfAKindAttempt.Add(secondGrouping.Value[0]);
                        CardCombination cardCombination = new FourOfAKind(owner, fourOfAKindAttempt);
                        if(cardCombination.IsValid())
                        {
                            cardCombinations.Add(cardCombination);
                        }
                    }
                }
                
            }
        }

        return cardCombinations;
    }
}
