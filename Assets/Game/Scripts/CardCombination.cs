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
		
		// cardDatas.Sort();
		cardDatas.Sort((x, y) => y.CompareTo(x)); //!TODO: ENSURE THIS SORTING IS ASCENDING ORDER

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
