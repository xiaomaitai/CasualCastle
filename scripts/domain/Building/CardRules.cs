// 手牌规则：上限与索引校验
namespace CasualCastle.Domain.Building;

public static class CardRules
{
	public const int MaxHandSize = 10;

	public static bool CanAddCard(int currentHandSize) => currentHandSize < MaxHandSize;

	public static bool IsValidHandIndex(int index, int handSize) =>
		index >= 0 && index < handSize;
}
