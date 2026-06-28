using CasualCastle.Domain.Battle;
using CasualCastle.Adapters.Godot;
using Godot;

public partial class NightSystem : Node
{
	public static NightSystem Instance { get; private set; }

	private IGameState _gameState;

	public override void _Ready()
	{
		Instance = this;
		AdapterRegistry.Register<NightSystem>(this);
		_gameState = AdapterRegistry.Resolve<IGameState>();
	}

	public override void _ExitTree()
	{
		if (Instance == this)
		{
			AdapterRegistry.Unregister<NightSystem>(this);
			Instance = null;
		}
	}

	public bool CanUnitWork(bool hasNightCombat)
	{
		if (_gameState == null)
			return true;
		return NightRules.CanUnitWork(hasNightCombat, _gameState.IsDay);
	}
}
