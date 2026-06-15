using Godot;

public partial class TitleScreen : Control
{
	private const string MainGameScene = "res://scenes/main/main_game.tscn";

	public override void _Ready()
	{
		GetNode<Button>("UI/StartButton").Pressed += OnStartPressed;
		GetNode<Button>("UI/ExitButton").Pressed += OnExitPressed;
	}

	private void OnStartPressed()
	{
		GetTree().ChangeSceneToFile(MainGameScene);
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}
}
