using System.Collections;
using System.Collections.Generic;
using Game;
using Sirenix.OdinInspector;
using UnityEngine;

public enum Suit
{
    Diamond,
    Clover,
    Heart,
    Spade,
    COUNT
}

public class PlayerData
{
    public int characterIndex;
    public List<CardData> handCardDatas;

    public PlayerData()
    {
        handCardDatas = new List<CardData>();
    }

    // public int money;
}

public class LevelManager : MonoBehaviour
{
    StateMachine gameLoopSm;
    
    GameManager gameManager;
    [HideInInspector] public AssetManager assetManager;

    bool levelStarted = true;

    List<int> usedCharacterIndexes;

    public List<PlayerData> sessionPlayerDatas;

    public List<CardData> cardDatas;

    public Transform rootCardObjectsPool;
    [ReadOnly, SerializeField] List<CardObject> cardObjectsPool;
    

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        assetManager = gameManager.assetManager;
        usedCharacterIndexes = new List<int>();
        sessionPlayerDatas = new List<PlayerData>();

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
        PlayerData playerData = new PlayerData();
        playerData.characterIndex = gameManager.userData.selectedCharIndex;
        sessionPlayerDatas.Add(playerData);
        usedCharacterIndexes.Add(playerData.characterIndex);
        int botCount = playerCount - 1;
        for(int i = 0; i < botCount; i++)
        {
            PlayerData botData = new PlayerData();
            botData.characterIndex = GetUniqueCharacterIndex();
            usedCharacterIndexes.Add(botData.characterIndex);
            sessionPlayerDatas.Add(botData);
        }

        GameStart gameStartState = new GameStart(this);
        DistributeDeck distributeDeckState = new DistributeDeck(this);
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
}
