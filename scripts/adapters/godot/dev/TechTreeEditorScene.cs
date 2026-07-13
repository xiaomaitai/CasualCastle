using CasualCastle.Adapters.Godot.Dev;
using Godot;

public partial class TechTreeEditorScene : Control
{
    private TechTreeEditorController _controller;

    public override void _Ready()
    {
        SetProcessInput(true);
        _controller = new TechTreeEditorController(this);
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
