using CasualCastle.Domain.Shared;
using System.Collections.Generic;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class BgmSelectionRulesTests
{
	[Fact]
	public void FirstDay_GameplayPool_ReturnsOnlySoothingTracks()
	{
		List<BgmDef> pool = CreateMixedPool(BgmPoolIds.AllianceDay);

		List<BgmDef> selected = BgmSelectionRules.SelectTracks(pool, BgmPoolIds.AllianceDay, 0);

		Assert.Single(selected);
		Assert.Equal(BgmStyle.Soothing, selected[0].Style);
	}

	[Fact]
	public void LaterDay_GameplayPool_ReturnsOnlyStirringTracks()
	{
		List<BgmDef> pool = CreateMixedPool(BgmPoolIds.AllianceNight);

		List<BgmDef> selected = BgmSelectionRules.SelectTracks(pool, BgmPoolIds.AllianceNight, 1);

		Assert.Single(selected);
		Assert.Equal(BgmStyle.Stirring, selected[0].Style);
	}

	[Fact]
	public void LaterDay_SoothingTrack_IsNeverSelected()
	{
		List<BgmDef> pool = CreateMixedPool(BgmPoolIds.HordeNight);

		List<BgmDef> selected = BgmSelectionRules.SelectTracks(pool, BgmPoolIds.HordeNight, 5);

		foreach (BgmDef def in selected)
			Assert.Equal(BgmStyle.Stirring, def.Style);
	}

	[Fact]
	public void TitlePool_IgnoresDayRule()
	{
		List<BgmDef> pool = CreateMixedPool(BgmPoolIds.Title);

		List<BgmDef> selected = BgmSelectionRules.SelectTracks(pool, BgmPoolIds.Title, 2);

		Assert.Equal(2, selected.Count);
	}

	[Fact]
	public void EmptyStyleFilter_FallsBackToFullPool()
	{
		List<BgmDef> pool = new List<BgmDef>
		{
			CreateDef(1, BgmStyle.Stirring, BgmPoolIds.HordeDay),
			CreateDef(2, BgmStyle.Stirring, BgmPoolIds.HordeDay),
		};

		List<BgmDef> selected = BgmSelectionRules.SelectTracks(pool, BgmPoolIds.HordeDay, 0);

		Assert.Equal(2, selected.Count);
	}

	private static List<BgmDef> CreateMixedPool(string pool)
	{
		return new List<BgmDef>
		{
			CreateDef(1, BgmStyle.Soothing, pool),
			CreateDef(2, BgmStyle.Stirring, pool),
		};
	}

	private static BgmDef CreateDef(int id, BgmStyle style, string pool)
	{
		return new BgmDef
		{
			Id = id,
			Name = $"BGM {id}",
			Style = style,
			Pool = pool,
			Uid = $"uid://bgm_{pool}_{id}",
		};
	}
}