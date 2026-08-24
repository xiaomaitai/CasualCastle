// 死灵法师平衡配置端口：黑魔法产出数值可调
namespace CasualCastle.Domain.Battle;

public interface INecromancerBalanceConfig
{
	int FriendlyDeathYield { get; }
	int EnemyDeathYield { get; }
}
