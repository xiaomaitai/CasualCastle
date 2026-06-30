namespace CasualCastle.Domain.Battle;

public interface IUnitRepository
{
	UnitStats Get(string typeId);
}
