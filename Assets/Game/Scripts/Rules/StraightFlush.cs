using System;
using System.Collections.Generic;
using Game;

public class StraightFlush : CardCombination
{
	public StraightFlush(PlayerData ownerIn, List<CardData> cardDatasIn) : base(ownerIn, cardDatasIn) {}

	public override CardData GetStrongestCard()
	{
		//!TODO: ensure carddatas array is sorted by rank
		return cardDatas[0];
	}

	public override bool IsValid()
	{
		int cardDatasCount = cardDatas.Count;
		bool ret = cardDatasCount == Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.StraightFlush];

		if(ret)
		{
			ret = cardDatas[0].suit == cardDatas[1].suit &&
			      cardDatas[1].suit == cardDatas[2].suit &&
			      cardDatas[2].suit == cardDatas[3].suit &&
			      cardDatas[3].suit == cardDatas[4].suit;

			if(ret)
			{
				for(int i = 1; i < cardDatasCount; i++)
				{
					if(cardDatas[i].rank != cardDatas[i - 1].rank + 1) //Current card's rank should be equal to previous card's rank + 1 as they are consecutive.
					{
						ret = false;
						break;
					}
				}
			}
		}

		return ret;
	}

	public override CombinationType GetCombinationType()
	{
		return CombinationType.StraightFlush;
	}
}