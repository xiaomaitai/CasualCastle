using CasualCastle.Domain.Night;
using Godot;

// Thin adapter: delegates night combat check to domain NightRules.
public partial class NightSystem : Node
{
	public static NightSystem Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		if (Instance == this)
			Instance = null;
	}

	public static bool CanUnitWork(bool hasNightCombat)
	{
		if (GameManager.Instance == null)
			return true;
		bool isDay = GameManager.Instance.CurrentPhase == GameManager.GamePhase.Day;
		return NightRules.CanUnitWork(hasNightCombat, isDay);
	}
}
