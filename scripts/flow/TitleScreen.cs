using Godot;
using System.Collections.Generic;

public partial class TitleScreen : Control
{
	private const string MainGameScene = "res://scenes/main/main_game.tscn";
	private const int NoReportItemId = 0;

	private SettingsUiController _settingsUi;
	private OptionButton _reportOption;

	public override void _Ready()
	{
		GetNode<Button>("UI/StartButton").Pressed += OnStartPressed;
		GetNode<Button>("UI/ExitButton").Pressed += OnExitPressed;
		GetNode<Button>("UI/SettingsButton").Pressed += OnSettingsPressed;
		_reportOption = GetNode<OptionButton>("UI/ReportOption");

		_settingsUi = new SettingsUiController(GetNode<Control>("SettingsPanel"));
		BuildReportOptions();
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
		ApplySelectedReportToGameManager();
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

	private void BuildReportOptions()
	{
		_reportOption.Clear();
		_reportOption.AddItem("不使用战报", NoReportItemId);
		_reportOption.SetItemMetadata(0, "");
		_reportOption.Selected = 0;

		List<BattleReport> reports = BattleReportStorage.LoadAll();
		for (int i = 0; i < reports.Count; i++)
		{
			BattleReport report = reports[i];
			string item = $"{report.DisplayName}（{report.Nights.Count} 夜）";
			_reportOption.AddItem(item, i + 1);
			_reportOption.SetItemMetadata(i + 1, report.ReportId);
		}
	}

	private void ApplySelectedReportToGameManager()
	{
		if (_reportOption == null || GameManager.Instance == null)
			return;

		int selectedIndex = _reportOption.Selected;
		if (selectedIndex <= 0)
		{
			GameManager.Instance.SetPendingReplayReportId("");
			return;
		}

		Variant metadata = _reportOption.GetItemMetadata(selectedIndex);
		string reportId = metadata.VariantType == Variant.Type.Nil ? "" : metadata.AsString();
		GameManager.Instance.SetPendingReplayReportId(reportId);
	}
}
