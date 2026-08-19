using CasualCastle.Domain.Battle;
using Xunit;

namespace CasualCastle.Domain.Tests;

public class SkillRuntimeTests
{
	private static SkillRuntime CreateRuntimeWithStealth()
	{
		SkillSet skills = new SkillSet();
		skills.Add(UnitTestFactory.CreateStealthSkill(180f, 0.85f, 3f));
		return new SkillRuntime(skills);
	}

	[Fact]
	public void Start_SetsCooldownDuration()
	{
		SkillRuntime runtime = CreateRuntimeWithStealth();

		runtime.Start("stealth");

		Assert.Equal(3f, runtime.GetRemaining("stealth"), 3);
	}

	[Fact]
	public void Start_UnknownSkill_LeavesZero()
	{
		SkillRuntime runtime = CreateRuntimeWithStealth();

		runtime.Start("missing");

		Assert.Equal(0f, runtime.GetRemaining("missing"));
	}

	[Fact]
	public void Tick_ReducesAndClears()
	{
		SkillRuntime runtime = CreateRuntimeWithStealth();
		runtime.Start("stealth");

		runtime.Tick(2f);
		Assert.Equal(1f, runtime.GetRemaining("stealth"), 3);

		runtime.Tick(1f);
		Assert.Equal(0f, runtime.GetRemaining("stealth"));
	}

	[Fact]
	public void Reduce_ShortensAndClears()
	{
		SkillRuntime runtime = CreateRuntimeWithStealth();
		runtime.Start("stealth");

		runtime.Reduce(1f);
		Assert.Equal(2f, runtime.GetRemaining("stealth"), 3);

		runtime.Reduce(2f);
		Assert.Equal(0f, runtime.GetRemaining("stealth"));
	}
}
