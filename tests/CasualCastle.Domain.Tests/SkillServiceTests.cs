using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class SkillServiceTests
{
	[Fact]
	public void UpdateContexts_AppliesGlobalRegenToContext()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		FakeUnitRepository repo = new();
		repo.Register(unit);
		SkillService service = new(new AuraService());
		DungeonFactionBuffs.GlobalRegenPerSecond = 0.01f;

		try
		{
			service.UpdateContexts(repo.AllUnits);

			Assert.Equal(0.01f, unit.GameCtx.AuraRegenPerSecond, 3);
		}
		finally
		{
			DungeonFactionBuffs.Reset();
		}
	}

	[Fact]
	public void UpdateContexts_ResetsWhenNoGlobalRegen()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		FakeUnitRepository repo = new();
		repo.Register(unit);
		SkillService service = new(new AuraService());

		service.UpdateContexts(repo.AllUnits);

		Assert.Equal(0f, unit.GameCtx.AuraRegenPerSecond);
	}

	[Fact]
	public void UpdateContexts_AppliesPleasureStageAttackMultiplier_RealBug_StageBuffMissing()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		FakeUnitRepository repo = new();
		repo.Register(unit);
		SkillService service = new(new AuraService());
		DungeonFactionBuffs.AttackDamageMultiplier = DungeonFactionBuffs.PleasureStageAttackMultiplier;

		try
		{
			service.UpdateContexts(repo.AllUnits);

			Assert.Equal(DungeonFactionBuffs.PleasureStageAttackMultiplier, unit.GameCtx.AuraAttackDamageMultiplier, 3);
		}
		finally
		{
			DungeonFactionBuffs.Reset();
		}
	}

	[Fact]
	public void UpdateContexts_NoPleasureStage_AttackMultiplierIsOne()
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		FakeUnitRepository repo = new();
		repo.Register(unit);
		SkillService service = new(new AuraService());

		service.UpdateContexts(repo.AllUnits);

		Assert.Equal(1f, unit.GameCtx.AuraAttackDamageMultiplier);
	}
}