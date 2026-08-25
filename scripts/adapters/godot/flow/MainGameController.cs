// 主对局场景入口：组装战场、启动对局并按昼夜显示切换战场色调
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;

public partial class MainGameController : Node2D
{
	private const string NightTintPath = "Battlefield/NightTint";
	private static readonly Color DayTint = Colors.White;
	private static readonly Color NightTintColor = new(0.72f, 0.78f, 0.95f, 1f);

	private CanvasModulate _nightTint;
	private GameManager _gameManager;

	public override void _Ready()
	{
		_nightTint = GetNode<CanvasModulate>(NightTintPath);
		Node2D battlefield = GetNode<Node2D>("Battlefield");
		Castle playerCastle = GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
		Castle enemyCastle = GetNode<Castle>("Battlefield/EnemySide/EnemyCastle");

		_gameManager = AdapterRegistry.Resolve<GameManager>();
		_gameManager.StartGameSession(battlefield, playerCastle, enemyCastle);
		GameManager.Get<AdjacencyService>().RefreshCastle(playerCastle.GetBuildingStates());
		GameManager.Get<AdjacencyService>().RefreshCastle(enemyCastle.GetBuildingStates());

		_gameManager.PhaseChanged += OnPhaseChanged;
		OnPhaseChanged(_gameManager.CurrentPhase);
	}

	public override void _ExitTree()
	{
		if (AdapterRegistry.Resolve<GameManager>() != null)
			AdapterRegistry.Resolve<GameManager>().PhaseChanged -= OnPhaseChanged;
		AdapterRegistry.Resolve<GameManager>().ClearGameSession();
	}

	private void OnPhaseChanged(MatchPhase phase)
	{
		_nightTint.Color = _gameManager.IsNightDisplayed ? NightTintColor : DayTint;
	}
}
