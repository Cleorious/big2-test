using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class UserData
{
    public int money;
    public int selectedCharIndex;

    public UserData()
    {
        money = 0;
        selectedCharIndex = 0;
    }
}

public class GameManager : MonoBehaviour
{
    //!UI
    public UIManager uiManager;

    public AssetManager assetManager;
    
    public LevelManager levelManager;

    float dtBuff;
    float fixedTimeStep;

    [ReadOnly] public UserData userData;


    void Start()
    {
        dtBuff = 0.0f;
        fixedTimeStep = 0.016f;
        
        //!init managers
        levelManager.Init(this);
        
        Load();
        
        //!init UI
        uiManager.Init(this);
        
        uiManager.menuView.Show();
    }

    public void StartGame(int characterIndex)
    {
        userData.selectedCharIndex = characterIndex;
        levelManager.StartLevel(Parameter.PLAYER_COUNT);
    }

    void Update()
    {
        dtBuff += Time.deltaTime;
        for(; dtBuff > fixedTimeStep; dtBuff -= fixedTimeStep)
        {
            levelManager.DoUpdate(fixedTimeStep);
        }
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(userData);
        PlayerPrefs.SetString(Parameter.SAVE_KEY, json);
    }

    public void Load()
    {
        string json = PlayerPrefs.GetString(Parameter.SAVE_KEY);

        if(!string.IsNullOrEmpty(json))
        {
            userData = JsonUtility.FromJson<UserData>(json);
        }
        else
        {
            userData = new UserData();
        }
    }
}
