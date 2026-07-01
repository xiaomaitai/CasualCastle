namespace CasualCastle.Domain.History;

public interface IReplayUseCase
{
	void ApplyNightSnapshot(IReplayTarget target, int nightIndex);
}
