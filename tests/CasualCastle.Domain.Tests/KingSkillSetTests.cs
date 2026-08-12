using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class KingSkillSetTests
{
	[Fact]
	public void Add_WithLevel_RecordsLearnLevel()
	{
		KingSkillSet set = new();

		set.Add("blood_rage", 3);

		Assert.True(set.Contains("blood_rage"));
		Assert.Equal(3, set.GetLearnLevel("blood_rage"));
	}

	[Fact]
	public void Add_Duplicate_Ignored()
	{
		KingSkillSet set = new();
		set.Add("blood_rage", 2);

		set.Add("blood_rage", 4);

		Assert.Equal(1, set.Count);
		Assert.Equal(2, set.GetLearnLevel("blood_rage"));
	}

	[Fact]
	public void Reset_ClearsLevels()
	{
		KingSkillSet set = new();
		set.Add("blood_rage", 3);

		set.Reset();

		Assert.Equal(0, set.Count);
		Assert.Equal(1, set.GetLearnLevel("blood_rage"));
	}
}