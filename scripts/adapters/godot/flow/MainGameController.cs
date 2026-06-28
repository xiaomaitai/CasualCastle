using Godot;

public partial class MainGameController : Node2D
{
    public override void _Ready()
    {
        Node2D battlefield = GetNode<Node2D>("Battlefield");
        Castle playerCastle = GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
        Castle enemyCastle = GetNode<Castle>("Battlefield/EnemySide/EnemyCastle");

        GameManager.Instance.StartGameSession(battlefield, playerCastle, enemyCastle);
        AdjacentSystem.Instance?.RefreshCastle(playerCastle);
        AdjacentSystem.Instance?.RefreshCastle(enemyCastle);
    }

    public override void _ExitTree()
    {
        GameManager.Instance.ClearGameSession();
    }
}
