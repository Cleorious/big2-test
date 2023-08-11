using System;
using System.Collections.Generic;
using Game;

public class Triple : CardCombination
{
	public Triple(PlayerData ownerIn, List<CardData> cardDatasIn) : base(ownerIn, cardDatasIn) {}

	public override CardData GetStrongestCard()
	{
		//!TODO check if array sorting is working properly

		return base.GetStrongestCard();
	}

	public override bool IsValid()
	{
		bool ret = false;

		ret = cardDatas.Count == Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.Triples] &&
		      cardDatas[0].rank == cardDatas[1].rank &&
		      cardDatas[1].rank == cardDatas[2].rank;

		return ret;
	}

	public override CombinationType GetCombinationType()
	{
		return CombinationType.Triples;
	}
}
