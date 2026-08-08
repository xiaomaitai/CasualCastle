// 开发者工具菜单场景控制器
using CasualCastle.Adapters.Godot.Dev;
using Godot;

public partial class DevToolsMenuScene : Control
{
	private DevToolsMenu _menu;

	public override void _Ready()
	{
		_menu = new DevToolsMenu(this);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			_menu?.Dispose();
		base.Dispose(disposing);
	}
}
