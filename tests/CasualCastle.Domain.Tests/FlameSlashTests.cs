using CasualCastle.Domain.Battle;
using System.Collections.Generic;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class FlameSlashTests
{
	[Fact]
	public void KingAttack_WithFlameSlash_DamagesNearbyEnemies()
	{
		Unit king = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		king.Skills.Add(new SkillDef
		{
			Id = "flame_slash",
			DisplayName = "魔焰斩",
			Type = SkillType.OnHit,
			ConfigJson = "{\"effects\":[{\"type\":\"explosion\",\"value\":15,\"range\":70}]}"
		});
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit nearby = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		king.SetPosition(0f, 0f);
		target.SetPosition(50f, 0f);
		nearby.SetPosition(100f, 0f);
		king.SetEnemyTarget(target);
		king.UpdateTargeting(target);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		tactical.Enemies.Add(nearby);
		king.Tactical = tactical;

		king.UpdateBehavior(1f, 0f, 0f);

		Assert.True(target.Health < target.MaxHealth);
		Assert.True(nearby.Health < nearby.MaxHealth);
	}
}