using System;
using System.Collections;
using System.Collections.Generic;
using Game;
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

		//!deep copy carddatas

		cardDatas = new List<CardData>(cardDatasIn);
		
		
		cardDatas.Sort((x, y) => x.CompareTo(y)); //!TODO: ENSURE THIS SORTING IS ASCENDING ORDER
		
	}

	public PlayerData GetOwner()
	{
		return owner;
	}

	public virtual CardData GetStrongestCard()
	{
		return cardDatas[0];
	}

	public List<CardData> GetCards()
	{
		return cardDatas;
	}

	public int Size()
	{
		return cardDatas.Count;
	}

	public bool HasSpecificCard(int val)
	{
		bool ret = false;

		if(cardDatas.Count > 0)
		{
			int count = Size();
			for(int i = 0; i < count; i++)
			{
				if(cardDatas[i].val == val)
				{
					ret = true;
					break;
				}
			}
		}

		return ret;
	}

	public bool IsStrongerPlay(CardCombination otherCardCombination)
	{
		bool ret = false;
		CombinationType combinationType = GetCombinationType();
		CombinationType otherCombinationType = otherCardCombination.GetCombinationType();
		bool sameCombinationType = combinationType == otherCombinationType;
		bool isValid = IsValid() && otherCardCombination.IsValid();
		if(isValid)
		{
			CardData strongestCard = GetStrongestCard();
			CardData otherStrongestCard = otherCardCombination.GetStrongestCard();

			switch(otherCombinationType)
			{
				case CombinationType.Singles:
				case CombinationType.Doubles:
				case CombinationType.Triples:
					ret = sameCombinationType && strongestCard.CompareTo(otherStrongestCard) == 1;
					break;
				case CombinationType.Straight:
				case CombinationType.Flush:
				case CombinationType.FullHouse:
				case CombinationType.FourOfAKind:
				case CombinationType.StraightFlush:
					ret = combinationType >= otherCombinationType && strongestCard.CompareTo(otherStrongestCard) == 1;
					break;
			}
		}

		return ret;
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
