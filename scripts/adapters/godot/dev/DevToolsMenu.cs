using Godot;

namespace CasualCastle.Adapters.Godot.Dev;

public sealed class DevToolsMenu
{
	private const string TitleScene = "res://scenes/ui/title_screen.tscn";

	private readonly Control _root;
	private readonly Button _backButton;
	private readonly Button _damageMatrixButton;

	public DevToolsMenu(Control root)
	{
		_root = root;
		VBoxContainer list = root.GetNode<VBoxContainer>("Panel/ToolList");
		_backButton = root.GetNode<Button>("Panel/BackButton");
		_damageMatrixButton = list.GetNode<Button>("DamageMatrixButton");

		_backButton.Pressed += OnBackPressed;
		_damageMatrixButton.Pressed += OnDamageMatrixPressed;
	}

	public void Dispose()
	{
		_backButton.Pressed -= OnBackPressed;
		_damageMatrixButton.Pressed -= OnDamageMatrixPressed;
	}

	private void OnBackPressed()
	{
		_root.GetTree().ChangeSceneToFile(TitleScene);
	}

	private void OnDamageMatrixPressed()
	{
		_root.GetTree().ChangeSceneToFile("res://scenes/ui/dev_tools/damage_matrix_editor.tscn");
	}
}
