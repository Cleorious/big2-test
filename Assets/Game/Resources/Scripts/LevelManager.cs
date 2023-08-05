using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    StateMachine gameLoopSm;
    
    GameManager gameManager;

    bool gameStarted = true;
    
    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;

        
    }

    public void DoUpdate(float dt)
    {
        if(gameStarted)
        {
            gameLoopSm.Tick();
        }
    }

    public void StartGame(int playerCount)
    {
        gameLoopSm = new StateMachine();

        GameStart gameStartState = new GameStart();
        DistributeDeck distributeDeckState = new DistributeDeck();
        StarterPlayerSearch starterPlayerSearchState = new StarterPlayerSearch();
        PlayerTurn playerTurnState = new PlayerTurn();
        NonPlayerTurn nonPlayerTurnState = new NonPlayerTurn();
        GameEnd gameEndState = new GameEnd();
        
        gameLoopSm.SetState(gameStartState);

        gameStarted = true;
    }

    public void RefreshHUD()
    {
        
    }
}
