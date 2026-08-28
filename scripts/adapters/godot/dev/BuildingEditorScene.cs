// 建筑编辑器场景控制器
using CasualCastle.Adapters.Godot.Dev;
using CasualCastle.Domain.Building;
using Godot;

public partial class BuildingEditorScene : Control
{
	private BuildingEditorController _controller;

	public override void _Ready()
	{
		ITechTreeRepository repo = GameManager.Get<ITechTreeRepository>();
		_controller = new BuildingEditorController(this, repo);
		CallDeferred(nameof(LoadDeferred));
	}

	private void LoadDeferred()
	{
		_controller.LoadInitialData();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			_controller?.Dispose();
		base.Dispose(disposing);
	}
}
