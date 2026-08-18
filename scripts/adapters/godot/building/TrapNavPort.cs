// 陷阱查询单位导航端口：空实现占位，不参与真实寻路
using CasualCastle.Domain.Battle;

internal sealed class TrapNavPort : INavigationPort
{
	public void SetTarget(float gameX, float gameY)
	{
	}

	public void ConfigureRvo(float radius, float neighborDistance, float timeHorizon)
	{
	}
}
