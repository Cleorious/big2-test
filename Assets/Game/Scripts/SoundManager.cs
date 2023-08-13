using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BGM
{
    BGM1,
}

public enum SFX
{
    GenericButton,
    EndGame,
    PlayerTurn,
    CardIntro,
    CardFlip,
    PassTurn,
    PlayerWin,
    SFX_Count
}

public class SoundManager
{
    static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new SoundManager();
            }

            return _instance;
        }
        
    }
    
    AudioSource bgmSource;
    AudioSource[] sfxSources;
    Dictionary<SFX, AudioSource> sfxSourceDict;

    const float BGM_VOLUME = 1f;
    const string BGM_PATH = "BGM/";
    const string SFX_PATH = "SFX/";

    GameObject audioSourceHolder;
    
    public void Init(GameManager gameManager)
    {
        audioSourceHolder = new GameObject("Audio");
        InitBGM();
        InitSFX();
    }

    public void InitBGM()
    {
        bgmSource = audioSourceHolder.AddComponent<AudioSource>();
        bgmSource.playOnAwake = true;
        bgmSource.loop = true;
        bgmSource.volume = BGM_VOLUME;
        bgmSource.spatialBlend = 0;

        bgmSource = audioSourceHolder.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0;
    }

    public void InitSFX()
    {
        int sfxCount = (int)SFX.SFX_Count;
        
        sfxSources = new AudioSource[sfxCount];
        sfxSourceDict = new Dictionary<SFX, AudioSource>();
        
        for(int i = 0; i < sfxCount; i++)
        {
            sfxSources[i] = audioSourceHolder.AddComponent<AudioSource>();
            sfxSources[i].playOnAwake = false;
            sfxSources[i].loop = false;
            sfxSources[i].spatialBlend = 0;
            sfxSources[i].clip = GetAudioClip(SFX_PATH + ((SFX)i));

            sfxSourceDict[(SFX)i] = sfxSources[i];
        }
    }

    public void PlayBGM(BGM bgm, float volume = 1)
    {
        bgmSource.clip = GetAudioClip(BGM_PATH + bgm);
        bgmSource.volume = volume;
        bgmSource.Play();
    }
    
    public void PlaySfx(SFX sfx, float volume = 1)
    {
        AudioSource source = sfxSourceDict[sfx];
        source.volume = volume;
        source.Play();
    }
    
    AudioClip GetAudioClip(string path)
    {
        Debug.Log("Sounds/" + path);
        AudioClip result = Resources.Load<AudioClip>("Sounds/" + path);
        return result;
    }
}
