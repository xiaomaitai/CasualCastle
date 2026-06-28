using Godot;

public partial class InitManager : Node
{
	public override void _Ready()
	{
		AddChild(new AdjacentSystem());
		AddChild(new BattleReportSystem());
		AddChild(new BuildingSystem());
		AddChild(new FusionSystem());
		AddChild(new ReplayAiSystem());
		AddChild(new CardSystem());
		AddChild(new ShopSystem());
		AddChild(new NightSystem());
	}
}
