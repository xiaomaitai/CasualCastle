using CasualCastle.Adapters.Godot.Dev;
using CasualCastle.Domain.Building;
using Godot;

public partial class TechTreeEditorScene : Control
{
	private TechTreeEditorController _controller;

	public override void _Ready()
	{
		ITechTreeRepository repo = GameManager.Get<ITechTreeRepository>();
		_controller = new TechTreeEditorController(this, repo);
		CallDeferred(nameof(LoadDeferred));
	}

	private void LoadDeferred()
	{
		_controller.LoadInitialData();
	}

	public override void _Process(double delta)
	{
		_controller.Process();
	}

	public override void _Input(InputEvent @event)
	{
		_controller.HandleGlobalInput(@event);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			_controller?.Dispose();
		base.Dispose(disposing);
	}
}
