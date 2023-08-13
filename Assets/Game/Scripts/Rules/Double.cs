using System;
using System.Collections.Generic;
using Game;

public class Double : CardCombination
{

	public Double(PlayerData ownerIn, List<CardData> cardDatasIn) : base(ownerIn, cardDatasIn) {}

	public override CardData GetStrongestCard()
	{
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