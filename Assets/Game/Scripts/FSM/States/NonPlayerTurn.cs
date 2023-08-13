using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

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
