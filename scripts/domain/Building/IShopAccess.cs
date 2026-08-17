// 商店访问端口：阶段副作用驱动的商店操作
namespace CasualCastle.Domain.Building;

public interface IShopAccess
{
	void SetShopAvailable(bool available);
	void RefreshOffers();
	void RequestOpenShop();
	void SetFaction(string factionId);
}
