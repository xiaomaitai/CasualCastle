// 开发者工具菜单逻辑
using Godot;

namespace CasualCastle.Adapters.Godot.Dev;

public sealed class DevToolsMenu
{
	private const string TitleScene = "res://scenes/ui/title_screen.tscn";

	private readonly Control _root;
	private readonly Button _backButton;
	private readonly Button _damageMatrixButton;
	private readonly Button _parameterEditorButton;
	private readonly Button _techTreeButton;
	private readonly Button _buildingEditorButton;

	public DevToolsMenu(Control root)
	{
		_root = root;
		VBoxContainer list = root.GetNode<VBoxContainer>("Panel/ToolList");
		_backButton = root.GetNode<Button>("Panel/BackButton");
		_damageMatrixButton = list.GetNode<Button>("DamageMatrixButton");
		_parameterEditorButton = list.GetNode<Button>("ParameterEditorButton");
		_techTreeButton = list.GetNode<Button>("TechTreeButton");
		_buildingEditorButton = list.GetNode<Button>("BuildingEditorButton");

		_backButton.Pressed += OnBackPressed;
		_damageMatrixButton.Pressed += OnDamageMatrixPressed;
		_parameterEditorButton.Pressed += OnParameterEditorPressed;
		_techTreeButton.Pressed += OnTechTreePressed;
		_buildingEditorButton.Pressed += OnBuildingEditorPressed;
	}

	public void Dispose()
	{
		if (GodotObject.IsInstanceValid(_backButton))
			_backButton.Pressed -= OnBackPressed;
		if (GodotObject.IsInstanceValid(_damageMatrixButton))
			_damageMatrixButton.Pressed -= OnDamageMatrixPressed;
		if (GodotObject.IsInstanceValid(_parameterEditorButton))
			_parameterEditorButton.Pressed -= OnParameterEditorPressed;
		if (GodotObject.IsInstanceValid(_techTreeButton))
			_techTreeButton.Pressed -= OnTechTreePressed;
		if (GodotObject.IsInstanceValid(_buildingEditorButton))
			_buildingEditorButton.Pressed -= OnBuildingEditorPressed;
	}

	private void OnBackPressed()
	{
		_root.GetTree().ChangeSceneToFile(TitleScene);
	}

	private void OnDamageMatrixPressed()
	{
		_root.GetTree().ChangeSceneToFile("res://scenes/ui/dev_tools/damage_matrix_editor.tscn");
	}

	private void OnParameterEditorPressed()
	{
		_root.GetTree().ChangeSceneToFile("res://scenes/ui/dev_tools/parameter_editor.tscn");
	}

	private void OnTechTreePressed()
	{
		_root.GetTree().ChangeSceneToFile("res://scenes/dev/tech_tree_editor.tscn");
	}

	private void OnBuildingEditorPressed()
	{
		_root.GetTree().ChangeSceneToFile("res://scenes/ui/dev_tools/building_editor.tscn");
	}
}
