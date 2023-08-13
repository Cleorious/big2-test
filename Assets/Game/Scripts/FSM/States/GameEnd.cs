using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

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
