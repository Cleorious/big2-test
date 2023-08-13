using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

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
