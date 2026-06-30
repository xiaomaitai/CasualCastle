namespace CasualCastle.Domain.Battle;

public interface IBuildingTarget
{
	bool IsDestroyed { get; }
	void TakeDamage(int amount);
}
