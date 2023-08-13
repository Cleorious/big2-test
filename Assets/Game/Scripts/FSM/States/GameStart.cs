using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

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