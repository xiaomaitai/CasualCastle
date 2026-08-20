// 导航网格源端口：提供双方阻塞网格与变更版本
namespace CasualCastle.Domain.Battle;

public interface INavGridSource
{
	void Refresh();
	int Version { get; }
	NavGrid GetGrid(bool isPlayerUnit);
}
