using System;
using System.Collections;
using System.Collections.Generic;
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
    public Transform[] playerHandRefs;

    StateMachine gameLoopSm;
    
    GameManager gameManager;
    [HideInInspector] public AssetManager assetManager;

    bool levelStarted;

    List<int> usedCharacterIndexes;

    public List<PlayerData> sessionPlayerDatas;
    public List<CardData> cardDatas;
    
    public Transform rootCardObjectsPool;
    [ReadOnly, SerializeField] List<CardObject> cardObjectsPool;

    bool gameViewReady;

    public Coroutine distributeDeckCoroutine;

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
        

        Func<bool> HUDReady() => () => gameViewReady;
        Func<bool> DistributionDone() => () => distributeDeckCoroutine == null;

        At(distributeDeckState, gameStartState, HUDReady());
        At(starterPlayerSearchState, distributeDeckState, DistributionDone());

        gameLoopSm.SetState(gameStartState);

        levelStarted = true;
    }

    void At(IState to, IState from, Func<bool> condition)
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

    public void SetupHUD()
    {
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

    public void DistributeCards()
    {
        distributeDeckCoroutine = StartCoroutine(DoDistributeCards());
    }
    
    public IEnumerator DoDistributeCards()
    {
        //!TODO: animate players' cards distribution
        PlayerData playerData = sessionPlayerDatas[0];
        List<CardData> handCardDatas = playerData.handCardDatas;
        
        RefreshPlayerCardPosition(playerData);

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
                handCardDatas[j].cardObject.transform.SetParent(playerHandRefs[playerIndex]);
                handCardDatas[j].cardObject.transform.localPosition = Vector3.zero;
                handCardDatas[j].cardObject.transform.localRotation = Quaternion.Euler(Parameter.CARD_FACEDOWN_ROT);
            }
        }

        distributeDeckCoroutine = null;
    }

    public void RefreshPlayerCardPosition(PlayerData playerData, CardData exemptedCard = null)
    {
        List<CardData> handCardDatas = playerData.handCardDatas;
        int poolLeftCount = handCardDatas.Count;
        for(int index = 0; index < poolLeftCount; ++index)
        {
            if(exemptedCard != null)
            {
                if(handCardDatas[index] == exemptedCard)
                {
                    continue;
                }
            }
        
            float xPos = 0;
            
            xPos = (index) * Parameter.CARD_POOL_X_STACKING_OFFSET + Parameter.CARD_POOL_X_DEFAULT_OFFSET;
        
            float zPos = -Parameter.CARD_Z_OFFSET * (index + 1);
            Vector3 newPos = new Vector3(xPos, Parameter.CARD_POOL_Y_POS, zPos);
        
            handCardDatas[index].cardObject.AnimatePoolPosition(newPos);
        }
    }
}
