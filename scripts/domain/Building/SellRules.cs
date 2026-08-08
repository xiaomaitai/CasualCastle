// 卖出价格规则：半价向上取整
namespace CasualCastle.Domain.Building;

public static class SellRules
{

	public static int GetSellPrice(int value)
	{
		return (value + 1) / 2;
	}
}
