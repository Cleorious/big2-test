using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class PlayerTurn : IState
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
