// 开发者模式专属：左上角小字显示当前 BGM 名称与编号。
using CasualCastle.Domain.Shared;
using Godot;

namespace CasualCastle.Adapters.Godot.Dev;

public sealed class BgmInfoController
{
	private readonly Label _label;

	public BgmInfoController(Label label)
	{
		_label = label;
	}

	public void Process()
	{
		_label.Visible = DisplaySettingsManager.DevModeEnabled;
		if (!DisplaySettingsManager.DevModeEnabled)
			return;

		BgmDef current = AudioManager.Instance?.CurrentBgm;
		_label.Text = current == null ? "" : $"BGM {current.Id:D2} · {current.Name}";
	}
}