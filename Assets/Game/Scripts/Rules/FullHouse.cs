using System;
using System.Collections.Generic;
using Game;

public class FullHouse : CardCombination
{
	public FullHouse(PlayerData ownerIn, List<CardData> cardDatasIn) : base(ownerIn, cardDatasIn) {}

	public override CardData GetStrongestCard()
	{
		CardData ret;
		if(cardDatas[0].rank == cardDatas[2].rank) 
		{
			ret = cardDatas[2]; //! All the sorted first 3 cards have same rank means the 3rd card is the highest - top card
		}
		else
		{
			ret = cardDatas[4]; //! Odd 2 cards have lower rank, thus final card is the top card
		}

		return ret;
	}
	

	
	public override bool IsValid()
	{
		bool ret = cardDatas.Count == Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.FullHouse];

		if(ret)
		{
			if(cardDatas[0].rank == cardDatas[2].rank)
			{
				ret = cardDatas[0].rank == cardDatas[1].rank && cardDatas[3].rank == cardDatas[4].rank;
			}
			else if(cardDatas[2].rank == cardDatas[4].rank)
			{
				ret = cardDatas[2].rank == cardDatas[3].rank && cardDatas[0].rank == cardDatas[1].rank;
			}
		}

		return ret;
	}

	public override CombinationType GetCombinationType()
	{
		return CombinationType.FullHouse;
	}
}