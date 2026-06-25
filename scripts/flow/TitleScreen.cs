using Godot;

public partial class TitleScreen : Control
{
	private const string MainGameScene = "res://scenes/main/main_game.tscn";

	private SettingsUiController _settingsUi;

	public override void _Ready()
	{
		GetNode<Button>("UI/StartButton").Pressed += OnStartPressed;
		GetNode<Button>("UI/ExitButton").Pressed += OnExitPressed;
		GetNode<Button>("UI/SettingsButton").Pressed += OnSettingsPressed;

		_settingsUi = new SettingsUiController(GetNode<Control>("SettingsPanel"));
	}

	public override void _ExitTree()
	{
		_settingsUi?.Dispose();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
			return;

		if (keyEvent.Keycode != Key.Escape)
			return;

		if (_settingsUi?.Close() == true)
			GetViewport().SetInputAsHandled();
	}

	private void OnStartPressed()
	{
		GetTree().ChangeSceneToFile(MainGameScene);
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}

	private void OnSettingsPressed()
	{
		_settingsUi?.Open();
	}
}
