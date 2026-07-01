using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;

public partial class BattleManager : Node
{
    public override void _Ready()
    {
        AdapterRegistry.Register<BattleManager>(this);
    }

    public override void _ExitTree()
    {
        AdapterRegistry.Unregister<BattleManager>(this);
    }

    public override void _Process(double delta)
    {
        IFieldUnitRepository fieldRepo = AdapterRegistry.Resolve<IFieldUnitRepository>();
        ICombatUseCase combatUseCase = GameManager.Get<ICombatUseCase>();
        if (fieldRepo != null && combatUseCase != null)
            combatUseCase.PushSoldiers(fieldRepo.AllUnits, (float)delta);
    }
}
