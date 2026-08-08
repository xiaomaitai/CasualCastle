// 伤害矩阵编辑器场景控制器
using CasualCastle.Adapters.Godot.Dev;
using Godot;

public partial class DamageMatrixEditorScene : Control
{
	private DamageMatrixEditor _editor;

	public override void _Ready()
	{
		_editor = new DamageMatrixEditor(this);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			_editor?.Dispose();
		base.Dispose(disposing);
	}
}
