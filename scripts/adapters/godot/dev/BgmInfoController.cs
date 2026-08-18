// 开发者模式专属：左上角小字显示当前 BGM 名称与编号。
using CasualCastle.Domain.Shared;
using Godot;

namespace CasualCastle.Adapters.Godot.Dev;

public partial class BgmInfoController : Node
{
	public Label Label { get; set; }

	public override void _Process(double _)
	{
		Label label = Label;
		label.Visible = DisplaySettingsManager.DevModeEnabled;
		if (!DisplaySettingsManager.DevModeEnabled)
			return;

		BgmDef current = AudioManager.Instance?.CurrentBgm;
		label.Text = current == null ? "" : $"BGM {current.Id:D2} · {current.Name}";
	}
}