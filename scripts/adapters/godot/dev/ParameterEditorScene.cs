// 参数编辑器场景控制器
using CasualCastle.Adapters.Godot.Dev;
using Godot;

public partial class ParameterEditorScene : Control
{
	private ParameterEditor _editor;

	public override void _Ready()
	{
		_editor = new ParameterEditor(this);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			_editor?.Dispose();
		base.Dispose(disposing);
	}
}
