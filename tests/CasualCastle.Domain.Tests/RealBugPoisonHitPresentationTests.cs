using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class RealBugPoisonHitPresentationTests
{
	[Fact]
	public void PoisonTick_FiresDamagedEvent_WithDotFlag_RealBug_RapidHitSounds()
	{
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		RecordingEventPort port = new();
		target.EventPort = port;

		target.ApplyPoison(4f, 3f);
		target.UpdateBehavior(0.3f, 0, 0);

		Assert.True(port.DamagedCount > 0);
		Assert.True(port.LastIsDot);
	}

	[Fact]
	public void DirectHit_FiresDamagedEvent_NotDot_RealBug()
	{
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		RecordingEventPort port = new();
		target.EventPort = port;
		target.SetPosition(80, 0);
		self.SetPosition(0, 0);
		self.SetEnemyTarget(target);
		self.UpdateTargeting(target);

		self.UpdateBehavior(0.6f, 0, 0);

		Assert.True(port.DamagedCount > 0);
		Assert.False(port.LastIsDot);
	}

	[Fact]
	public void ExecuteHeavyHit_FiresDamagedEvent_WithHeavyFlag_RealBug()
	{
		Unit self = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		self.GameCtx.AuraExecuteDamageMultiplier = 2f;
		Unit target = UnitTestFactory.CreateUnit(AttackType.Melee, false);
		RecordingEventPort port = new();
		target.EventPort = port;
		target.TakeDamage(85, null, 0, 0);
		target.SetPosition(80, 0);
		self.SetPosition(0, 0);
		self.SetEnemyTarget(target);
		self.UpdateTargeting(target);

		self.UpdateBehavior(0.6f, 0, 0);

		Assert.True(port.LastIsHeavy);
	}

	private sealed class RecordingEventPort : IUnitEventPort
	{
		public int DamagedCount { get; private set; }
		public bool LastIsDot { get; private set; }
		public bool LastIsHeavy { get; private set; }

		public void OnDamaged(int amount, IUnitHandle attacker, bool isDot, bool isHeavy)
		{
			DamagedCount++;
			LastIsDot = isDot;
			LastIsHeavy = isHeavy;
		}

		public void OnDied() { }

		public void OnAttack(IUnitHandle target, float targetGameX, float targetGameY) { }

		public void OnHeal(IUnitHandle target, int amount) { }

		public void OnSummon(string unitTypeId, float x, float y) { }
	}
}
