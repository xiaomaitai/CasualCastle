using Godot;

public partial class MainGameController : Node2D
{
    public override void _EnterTree()
    {
        var battlefield = GetNode<Node2D>("Battlefield");
        var playerCastle = GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
        GameManager.Instance.StartGameSession(battlefield, playerCastle);
    }

    public override void _Ready()
    {
        var playerCastle = GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
        var enemyCastle = GetNode<Castle>("Battlefield/EnemySide/EnemyCastle");
        AdjacentSystem.Instance?.RefreshCastle(playerCastle);
        AdjacentSystem.Instance?.RefreshCastle(enemyCastle);
    }

    public override void _ExitTree()
    {
        GameManager.Instance.ClearGameSession();
    }
}
