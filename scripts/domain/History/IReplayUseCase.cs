// 回放用例端口
namespace CasualCastle.Domain.History;

public interface IReplayUseCase
{
	void ApplyNightSnapshot(IReplayTarget target, int nightIndex, BattlePhase phase);
}
