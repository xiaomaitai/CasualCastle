using CasualCastle.Domain.Battle;
using System.Collections.Generic;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class AuraRangeBonusTests
{
	[Fact]
	public void AuraRangeMultiplier_ExtendsAuraReach()
	{
		SkillDef aura = new()
		{
			Id = "succubus_aura",
			DisplayName = "魅惑",
			Type = SkillType.Aura,
			ConfigJson = "{\"target\":\"nearby_allies\",\"range\":150,\"race\":\"Demon\",\"bonus_type\":\"attack_damage\",\"value\":0.15}"
		};
		Unit king = UnitTestFactory.CreateUnit(AttackType.Melee, true);
		king.Race = DemonKingRules.DemonKingRace;
		king.SetPosition(0f, 0f);
		Unit succubus = UnitTestFactory.CreateUnit(AttackType.Ranged, true);
		succubus.Race = DemonKingRules.DemonRace;
		succubus.Skills.Add(aura);
		succubus.SetPosition(200f, 0f);
		AuraService service = new();
		DungeonFactionBuffs.AuraRangeMultiplier = 1.5f;

		try
		{
			AuraEffects effects = service.CalculateEffects(king, new List<IUnitHandle> { king, succubus });

			Assert.Equal(1.15f, effects.AttackDamageMultiplier, 2);
		}
		finally
		{
			DungeonFactionBuffs.Reset();
		}
	}
}