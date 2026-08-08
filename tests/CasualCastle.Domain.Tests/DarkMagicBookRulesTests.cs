using System.Collections.Generic;
using CasualCastle.Domain.Building;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class DarkMagicBookRulesTests
{
	[Fact]
	public void GetRitualId_LoadedEntry_ReturnsMappedRitual()
	{
		DarkMagicBookRules.Load(new List<KeyValuePair<string, string>>
		{
			new("book_summon", RitualRules.UndeadSummonId),
			new("book_reinforce", RitualRules.BoneReinforceId)
		});

		Assert.Equal(RitualRules.UndeadSummonId, DarkMagicBookRules.GetRitualId("book_summon"));
		Assert.True(DarkMagicBookRules.IsDarkMagicBook("book_reinforce"));
		Assert.False(DarkMagicBookRules.IsDarkMagicBook("unknown"));
	}

	[Fact]
	public void Load_ReplacesPreviousMapping()
	{
		DarkMagicBookRules.Load(new List<KeyValuePair<string, string>> { new("book_a", "ritual_a") });
		DarkMagicBookRules.Load(new List<KeyValuePair<string, string>> { new("book_b", "ritual_b") });

		Assert.Null(DarkMagicBookRules.GetRitualId("book_a"));
		Assert.Equal("ritual_b", DarkMagicBookRules.GetRitualId("book_b"));
	}
}