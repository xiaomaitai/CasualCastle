using Godot;

public partial class MainGameController : Node2D
{
    private static readonly StringName PauseOverlayName = "PauseOverlay";
    private static readonly StringName GameOverOverlayName = "GameOverOverlay";
    private static readonly StringName SettingsPanelName = "SettingsPanel";

    public override void _Ready()
    {
        ScaleLayoutToDesignResolution();

        var battlefield = GetNode<Node2D>("Battlefield");
        var playerCastle = GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
        var enemyCastle = GetNode<Castle>("Battlefield/EnemySide/EnemyCastle");

        GameManager.Instance.StartGameSession(battlefield, playerCastle, enemyCastle);
        AdjacentSystem.Instance?.RefreshCastle(playerCastle);
        AdjacentSystem.Instance?.RefreshCastle(enemyCastle);
    }

    public override void _ExitTree()
    {
        GameManager.Instance.ClearGameSession();
    }

    private void ScaleLayoutToDesignResolution()
    {
        GetNode<Node2D>("Battlefield").Scale = GameConfig.LayoutScale;

        var ui = GetNode<CanvasLayer>("UI");
        foreach (Node child in ui.GetChildren())
        {
            if (child.Name == PauseOverlayName || child.Name == GameOverOverlayName
                || child.Name == SettingsPanelName)
                continue;

            if (child is Node2D node2D)
                node2D.Scale = GameConfig.LayoutScale;
            else if (child is Control control)
                control.Scale = GameConfig.LayoutScale;
        }
    }
}
