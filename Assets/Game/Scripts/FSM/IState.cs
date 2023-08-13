
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public interface IState
{
    void Tick(float dt);
    void OnEnter();
    void OnExit();
}

public class GameStart : IState
{
    //!TODO: prepare UI, HUD, ETC?
    LevelManager levelManager;
    public GameStart(LevelManager levelManagerIn)
    {
        levelManager = levelManagerIn;
    }
    
    public void Tick(float dt)
    {
    }

    public void OnEnter()
    {
        Util.Log("Entered State:{0}", this.GetType().ToString());

        levelManager.PrepareGame();
    }

    public void OnExit()
    {
    }
}

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

public class ResolveTurn : IState
{
    //!TODO: start of game: search for player with 3 diamonds
    //!TODO: end of resolve: start turn from player with highest card/last player who played card(?)

    LevelManager levelManager;
    public bool finished;
    public int nextPlayerIndex;
    public bool gameEnded;

    public ResolveTurn(LevelManager levelManagerIn)
    {
        levelManager = levelManagerIn;
    }
    
    public void Tick(float dt)
    {
    }

    public void OnEnter()
    {
        Util.Log("Entered State:{0}", this.GetType().ToString());

        //!TODO: check if total turn count is 0, if so, prep new round
        if(levelManager.roundCount == 0)
        {
            levelManager.PrepareNewRound();
            SearchNextPlayer(false);
        }
        else
        {
            levelManager.ResolveWinner();

            bool roundEnded = CheckRoundEnded();
            if(!roundEnded)
            {
                SearchNextPlayer(false);
            }
            else
            {
                //!TODO: should levelManager.PrepNewRound(); if there is still players who hasn't won
                SearchNextPlayer(true);
                int remainingPlayerCount = GetRemainingPlayerCount();
                bool shouldPrepNewRound = nextPlayerIndex != -1 && remainingPlayerCount > 1;
                if(shouldPrepNewRound)
                {
                    levelManager.PrepareNewRound();
                }
                else
                {
                    gameEnded = true;
                }
            }
        }

        //!TODO: else, check if curr turn count is less than total intended turn count, if so, resolve any winners, and search for next player
        //!TODO: else if curr turn count is max turn count, resolve round, and start new round if there are still players who hasn't won

        levelManager.currRoundTurnCount++;
        levelManager.RefreshHUD();
        finished = true;
    }

    public void OnExit()
    {
        finished = false;
        gameEnded = false;
        levelManager.currTurnPlayerIndex = nextPlayerIndex;
        Util.Log("Exited ResolveTurn, currTurnPlayerIndex: {0}", levelManager.currTurnPlayerIndex);

    }
    
    void SearchNextPlayer(bool roundEnded)
    {
        List<PlayerData> playerDatas = levelManager.sessionPlayerDatas;
        PlayerData starterPlayer = null;

        if(levelManager.roundCount == 1 && levelManager.currRoundTurnCount == 0)
        {
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

            nextPlayerIndex = starterPlayer.playerIndex;
        }
        else
        {
            if(roundEnded)
            {
                CardCombination lastBoardCardCombination = levelManager.currBoardCardCombination;
                if(lastBoardCardCombination != null)
                {
                    PlayerData roundWinnerPlayerData = lastBoardCardCombination.GetOwner();
                    if(roundWinnerPlayerData.winnerOrderIndex == Parameter.PLAYERDATA_WINNERORDER_STILLPLAYING)
                    {
                        nextPlayerIndex = roundWinnerPlayerData.playerIndex;
                    }
                    else
                    {
                        SetNextPlayerIndex();
                    }
                }
                else
                {
                    SetNextPlayerIndex();
                }
            }
            else
            {
                
                SetNextPlayerIndex();
            }
        }
    }
    
    int GetRemainingPlayerCount()
    {
        int remainingPlayerCount = 0;
        List<PlayerData> playerDatas = levelManager.sessionPlayerDatas;
        int count = playerDatas.Count;
        for(int i = 0; i < count; i++)
        {
            if(playerDatas[i].winnerOrderIndex == Parameter.PLAYERDATA_WINNERORDER_STILLPLAYING)
            {
                remainingPlayerCount++;
            }
        }

        return remainingPlayerCount;
    }

    void SetNextPlayerIndex()
    {
        int checkCount = 0;
        int tempNextIndex = CyclePlayerIndex(nextPlayerIndex);
        PlayerData tempNextPlayer = levelManager.sessionPlayerDatas[tempNextIndex];
        while(tempNextPlayer.winnerOrderIndex != Parameter.PLAYERDATA_WINNERORDER_STILLPLAYING || tempNextPlayer.hasPassed)
        {
            if(checkCount > Parameter.PLAYER_COUNT)
            {
                tempNextIndex = -1;
                break;
            }

            tempNextIndex = CyclePlayerIndex(tempNextIndex);
            tempNextPlayer = levelManager.sessionPlayerDatas[tempNextIndex];
            checkCount++;
        }

        nextPlayerIndex = tempNextIndex;
    }

    int CyclePlayerIndex(int playerIndex)
    {
        playerIndex++;
        if(playerIndex >= Parameter.PLAYER_COUNT)
        {
            playerIndex = 0;
        }

        return playerIndex;
    }

    bool CheckRoundEnded()
    {
        bool roundEnded = false;
        List<PlayerData> playerDatas = levelManager.sessionPlayerDatas;
        int count = playerDatas.Count;
        int remainingRoundPlayers = 0;
        for(int i = 0; i < count; i++)
        {
            if(playerDatas[i].winnerOrderIndex == Parameter.PLAYERDATA_WINNERORDER_STILLPLAYING && !playerDatas[i].hasPassed)
            {
                remainingRoundPlayers++;
            }
        }

        roundEnded = remainingRoundPlayers <= 1;

        return roundEnded;
    }
}

public class PlayerTurn : IState //!TODO: imo the main game loop needs to include the number of playerturn based on the amount of players present in that game.. meaning there would be multiple player turn classes?
{

    
    public bool turnFinished;

    LevelManager levelManager;

    public PlayerTurn(LevelManager levelManagerIn)
    {
        levelManager = levelManagerIn;
    }
    
    //!TODO allow player input, and allow player to submit their selected card(if possible by rules)
    public void Tick(float dt)
    {
    }

    public void OnEnter()
    {
        Util.Log("Entered State:{0}", this.GetType().ToString());
        levelManager.onTurnEnd += OnTurnEnd;
        levelManager.RefreshHUD();
        SoundManager.Instance.PlaySfx(SFX.PlayerTurn, 0.25f);

    }

    void OnTurnEnd()
    {
        turnFinished = true;
    }

    public void OnExit()
    {
        turnFinished = false;
        levelManager.onTurnEnd -= OnTurnEnd;
    }
}

public class GameEnd : IState
{
    //!TODO: enter gameend when (remaining player is 1/ actual player has depleted their hand and show their placement based on remaining bot count), and show game end UI + option to replay game
    LevelManager levelManager;
    public GameEnd(LevelManager levelManagerIn)
    {
        levelManager = levelManagerIn;
    }
    
    public void Tick(float dt)
    {
    }

    public void OnEnter()
    {
        Util.Log("Entered State:{0}", this.GetType().ToString());
        levelManager.EndGame();
    }

    public void OnExit()
    {
    }
}

public class NonPlayerTurn : IState //!TODO: figure out if need to separate into different players? dont seem to be recommended because number of other players could be dynamic
{
    //!TODO: loop through number of other bots present in the game, have them run separate fsm logic(?)
    public int currBotIndex;
    public bool turnFinished;
    LevelManager levelManager;

    float thinkingDelay;
    float timer;

    bool hasMoved = false;

    public NonPlayerTurn(LevelManager levelManagerIn)
    {
        levelManager = levelManagerIn;
    }
    
    public void Tick(float dt)
    {
        timer += dt;

        if(timer > thinkingDelay && !hasMoved)
        {
            DoBotPlay();
        }
    }

    public void OnEnter()
    {
        Util.Log("Entered State:{0}, currTurnPlayerIndex:{1}", this.GetType().ToString(), levelManager.currTurnPlayerIndex);
        levelManager.onTurnEnd += OnTurnEnd;
        timer = 0f;
        
        //!Show bot thinking
        thinkingDelay = Random.Range(Parameter.BOT_THINKING_MIN, Parameter.BOT_THINKING_MAX);
        Util.Log("Bot index:{0} is thinking..", levelManager.currTurnPlayerIndex);
        levelManager.RefreshHUD();
    }

    void DoBotPlay()
    {
        hasMoved = true;
        levelManager.DoBotPlay();
        //!Show bot thinking
        Util.Log("Bot index:{0} is Action!", levelManager.currTurnPlayerIndex);
    }
    
    

    void OnTurnEnd()
    {
        turnFinished = true;
    }

    public void OnExit()
    {
        turnFinished = false;
        hasMoved = false;
        
        levelManager.onTurnEnd -= OnTurnEnd;
    }
}

public class CheckTurnEnd : IState
{
    //!TODO: basically bridge state to check if should go to player turn, non player turn, or game end
    //!TODO: should also check based on roundturncount who next player should be
    
    //!TODO: loop through number of other bots present in the game, have them run separate fsm logic(?)
    LevelManager levelManager;
    // public int nextPlayerIndex;
    public bool finished;

    public CheckTurnEnd(LevelManager levelManagerIn)
    {
        levelManager = levelManagerIn;
    }
    
    public void Tick(float dt)
    {
    }

    public void OnEnter()
    {
        Util.Log("Entered State:{0}", this.GetType().ToString());

        finished = true;
    }

    public void OnExit()
    {
        finished = false;
    }
}

// public class ResolveBoard : IState
// {
//     //!TODO: run after all players/bots have taken their turn, and resolve the winner 
//     public void Tick()
//     {
//     }
//
//     public void OnEnter()
//     {
//     }
//
//     public void OnExit()
//     {
//     }
// }

