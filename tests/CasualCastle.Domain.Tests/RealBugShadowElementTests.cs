using System;
using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class RealBugShadowElementTests
{
	[Fact]
	public void Shadow_IsDamageElement_NotDamageType()
	{
		Assert.DoesNotContain("Shadow", Enum.GetNames<DamageType>());
	}
}