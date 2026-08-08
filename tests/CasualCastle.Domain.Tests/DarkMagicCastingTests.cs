using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class DarkMagicCastingTests
{
	private static Unit CreateDarkMage(BlackMagicService blackMagic)
	{
		Unit mage = UnitTestFactory.CreateUnit(AttackType.Ranged, true);
		mage.Damage = 12;
		mage.BlackMagic = blackMagic;
		mage.Skills.Add(new SkillDef
		{
			Id = "shadow_curse",
			DisplayName = "暗影诅咒",
			Type = SkillType.Special,
			ConfigJson = "{\"behavior\":\"curse\",\"param\":4.0,\"cost\":20,\"value\":0.25}"
		});
		mage.Skills.Add(new SkillDef
		{
			Id = "shadow_nova",
			DisplayName = "暗影新星",
			Type = SkillType.Special,
			ConfigJson = "{\"behavior\":\"shadow_nova\",\"param\":120,\"cost\":40,\"value\":30,\"cooldown\":6.0}"
		});
		return mage;
	}

	[Fact]
	public void DarkMage_CastsCurse_SpendsBlackMagicAndSlowsTarget()
	{
		BlackMagicService blackMagic = new();
		blackMagic.Add(20);
		Unit mage = CreateDarkMage(blackMagic);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(120, 0);
		mage.SetPosition(0, 0);
		mage.SetEnemyTarget(target);
		mage.UpdateTargeting(target);
		FakeTacticalQueries tactical = new();
		mage.Tactical = tactical;

		mage.UpdateBehavior(2f, 0, 0);

		Assert.Equal(0, blackMagic.Value);
		Assert.Equal(target.Speed * 0.75f, target.GetCurrentSpeed(), 4);
		Assert.Equal(100, target.Health);
	}

	[Fact]
	public void DarkMage_CastsNova_SpendsBlackMagicAndDamagesNearbyEnemies()
	{
		BlackMagicService blackMagic = new();
		blackMagic.Add(40);
		Unit mage = CreateDarkMage(blackMagic);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		Unit nearby = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(120, 0);
		nearby.SetPosition(140, 0);
		mage.SetPosition(0, 0);
		mage.SetEnemyTarget(target);
		mage.UpdateTargeting(target);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		tactical.Enemies.Add(nearby);
		mage.Tactical = tactical;

		mage.UpdateBehavior(2f, 0, 0);

		Assert.Equal(0, blackMagic.Value);
		Assert.Equal(100 - 30, target.Health);
		Assert.Equal(100 - 30, nearby.Health);
	}

	[Fact]
	public void DarkMage_InsufficientBlackMagic_FallsBackToNormalAttack()
	{
		BlackMagicService blackMagic = new();
		Unit mage = CreateDarkMage(blackMagic);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(120, 0);
		mage.SetPosition(0, 0);
		mage.SetEnemyTarget(target);
		mage.UpdateTargeting(target);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		mage.Tactical = tactical;

		mage.UpdateBehavior(2f, 0, 0);

		Assert.Equal(0, blackMagic.Value);
		Assert.Equal(100 - 12, target.Health);
	}

	[Fact]
	public void DarkMage_SingleEnemy_NovaNotCast_CurseCastInstead()
	{
		BlackMagicService blackMagic = new();
		blackMagic.Add(60);
		Unit mage = CreateDarkMage(blackMagic);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(120, 0);
		mage.SetPosition(0, 0);
		mage.SetEnemyTarget(target);
		mage.UpdateTargeting(target);
		FakeTacticalQueries tactical = new();
		tactical.Enemies.Add(target);
		mage.Tactical = tactical;

		mage.UpdateBehavior(2f, 0, 0);

		Assert.Equal(40, blackMagic.Value);
		Assert.Equal(100, target.Health);
		Assert.Equal(target.Speed * 0.75f, target.GetCurrentSpeed(), 4);
	}

	[Fact]
	public void Curse_ReducesTargetAttackDamage()
	{
		BlackMagicService blackMagic = new();
		blackMagic.Add(20);
		Unit mage = CreateDarkMage(blackMagic);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		target.SetPosition(120, 0);
		mage.SetPosition(0, 0);
		mage.SetEnemyTarget(target);
		mage.UpdateTargeting(target);
		FakeTacticalQueries tactical = new();
		mage.Tactical = tactical;

		mage.UpdateBehavior(2f, 0, 0);

		target.SetEnemyTarget(mage);
		target.UpdateTargeting(mage);
		target.UpdateBehavior(2f, 0, 0);

		Assert.Equal(100 - (int)(20 * 0.75f), mage.Health);
	}
}