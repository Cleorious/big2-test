using System;
using System.Collections.Generic;
using Game;

public class FourOfAKind : CardCombination
{
	public FourOfAKind(PlayerData ownerIn, List<CardData> cardDatasIn) : base(ownerIn, cardDatasIn) {}

	public override CardData GetStrongestCard()
	{
		CardData ret;
		if(cardDatas[0].rank == cardDatas[1].rank) //If rank of first 2 cards are the same.
		{
			ret = cardDatas[3]; //4th card would be the biggest.
		}
		else
		{
			ret = cardDatas[4];
		}

		return ret;
	}
	

	public override bool IsValid()
	{
		bool ret = cardDatas.Count == Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.FourOfAKind];

		if(ret)
		{
			if(cardDatas[0].rank == cardDatas[1].rank)
			{
				ret = cardDatas[1].rank == cardDatas[2].rank && cardDatas[2].rank == cardDatas[3].rank;
			}
			else
			{
				ret = cardDatas[1].rank == cardDatas[2].rank && cardDatas[2].rank == cardDatas[3].rank && cardDatas[3].rank == cardDatas[4].rank;
			}
		}

		return ret;
	}

	public override CombinationType GetCombinationType()
	{
		return CombinationType.FourOfAKind;
	}
}