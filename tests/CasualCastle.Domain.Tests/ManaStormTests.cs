using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class ManaStormTests
{
	private static Unit CreateKingWithManaStorm(ManaService mana)
	{
		Unit king = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		king.Resources = new FactionResources(null, mana, null);
		king.Skills.Add(new SkillDef
		{
			Id = "mana_storm",
			DisplayName = "魔力风暴",
			Type = SkillType.Special,
			ConfigJson = "{\"behavior\":\"mana_storm\",\"param\":120,\"cost\":40,\"value\":30,\"cooldown\":8.0}"
		});
		return king;
	}

	[Fact]
	public void King_CastsManaStorm_SpendsManaAndDamagesNearbyEnemies()
	{
		ManaService mana = new();
		mana.Add(40f);
		Unit king = CreateKingWithManaStorm(mana);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit nearby = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(120f, 0f);
		nearby.SetPosition(140f, 0f);
		king.SetPosition(0f, 0f);
		king.SetEnemyTarget(target);
		king.UpdateTargeting(target);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		tactical.Enemies.Add(nearby);
		king.Tactical = tactical;

		king.UpdateBehavior(2f, 0f, 0f);

		Assert.Equal(0f, mana.Value);
		Assert.Equal(70, target.Health);
		Assert.Equal(70, nearby.Health);
	}

	[Fact]
	public void King_InsufficientMana_FallsBackToNormalAttack()
	{
		ManaService mana = new();
		Unit king = CreateKingWithManaStorm(mana);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(120f, 0f);
		king.SetPosition(0f, 0f);
		king.SetEnemyTarget(target);
		king.UpdateTargeting(target);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		king.Tactical = tactical;

		king.UpdateBehavior(2f, 0f, 0f);

		Assert.Equal(80, target.Health);
	}
}