// RVO 配置服务端口
namespace CasualCastle.Domain.Battle;

public interface IRvoService
{
	void ConfigureRvo(INavigationPort navPort, float collisionRadius);
}
