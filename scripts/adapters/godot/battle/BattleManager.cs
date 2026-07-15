using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;

public partial class BattleManager : Node
{
	private SkillService _skillService;

	public override void _Ready()
	{
		AdapterRegistry.Register<BattleManager>(this);
		_skillService = GameManager.Get<SkillService>();
	}

	public override void _ExitTree()
	{
		AdapterRegistry.Unregister<BattleManager>(this);
	}

	public override void _Process(double delta)
	{
		IFieldUnitRepository fieldRepo = GameManager.Get<IFieldUnitRepository>();
		ICombatUseCase combatUseCase = GameManager.Get<ICombatUseCase>();
		if (fieldRepo != null && combatUseCase != null)
		{
			if (_skillService != null)
				_skillService.UpdateContexts(fieldRepo.AllUnits);
			combatUseCase.PushSoldiers(fieldRepo.AllUnits, (float)delta);
		}
	}
}
