using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public int characterIndex;
    public int[] handCardIndexes;

    // public int money;
}

public class LevelManager
{
    StateMachine gameLoopSm;
    
    GameManager gameManager;

    bool levelStarted = true;

    List<int> usedCharacterIndexes;

    public List<PlayerData> sessionPlayerDatas;

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        usedCharacterIndexes = new List<int>();
        sessionPlayerDatas = new List<PlayerData>();

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
        PlayerData playerData = new PlayerData();
        playerData.characterIndex = gameManager.userData.selectedCharIndex;
        usedCharacterIndexes.Add(playerData.characterIndex);
        int botCount = playerCount - 1;
        for(int i = 0; i < botCount; i++)
        {
            PlayerData botData = new PlayerData();
            botData.characterIndex = GetUniqueCharacterIndex();
            sessionPlayerDatas.Add(botData);
        }

        GameStart gameStartState = new GameStart(this);
        DistributeDeck distributeDeckState = new DistributeDeck();
        StarterPlayerSearch starterPlayerSearchState = new StarterPlayerSearch();
        PlayerTurn playerTurnState = new PlayerTurn();
        NonPlayerTurn nonPlayerTurnState = new NonPlayerTurn();
        GameEnd gameEndState = new GameEnd();
        
        gameLoopSm.SetState(gameStartState);

        levelStarted = true;
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

    public void SetupHUD()
    {
        gameManager.uiManager.gameplayView.SetupPlayers(sessionPlayerDatas);
    }

    public void RefreshHUD()
    {
    }
}
