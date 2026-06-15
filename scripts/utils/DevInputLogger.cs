using Godot;

public partial class DevInputLogger : Node
{
	public override void _Ready()
	{
		SetProcessInput(true);
		GD.Print("[KeyLog] DevInputLogger ready");
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
			return;

		GD.Print(
			$"[KeyLog] Keycode={keyEvent.Keycode} Physical={keyEvent.PhysicalKeycode} " +
			$"Unicode={keyEvent.Unicode} Shift={keyEvent.ShiftPressed} Ctrl={keyEvent.CtrlPressed} " +
			$"Alt={keyEvent.AltPressed} Location={keyEvent.Location}");
	}
}
