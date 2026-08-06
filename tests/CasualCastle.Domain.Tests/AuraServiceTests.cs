using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class AuraServiceTests
{
	private static SkillDef CreateAuraSkill(string bonusType, float value, float range = 150f, string race = "Goblin")
	{
		string json = "{\"target\":\"nearby_allies\",\"range\":" + ToText(range)
			+ ",\"race\":\"" + race + "\",\"bonus_type\":\"" + bonusType + "\",\"value\":" + ToText(value) + "}";
		return new SkillDef
		{
			Id = "aura_" + bonusType,
			DisplayName = "Aura",
			Type = SkillType.Aura,
			ConfigJson = json
		};
	}

	private static string ToText(float value)
	{
		return value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
	}

	private static Unit CreateHordeUnit(bool isPlayerUnit)
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, isPlayerUnit);
		unit.Race = "Orc";
		return unit;
	}

	private static Unit CreateEmitter(string race, string bonusType, float value, bool isPlayerUnit)
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, isPlayerUnit);
		unit.Race = race;
		unit.Skills.Add(CreateAuraSkill(bonusType, value, 150f, race));
		return unit;
	}

	[Fact]
	public void SameRaceAura_NotStacked()
	{
		Unit receiver = CreateHordeUnit(true);
		Unit emitterA = CreateEmitter("Goblin", "attack_speed", 0.12f, true);
		Unit emitterB = CreateEmitter("Goblin", "attack_speed", 0.12f, true);
		emitterA.SetPosition(30, 0);
		emitterB.SetPosition(-30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, emitterA, emitterB });

		Assert.Equal(1.12f, effects.AttackSpeedMultiplier, 4);
	}

	[Fact]
	public void DifferentRaceAuras_Stack()
	{
		Unit receiver = CreateHordeUnit(true);
		Unit goblin = CreateEmitter("Goblin", "attack_speed", 0.12f, true);
		Unit centaur = CreateEmitter("Centaur", "move_speed", 0.10f, true);
		goblin.SetPosition(30, 0);
		centaur.SetPosition(-30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, goblin, centaur });

		Assert.Equal(1.12f, effects.AttackSpeedMultiplier, 4);
		Assert.Equal(1.10f, effects.MoveSpeedMultiplier, 4);
	}

	[Fact]
	public void Aura_OutOfRange_NoEffect()
	{
		Unit receiver = CreateHordeUnit(true);
		Unit emitter = CreateEmitter("Goblin", "attack_speed", 0.12f, true);
		emitter.SetPosition(200, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, emitter });

		Assert.Equal(1f, effects.AttackSpeedMultiplier, 4);
	}

	[Fact]
	public void Aura_EnemyEmitter_NoEffect()
	{
		Unit receiver = CreateHordeUnit(true);
		Unit emitter = CreateEmitter("Goblin", "attack_speed", 0.12f, false);
		emitter.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, emitter });

		Assert.Equal(1f, effects.AttackSpeedMultiplier, 4);
	}

	[Fact]
	public void Aura_NonHordeReceiver_NoEffect()
	{
		Unit receiver = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		receiver.Race = "Human";
		Unit emitter = CreateEmitter("Goblin", "attack_speed", 0.12f, true);
		emitter.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, emitter });

		Assert.Equal(1f, effects.AttackSpeedMultiplier, 4);
	}

	[Fact]
	public void Aura_DeadEmitter_NoEffect()
	{
		Unit receiver = CreateHordeUnit(true);
		Unit emitter = CreateEmitter("Goblin", "attack_speed", 0.12f, true);
		emitter.IsAlive = false;
		emitter.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, emitter });

		Assert.Equal(1f, effects.AttackSpeedMultiplier, 4);
	}

	[Fact]
	public void Aura_AppliesToEmitterItself()
	{
		Unit goblin = CreateEmitter("Goblin", "attack_speed", 0.12f, true);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(goblin, new List<IUnitHandle> { goblin });

		Assert.Equal(1.12f, effects.AttackSpeedMultiplier, 4);
	}

	[Fact]
	public void RegenAura_AddsRegenPerSecond()
	{
		Unit receiver = CreateHordeUnit(true);
		Unit staglur = CreateEmitter("Staglur", "regen", 0.008f, true);
		staglur.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, staglur });

		Assert.Equal(0.008f, effects.RegenPerSecond, 4);
	}

	[Fact]
	public void ArmorAura_AddsArmorBonus()
	{
		Unit receiver = CreateHordeUnit(true);
		Unit staglur = CreateEmitter("Staglur", "armor", 8f, true);
		staglur.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, staglur });

		Assert.Equal(8, effects.ArmorBonus);
	}

	[Fact]
	public void ExecuteAura_AddsExecuteDamageMultiplier()
	{
		Unit receiver = CreateHordeUnit(true);
		Unit lizard = CreateEmitter("Lizard", "execute", 0.20f, true);
		lizard.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, lizard });

		Assert.Equal(1.20f, effects.ExecuteDamageMultiplier, 4);
	}

	[Fact]
	public void MultiAuraUnit_AppliesAllItsAuras()
	{
		Unit receiver = CreateHordeUnit(true);
		Unit staglur = CreateEmitter("Staglur", "regen", 0.008f, true);
		staglur.Skills.Add(CreateAuraSkill("armor", 8f, 150f, "Staglur"));
		staglur.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, staglur });

		Assert.Equal(0.008f, effects.RegenPerSecond, 4);
		Assert.Equal(8, effects.ArmorBonus);
	}
}