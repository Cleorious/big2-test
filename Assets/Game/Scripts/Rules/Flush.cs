using System;
using System.Collections.Generic;
using Game;

public class Flush : CardCombination
{
	public Flush(PlayerData ownerIn, List<CardData> cardDatasIn) : base(ownerIn, cardDatasIn) {}

	public override CardData GetStrongestCard()
	{
		//!TODO: ensure carddatas array is sorted by rank
		return cardDatas[0];
	}
	

	public override bool IsValid()
	{
		bool ret = cardDatas.Count == Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.Flush];

		if(ret)
		{
			ret = cardDatas[0].suit == cardDatas[1].suit && 
			      cardDatas[1].suit == cardDatas[2].suit && 
			      cardDatas[2].suit == cardDatas[3].suit && 
			      cardDatas[3].suit == cardDatas[4].suit;

		}

		return ret;
	}

	public override CombinationType GetCombinationType()
	{
		return CombinationType.Flush;
	}

	
	

}
