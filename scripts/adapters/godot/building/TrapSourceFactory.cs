// 陷阱/瘟疫共用的领域单位源工厂：惰性创建查询用单位
public static class TrapSourceFactory
{
	public static CasualCastle.Domain.Battle.Unit Create(bool isPlayerUnit)
	{
		CasualCastle.Domain.Battle.Unit source = new(new TrapNavPort());
		source.IsPlayerUnit = isPlayerUnit;
		return source;
	}
}
