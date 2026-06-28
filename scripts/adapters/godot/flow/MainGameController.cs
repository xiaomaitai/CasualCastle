using CasualCastle.Adapters.Godot;
using Godot;

public partial class MainGameController : Node2D
{
    public override void _Ready()
    {
        Node2D battlefield = GetNode<Node2D>("Battlefield");
        Castle playerCastle = GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
        Castle enemyCastle = GetNode<Castle>("Battlefield/EnemySide/EnemyCastle");

        AdapterRegistry.Resolve<GameManager>().StartGameSession(battlefield, playerCastle, enemyCastle);
        AdapterRegistry.Resolve<AdjacentSystem>()?.RefreshCastle(playerCastle);
        AdapterRegistry.Resolve<AdjacentSystem>()?.RefreshCastle(enemyCastle);
    }

    public override void _ExitTree()
    {
        AdapterRegistry.Resolve<GameManager>().ClearGameSession();
    }
}
