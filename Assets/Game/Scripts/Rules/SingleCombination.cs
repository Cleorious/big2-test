using System.Collections.Generic;
using Game;

public class SingleCombination : CardCombination
{
	public SingleCombination(PlayerData ownerIn, List<CardData> cardDatasIn) : base(ownerIn, cardDatasIn) {}

	public override CardData GetStrongestCard()
	{
		return cardDatas[0];
	}

	public override bool IsValid()
	{
		bool ret = false;

		ret = cardDatas.Count == Parameter.CARDCOMBO_COMBINATION_CARD_COUNTS[CombinationType.Singles];

		return ret;
	}

	public override CombinationType GetCombinationType()
	{
		return CombinationType.Singles;
	}
}