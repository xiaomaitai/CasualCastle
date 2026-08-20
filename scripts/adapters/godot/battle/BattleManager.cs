// 战斗管理：每帧推进潜行、技能上下文与导航移动
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;

public partial class BattleManager : Node
{
	private SkillService _skillService;
	private StealthService _stealthService;
	private INavigationService _navigation;

	public override void _Ready()
	{
		AdapterRegistry.Register<BattleManager>(this);
		_skillService = GameManager.Get<SkillService>();
		_stealthService = GameManager.Get<StealthService>();
		_navigation = GameManager.Get<INavigationService>();
	}

	public override void _ExitTree()
	{
		AdapterRegistry.Unregister<BattleManager>(this);
	}

	public override void _Process(double delta)
	{
		IFieldUnitRepository fieldRepo = GameManager.Get<IFieldUnitRepository>();
		if (fieldRepo == null)
			return;
		if (_stealthService != null)
		{
			foreach (IUnitHandle unit in fieldRepo.AllUnits)
				_stealthService.UpdateStealth(unit, fieldRepo);
		}
		if (_skillService != null)
			_skillService.UpdateContexts(fieldRepo.AllUnits);
		IGameState state = AdapterRegistry.Resolve<IGameState>();
		if (state != null && state.IsCombat && _navigation != null)
			_navigation.Step((float)delta);
	}
}
