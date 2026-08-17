using CasualCastle.Application;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using System.Collections.Generic;
using Xunit;

namespace CasualCastle.Application.Tests;

public class MatchSessionUseCaseTests
{
	private static MatchSessionUseCase Create(RecordingCapture capture, BlackMagicService blackMagic, RitualService ritual, ComfortService comfort, KingExperienceService xp, KingSkillSet skills)
	{
		GameSessionPorts ports = TestPorts.Create(new RecordingQuery(), new RecordingCommand(), new RecordingShop(), capture);
		return new MatchSessionUseCase(blackMagic, ritual, comfort, xp, skills, ports);
	}

	[Fact]
	public void StartNewMatch_DungeonFaction_ResetsDungeonServices()
	{
		KingExperienceService xp = new();
		xp.Restore(100);
		KingSkillSet skills = new();
		skills.Add("skill_a", 2);
		RecordingCapture capture = new();
		MatchSessionUseCase useCase = Create(capture, new BlackMagicService(), new RitualService(), new ComfortService(), xp, skills);

		useCase.StartNewMatch(FactionDef.DungeonId, "report_1");

		Assert.Equal(0, xp.GetCumulativeExperience());
		Assert.Empty(skills.GetAll());
		Assert.Equal("report_1", capture.StartedReportId);
	}

	[Fact]
	public void StartNewMatch_NecromancerFaction_ResetsRitualService()
	{
		RitualService ritual = new();
		ritual.Restore(3, new List<string> { "ritual_x" });
		RecordingCapture capture = new();
		MatchSessionUseCase useCase = Create(capture, new BlackMagicService(), ritual, new ComfortService(), new KingExperienceService(), new KingSkillSet());

		useCase.StartNewMatch(FactionDef.NecromancerId, "");

		Assert.Equal(0, ritual.StudyCompletionCount);
		Assert.Empty(ritual.GetLearnedRitualIds());
	}

	[Fact]
	public void StartNewMatch_OtherFaction_LeavesServicesUntouched()
	{
		RitualService ritual = new();
		ritual.Restore(3, new List<string> { "ritual_x" });
		KingExperienceService xp = new();
		xp.Restore(50);
		RecordingCapture capture = new();
		MatchSessionUseCase useCase = Create(capture, new BlackMagicService(), ritual, new ComfortService(), xp, new KingSkillSet());

		useCase.StartNewMatch(FactionIds.Alliance, "report_2");

		Assert.Equal(3, ritual.StudyCompletionCount);
		Assert.Equal(50, xp.GetCumulativeExperience());
		Assert.Equal("report_2", capture.StartedReportId);
	}
}
