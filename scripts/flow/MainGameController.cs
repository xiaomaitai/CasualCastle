using Godot;

public partial class MainGameController : Node2D
{
    public override void _EnterTree()
    {
        var battlefield = GetNode<Node2D>("Battlefield");
        var playerCastle = GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
        GameManager.Instance.StartGameSession(battlefield, playerCastle);
    }

    public override void _ExitTree()
    {
        GameManager.Instance.ClearGameSession();
    }
}
