using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CombinationType {
	None,
	Singles,
	Doubles,
	Triples,
	Straight,
	Flush,
	FullHouse,
	FourOfAKind,
	StraightFlush,
}

public class CardCombination
{
	protected PlayerData owner;
	protected List<CardData> cardDatas;

	public CardCombination(PlayerData ownerIn, List<CardData> cardDatasIn)
	{
		owner = ownerIn;

		cardDatas = cardDatasIn;
		
		cardDatas.Sort();
	}

	public PlayerData Owner()
	{
		return this.owner;
	}

	public virtual CardData GetStrongestCard()
	{
		return cardDatas[0];
	}
	
	public int Size()
	{
		return cardDatas.Count;
	}

	public bool IsStronger(CardCombination cardCombination)
	{
		if(cardCombination.Size() == 1) //Single
		{
			if(this.Size() == cardCombination.Size() && this.IsValid() && GetStrongestCard().CompareTo(cardCombination.GetStrongestCard()) == 1)
			{
				return true;
			}
		}
		
		if(cardCombination.Size() == 2) //Pair
		{
			
			
			if(this.Size() == cardCombination.Size() && this.IsValid() && this.GetStrongestCard().CompareTo(cardCombination.GetStrongestCard()) == 1)
			{
				return true;
			}
		}
		
		if(cardCombination.Size() == 3) //Triple
		{
			
			if(this.Size() == cardCombination.Size() && this.IsValid() && this.GetStrongestCard().CompareTo(cardCombination.GetStrongestCard()) == 1)
			{
				return true;
			}
		}
		
		
		
		
		
		
		
		return false;
	}

	public virtual bool IsValid()
	{
		return false;
	}
	
	
	public virtual CombinationType GetCombinationType()
	{
		return CombinationType.None;
	}
}
