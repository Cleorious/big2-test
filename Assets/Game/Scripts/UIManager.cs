using System.Collections;
using System.Collections.Generic;
using Game;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

public enum CharacterExpression
{
    Happy,
    Angry,
    Default,
    Embarassed,
    COUNT
}

public class UIManager : MonoBehaviour
{
    public MenuView menuView;
    public GameplayView gameplayView;

    AssetManager assetManager;

    public void Init(GameManager gameManagerIn)
    {
        menuView.Init(gameManagerIn);
        gameplayView.Init(gameManagerIn);
    }
}
