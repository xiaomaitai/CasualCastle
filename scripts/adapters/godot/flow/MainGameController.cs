// 主对局场景入口：组装战场并启动对局
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;

public partial class MainGameController : Node2D
{
	public override void _Ready()
	{
		Node2D battlefield = GetNode<Node2D>("Battlefield");
		Castle playerCastle = GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
		Castle enemyCastle = GetNode<Castle>("Battlefield/EnemySide/EnemyCastle");

		AdapterRegistry.Resolve<GameManager>().StartGameSession(battlefield, playerCastle, enemyCastle);
		GameManager.Get<AdjacencyService>().RefreshCastle(playerCastle.GetBuildingStates());
		GameManager.Get<AdjacencyService>().RefreshCastle(enemyCastle.GetBuildingStates());
	}

	public override void _ExitTree()
	{
		AdapterRegistry.Resolve<GameManager>().ClearGameSession();
	}
}
