// 单位配置仓库端口
namespace CasualCastle.Domain.Battle;

public interface IUnitRepository
{
	UnitStats Get(string typeId);
}
