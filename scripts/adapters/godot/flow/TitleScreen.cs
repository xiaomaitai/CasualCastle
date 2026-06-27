using CasualCastle.Domain.Ports;
using Godot;
using System.Collections.Generic;
using System;

public partial class TitleScreen : Control
{
	private const string MainGameScene = "res://scenes/main/main_game.tscn";
	private const int NoReportItemId = 0;

	private SettingsUiController _settingsUi;
	private Button _startButton;
	private Button _settingsButton;
	private Button _exitButton;
	private OptionButton _reportOption;
	private bool _isStartingGame;

	public override void _Ready()
	{
		_startButton = GetNode<Button>("UI/StartButton");
		_settingsButton = GetNode<Button>("UI/SettingsButton");
		_exitButton = GetNode<Button>("UI/ExitButton");
		_startButton.Pressed += OnStartPressed;
		_exitButton.Pressed += OnExitPressed;
		_settingsButton.Pressed += OnSettingsPressed;
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

	private async void OnStartPressed()
	{
		if (_isStartingGame)
			return;

		_isStartingGame = true;
		_startButton.Disabled = true;
		_settingsButton.Disabled = true;
		_exitButton.Disabled = true;
		_reportOption.Disabled = true;
		_startButton.Text = "加载中...";
		ApplySelectedReportToGameManager();
		await ForceClearMainGameResidue();
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

		List<BattleReport> reports = BattleReportStorage.Instance.LoadAll();
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

	private async System.Threading.Tasks.Task ForceClearMainGameResidue()
	{
		SceneTree tree = GetTree();
		Window root = tree.Root;

		foreach (Node child in root.GetChildren())
		{
			if (child.Name != "MainGame")
				continue;

			root.RemoveChild(child);
			child.QueueFree();
		}

		await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
		await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
		GC.Collect();
	}
}
