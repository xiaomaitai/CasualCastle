// 对局启动用例：阵营重置与战报开局
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;

namespace CasualCastle.Application;

public class MatchSessionUseCase
{
	private readonly BlackMagicService _blackMagicService;
	private readonly RitualService _ritualService;
	private readonly ComfortService _comfortService;
	private readonly KingExperienceService _kingXp;
	private readonly KingSkillSet _kingSkills;
	private readonly GameSessionPorts _ports;

	public MatchSessionUseCase(BlackMagicService blackMagicService, RitualService ritualService, ComfortService comfortService, KingExperienceService kingXp, KingSkillSet kingSkills, GameSessionPorts ports)
	{
		_blackMagicService = blackMagicService;
		_ritualService = ritualService;
		_comfortService = comfortService;
		_kingXp = kingXp;
		_kingSkills = kingSkills;
		_ports = ports;
	}

	public void StartNewMatch(string factionId, string pendingReplayReportId)
	{
		_ports.BattleReportCapture.StartMatch(pendingReplayReportId);

		if (factionId == FactionDef.NecromancerId)
		{
			_blackMagicService.Reset();
			UndeadFactionBuffs.Reset();
			_ritualService.Reset();
		}
		if (factionId == FactionDef.DungeonId)
		{
			_comfortService.Reset();
			_kingXp.Reset();
			_kingSkills.Reset();
			DungeonFactionBuffs.Reset();
		}
	}
}
