using Godot;

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
		return GameManager.Instance == null || GameManager.Instance.CanUnitWork(hasNightCombat);
	}
}
