// 导航服务端口：每帧驱动全体单位移动
namespace CasualCastle.Domain.Battle;

public interface INavigationService
{
	void Step(float dt);
}
