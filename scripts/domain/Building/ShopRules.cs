// 商店规则：卡池过滤、唯一魔法书与权重抽取
using System;
using System.Collections.Generic;

namespace CasualCastle.Domain.Building;

public class ShopRules
{
	public const int OfferCount = 5;

	private readonly CardData[] _allCatalog;
	private CardData[] _catalog = System.Array.Empty<CardData>();
	private Func<string, bool> _isRitualLearned = _ => false;
	private Func<string, bool> _isBookHeld = _ => false;
	private Func<int> _getCoreLevel = () => 1;
	private Func<int> _getNightNumber = () => 0;

	public ShopRules(IReadOnlyList<CardData> catalog)
	{
		_allCatalog = new CardData[catalog.Count];
		for (int i = 0; i < catalog.Count; i++)
			_allCatalog[i] = catalog[i];
		SetFaction("");
	}

	public void SetAvailability(Func<string, bool> isRitualLearned, Func<string, bool> isBookHeld, Func<int> getCoreLevel)
	{
		SetAvailability(isRitualLearned, isBookHeld, getCoreLevel, () => 0);
	}

	public void SetAvailability(Func<string, bool> isRitualLearned, Func<string, bool> isBookHeld, Func<int> getCoreLevel, Func<int> getNightNumber)
	{
		if (isRitualLearned != null)
			_isRitualLearned = isRitualLearned;
		if (isBookHeld != null)
			_isBookHeld = isBookHeld;
		if (getCoreLevel != null)
			_getCoreLevel = getCoreLevel;
		if (getNightNumber != null)
			_getNightNumber = getNightNumber;
	}

	public void SetFaction(string factionId)
	{
		List<CardData> filtered = new();
		foreach (CardData card in _allCatalog)
		{
			if (!string.IsNullOrEmpty(factionId) && !string.IsNullOrEmpty(card.FactionId) && card.FactionId != factionId)
				continue;
			filtered.Add(card);
		}
		_catalog = filtered.ToArray();
	}

	public CardData[] GenerateOffers(Random random)
	{
		CardData[] offers = new CardData[OfferCount];
		HashSet<string> pickedBooks = new();
		for (int i = 0; i < OfferCount; i++)
			offers[i] = PickWeighted(random, pickedBooks);
		return offers;
	}

	private CardData PickWeighted(Random random, HashSet<string> pickedBooks)
	{
		int totalWeight = 0;
		foreach (CardData card in _catalog)
		{
			if (IsCardOffered(card, pickedBooks))
				totalWeight += card.Weight;
		}
		if (totalWeight <= 0)
			return null;

		int roll = random.Next(totalWeight);
		int cumulative = 0;
		foreach (CardData card in _catalog)
		{
			if (!IsCardOffered(card, pickedBooks))
				continue;
			cumulative += card.Weight;
			if (roll >= cumulative)
				continue;
			if (DarkMagicBookRules.IsDarkMagicBook(card))
				pickedBooks.Add(card.Id);
			return card;
		}
		return null;
	}

	private bool IsCardOffered(CardData card, HashSet<string> pickedBooks)
	{
		if (card.UnlockNight > _getNightNumber() + 1)
			return false;
		if (!string.IsNullOrEmpty(card.UnlockRitualId) && !_isRitualLearned(card.UnlockRitualId))
			return false;
		if (!DarkMagicBookRules.IsDarkMagicBook(card))
			return true;

		string ritualId = DarkMagicBookRules.GetRitualId(card);
		RitualDef def = ritualId == null ? null : RitualRules.Get(ritualId);
		if (def == null || def.RequiredCoreLevel > _getCoreLevel())
			return false;
		if (_isBookHeld(card.Id) || _isRitualLearned(ritualId) || pickedBooks.Contains(card.Id))
			return false;
		return true;
	}
}
