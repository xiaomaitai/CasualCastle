using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;

public partial class BattleManager : Node
{
	private SkillService _skillService;
	private StealthService _stealthService;

	public override void _Ready()
	{
		AdapterRegistry.Register<BattleManager>(this);
		_skillService = GameManager.Get<SkillService>();
		_stealthService = GameManager.Get<StealthService>();
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
			if (_stealthService != null)
			{
				foreach (IUnitHandle unit in fieldRepo.AllUnits)
					_stealthService.UpdateStealth(unit, fieldRepo);
			}
			if (_skillService != null)
				_skillService.UpdateContexts(fieldRepo.AllUnits);
			combatUseCase.PushUnits(fieldRepo.AllUnits, fieldRepo.AllBuildings, (float)delta);
		}
	}
}
