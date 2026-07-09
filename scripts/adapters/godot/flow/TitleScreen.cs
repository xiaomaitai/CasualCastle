using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;
using System;

public partial class TitleScreen : Control
{
	private const string MainGameScene = "res://scenes/main/main_game.tscn";
	private const int NoReportItemId = 0;

	private SettingsUiController _settingsUi;
	private Button _startButton;
	private Button _continueButton;
	private Button _settingsButton;
	private Button _exitButton;
	private OptionButton _reportOption;
	private bool _isStartingGame;

	public override void _Ready()
	{
		VBoxContainer ui = GetNode<VBoxContainer>("UI");
		_startButton = ui.GetNode<Button>("StartButton");
		_settingsButton = ui.GetNode<Button>("SettingsButton");
		_exitButton = ui.GetNode<Button>("ExitButton");
		_reportOption = ui.GetNode<OptionButton>("ReportOption");

		_continueButton = new Button();
		_continueButton.Name = "ContinueButton";
		_continueButton.Text = "继续游戏";
		ui.AddChild(_continueButton);
		ui.MoveChild(_continueButton, ui.GetChildCount() - 4);

		_startButton.Pressed += OnStartPressed;
		_continueButton.Pressed += OnContinuePressed;
		_exitButton.Pressed += OnExitPressed;
		_settingsButton.Pressed += OnSettingsPressed;

		_settingsUi = new SettingsUiController(GetNode<Control>("SettingsPanel"));
		BuildReportOptions();

		ISaveRepository saveRepo = GameManager.Get<ISaveRepository>();
		_continueButton.Disabled = !saveRepo.HasSave(0);
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
		_continueButton.Disabled = true;
		_settingsButton.Disabled = true;
		_exitButton.Disabled = true;
		_reportOption.Disabled = true;
		_startButton.Text = "加载中...";
		AdapterRegistry.Resolve<GameManager>().PendingLoadSlot = -1;
		ApplySelectedReportToGameManager();
		await ForceClearMainGameResidue();
		GetTree().ChangeSceneToFile(MainGameScene);
	}

	private async void OnContinuePressed()
	{
		if (_isStartingGame)
			return;

		_isStartingGame = true;
		_startButton.Disabled = true;
		_continueButton.Disabled = true;
		_settingsButton.Disabled = true;
		_exitButton.Disabled = true;
		_reportOption.Disabled = true;
		_continueButton.Text = "加载中...";
		AdapterRegistry.Resolve<GameManager>().PendingLoadSlot = 0;
		AdapterRegistry.Resolve<GameManager>().SetPendingReplayReportId("");
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

		List<BattleReport> reports = GameManager.Get<IBattleReportRepository>()?.LoadAll() ?? new();
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
		if (_reportOption == null || AdapterRegistry.Resolve<GameManager>() == null)
			return;

		int selectedIndex = _reportOption.Selected;
		if (selectedIndex <= 0)
		{
			AdapterRegistry.Resolve<GameManager>().SetPendingReplayReportId("");
			return;
		}

		Variant metadata = _reportOption.GetItemMetadata(selectedIndex);
		string reportId = metadata.VariantType == Variant.Type.Nil ? "" : metadata.AsString();
		AdapterRegistry.Resolve<GameManager>().SetPendingReplayReportId(reportId);
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
