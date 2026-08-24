// 死灵法师平衡默认配置：DB 缺失时回退的产出数值
namespace CasualCastle.Domain.Battle;

public sealed class DefaultNecromancerBalanceConfig : INecromancerBalanceConfig
{
	public int FriendlyDeathYield => BlackMagicService.DefaultFriendlyDeathYield;
	public int EnemyDeathYield => BlackMagicService.DefaultEnemyDeathYield;
}
