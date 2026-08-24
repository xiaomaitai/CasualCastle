using System.Collections.Generic;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class AllianceEndgameSkillTests
{
	private static SkillDef CreateAuraSkill(string bonusType, float value)
	{
		string json = "{\"target\":\"nearby_allies\",\"range\":150,\"race\":\"Human\",\"effects\":[{\"bonus_type\":\"" + bonusType + "\",\"value\":" + ValueText(value) + "}]}";
		return new SkillDef
		{
			Id = "aura_" + bonusType,
			DisplayName = "Aura",
			Type = SkillType.Aura,
			ConfigJson = json
		};
	}

	private static SkillDef CreateChargeSkill(float damageMult)
	{
		string json = "{\"behavior\":\"charge\",\"damage_mult\":" + ValueText(damageMult) + "}";
		return new SkillDef
		{
			Id = "royal_knight_charge",
			DisplayName = "冲锋",
			Type = SkillType.Special,
			ConfigJson = json
		};
	}

	private static SkillDef CreateSummonSkill(string unitTypeId, float cooldown)
	{
		string json = "{\"behavior\":\"summon\",\"unit_type\":\"" + unitTypeId + "\",\"cooldown\":" + ValueText(cooldown) + "}";
		return new SkillDef
		{
			Id = "king_summon",
			DisplayName = "召唤禁卫",
			Type = SkillType.Special,
			ConfigJson = json
		};
	}

	private static string ValueText(float value)
	{
		return value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
	}

	private static Unit CreateHumanUnit(bool isPlayerUnit)
	{
		Unit unit = UnitTestFactory.CreateUnit(AttackType.Melee, isPlayerUnit);
		unit.Race = "Human";
		return unit;
	}

	[Fact]
	public void HumanAura_HumanReceiver_AppliesAttackDamage()
	{
		Unit receiver = CreateHumanUnit(true);
		Unit emitter = CreateHumanUnit(true);
		emitter.Skills.Add(CreateAuraSkill("attack_damage", 0.10f));
		emitter.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, emitter });

		Assert.Equal(1.10f, effects.AttackDamageMultiplier, 4);
	}

	[Fact]
	public void HumanAura_HordeReceiver_NoEffect()
	{
		Unit receiver = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		receiver.Race = "Orc";
		Unit emitter = CreateHumanUnit(true);
		emitter.Skills.Add(CreateAuraSkill("attack_damage", 0.10f));
		emitter.SetPosition(30, 0);
		AuraService service = new();

		AuraEffects effects = service.CalculateEffects(receiver, new List<IUnitHandle> { receiver, emitter });

		Assert.Equal(1f, effects.AttackDamageMultiplier, 4);
	}

	[Fact]
	public void DamageReductionAura_ReducesIncomingDamage_RealBug_HumanAuraUnsupported()
	{
		Unit receiver = CreateHumanUnit(true);
		Unit emitter = CreateHumanUnit(true);
		emitter.Skills.Add(CreateAuraSkill("damage_reduction", 0.20f));
		emitter.SetPosition(30, 0);
		Unit attacker = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		FakeUnitRepository repo = new();
		repo.Register(receiver);
		repo.Register(emitter);
		repo.Register(attacker);
		SkillService service = new(new AuraService());

		service.UpdateContexts(repo.AllUnits);
		receiver.TakeDamage(100, attacker, 0f, 0f);

		Assert.Equal(20, receiver.Health);
	}

	[Fact]
	public void Charge_FirstAttackDealsDoubleDamage_SecondAttackNormal()
	{
		Unit knight = CreateHumanUnit(true);
		knight.Skills.Add(CreateChargeSkill(2f));
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		knight.SetPosition(0, 0);
		target.SetPosition(20, 0);
		knight.SetEnemyTarget(target);
		knight.UpdateTargeting(target);

		knight.UpdateBehavior(1f, 0f, 0f);
		int afterFirst = target.Health;
		knight.UpdateBehavior(1f, 0f, 0f);
		int afterSecond = target.Health;

		Assert.Equal(60, afterFirst);
		Assert.Equal(40, afterSecond);
	}

	[Fact]
	public void Summon_TriggersOnceThenWaitsCooldown()
	{
		Unit king = CreateHumanUnit(true);
		king.Skills.Add(CreateSummonSkill("RoyalGuard", 30f));
		FakeEventPort port = new();
		king.EventPort = port;
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(200, 0);
		king.SetEnemyTarget(target);
		king.UpdateTargeting(target);

		king.UpdateBehavior(1f, 0f, 0f);

		Assert.Equal(1, port.SummonCount);
		Assert.Equal("RoyalGuard", port.LastSummonType);

		king.UpdateBehavior(1f, 0f, 0f);

		Assert.Equal(1, port.SummonCount);
	}

	private sealed class FakeEventPort : IUnitEventPort
	{
		public int SummonCount;
		public string LastSummonType;

		public void OnSummon(string unitTypeId, float x, float y)
		{
			SummonCount++;
			LastSummonType = unitTypeId;
		}

		public void OnDamaged(int amount, IUnitHandle attacker) { }
		public void OnDied() { }
		public void OnAttack(IUnitHandle target, float targetGameX, float targetGameY) { }
		public void OnHeal(IUnitHandle target, int amount) { }
	}
}
