
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public interface IState
{
    void Tick();
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
    
    public void Tick()
    {
    }

    public void OnEnter()
    {
        levelManager.SetupHUD();
    }

    public void OnExit()
    {
    }
}

public class DistributeDeck : IState
{
    //!TODO: distribute cards equally among number of players

    LevelManager levelManager;
    public DistributeDeck(LevelManager levelManagerIn)
    {
        levelManager = levelManagerIn;
    }
    
    public void Tick()
    {
    }

    public void OnEnter()
    {
        List<CardData> cardDatas = new List<CardData>();
        int maxIndex = Parameter.DECK_CARD_COUNT + Parameter.DECK_CARD_START_INDEX;
        for(int i = Parameter.DECK_CARD_START_INDEX; i < maxIndex; i++)
        {
            CardData cardData = new CardData();
            cardData.val = i;
            cardData.cardObject = levelManager.GetCardObject();
            cardData.cardObject.SetCard(cardData);
            cardDatas.Add(cardData);
        }
        
        cardDatas.Shuffle(); //!Consider using Seeded Randomizer for actual game(testing, synchronization)

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
        levelManager.DistributeCards();
        //!TODO: need to check to ensure tick() doesn't run while running on enter function?
    }
    
    

    public void OnExit()
    {
    }
}

public class StarterPlayerSearch : IState
{
    //!TODO: start of game: search for player with 3 diamonds
    //!TODO: end of resolve: start turn from player with highest card/last player who played card(?)
    public void Tick()
    {
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }
}

public class PlayerTurn : IState //!TODO: imo the main game loop needs to include the number of playerturn based on the amount of players present in that game.. meaning there would be multiple player turn classes?
{
    //!TODO allow player input, and allow player to submit their selected card(if possible by rules)
    public void Tick()
    {
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }
}

public class GameEnd : IState
{
    //!TODO: enter gameend when (remaining player is 1/ actual player has depleted their hand and show their placement based on remaining bot count), and show game end UI + option to replay game
    
    public void Tick()
    {
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }
}

public class NonPlayerTurn : IState //!TODO: figure out if need to separate into different players? dont seem to be recommended because number of other players could be dynamic
{
    //!TODO: loop through number of other bots present in the game, have them run separate fsm logic(?)
    public int currBotIndex;
    
    public void Tick()
    {
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
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

