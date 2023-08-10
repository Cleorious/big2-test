using System;
using System.Collections.Generic;
using Game;

public class DoubleCombination : CardCombination
{

	public DoubleCombination(PlayerData ownerIn, List<CardData> cardDatasIn) : base(ownerIn, cardDatasIn) {}

	public override CardData GetStrongestCard()
	{
		// CardData ret = cardDatas[0];
		// if(cardDatas[0].CompareTo(cardDatas[1]) == -1)
		// {
		// 	ret = cardDatas[1];
		// }
		//!TODO check if array sorting is working properly


		return base.GetStrongestCard();
	}

	public override bool IsValid()
	{
		bool ret = false;

		ret = cardDatas.Count == Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.Doubles] &&
		      cardDatas[0].rank == cardDatas[1].rank;

		return ret;
	}

	public override CombinationType GetCombinationType()
	{
		return CombinationType.Doubles;
	}
}