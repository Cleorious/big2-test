using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class CardData : IComparable<CardData>
{
    public int val{ get; }
    public CardObject cardObject;

    public int rank{ get; }

    public Suit suit{ get; }

    public CardData(int valIn)
    {
        val = valIn;
        rank = Util.GetBigTwoCardRank(val);
        suit = Util.GetCardSuit(val);
    }
    
    public int CompareTo(CardData other)
    {
        int ret = 0;

        if (rank > other.rank) 
        {
            ret = 1;
        } else if (rank < other.rank) 
        {
            ret = -1;
        } else if (suit > other.suit) 
        {
            ret = 1;
        } else if (suit < other.suit) 
        {
            ret = -1;
        } else 
        {
            ret = 0;
        }

        return ret;
    }
}
