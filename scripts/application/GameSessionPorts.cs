// 对局会话端口集合：运行时由场景桥接注入
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;

namespace CasualCastle.Application;

public class GameSessionPorts
{
	public IGameSessionQuery Query { get; set; }
	public IGameSessionCommand Command { get; set; }
	public IShopAccess Shop { get; set; }
	public IBattleReportCapture BattleReportCapture { get; set; }
}
