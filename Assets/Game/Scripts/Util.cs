using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{

public static class Util
{
    static System.Random rng = new System.Random();

    public static Suit GetCardSuit(int val)
    {
        Suit ret = Suit.Diamond;
        int suitVal = Mathf.FloorToInt((val - 1f) / 13f);
        if(suitVal < 0)
        {
            suitVal = 0;
        }

        ret = (Suit)suitVal;

        return ret;
    }

    public static Color GetCardSuitColor(int val)
    {
        bool color = (val <= 13) || ((val > 26) && (val <= 39));

        return color ? Color.red : Color.black;
    }

    public static int GetNormalizedCardValue(int val)
    {
        int ret = val % 13;
        if(ret == 0)
        {
            ret = 13;
        }

        return ret;
    }
    
    public static string GetNormalizedCardString(int val)
    {
        string ret = String.Empty;
        int normalizedVal = val % 13;
        if(normalizedVal == 0)
        {
            normalizedVal = 13;
        }

        if(normalizedVal <= 10)
        {
            if(normalizedVal == 1)
            {
                ret = "A";
            }
            else
            {
                ret = normalizedVal.ToString();
            }
        }
        else
        {
            int royalIndex = normalizedVal - Parameter.DECK_CARD_ROYAL_INDEX_START;
            switch(royalIndex)
            {
                case 0:
                    ret = "J";
                    break;
                case 1:
                    ret = "Q";
                    break;
                case 2:
                    ret = "K";
                    break;
            }
        }

        return ret;
    }
    
    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }

    public static void Log(string str, params object[] objects)
    {
#if UNITY_EDITOR
        if(objects.Length > 0)
        {
            Debug.Log(string.Format(str, objects));
        }
        else
        {
            Debug.Log(string.Format(str));
        }
#endif
    }
}

}

