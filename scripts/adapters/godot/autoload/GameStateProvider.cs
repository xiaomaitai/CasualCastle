// 游戏状态桥接：向 MS DI 提供当前 IGameState 实例
using CasualCastle.Domain.Battle;

public class GameStateProvider
{
	public IGameState Current { get; set; }
}
