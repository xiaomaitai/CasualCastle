using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;

public partial class MainGameController : Node2D
{
    public override void _Ready()
    {
        Node2D battlefield = GetNode<Node2D>("Battlefield");
        Castle playerCastle = GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
        Castle enemyCastle = GetNode<Castle>("Battlefield/EnemySide/EnemyCastle");

        NavigationRegion2D navRegion = GetNode<NavigationRegion2D>("Battlefield/NavigationRegion");
        NavigationPolygon navPoly = new NavigationPolygon();
        navPoly.AddOutline(new Vector2[] {
            new Vector2(0, 0),
            new Vector2(1920, 0),
            new Vector2(1920, 1080),
            new Vector2(0, 1080)
        });
        navRegion.NavigationPolygon = navPoly;
        navRegion.BakeNavigationPolygon();

        AdapterRegistry.Resolve<GameManager>().StartGameSession(battlefield, playerCastle, enemyCastle);
        AdapterRegistry.Resolve<AdjacencyService>().RefreshCastle(playerCastle.GetBuildingStates());
        AdapterRegistry.Resolve<AdjacencyService>().RefreshCastle(enemyCastle.GetBuildingStates());
    }

    public override void _ExitTree()
    {
        AdapterRegistry.Resolve<GameManager>().ClearGameSession();
    }
}
