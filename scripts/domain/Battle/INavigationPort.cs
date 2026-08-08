// 导航端口：寻路目标与 RVO 配置
namespace CasualCastle.Domain.Battle;

public interface INavigationPort
{
	void SetTarget(float gameX, float gameY);
	void ConfigureRvo(float radius, float neighborDistance, float timeHorizon);
}
