using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.History;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;
using System;
using System.Linq;

public partial class TitleScreen : Control
{
	private const string MainGameScene = "res://scenes/main/main_game.tscn";
	private const int RandomReportItemId = 0;
	private const int NoReportItemId = 1;

	private SettingsUiController _settingsUi;
	private Button _startButton;
	private Button _continueButton;
	private Button _settingsButton;
	private Button _exitButton;
	private Button _devToolsButton;
	private OptionButton _reportOption;
	private OptionButton _factionOption;
	private bool _isStartingGame;

	public override void _Ready()
	{
		VBoxContainer ui = GetNode<VBoxContainer>("UI");
		_startButton = ui.GetNode<Button>("StartButton");
		_settingsButton = ui.GetNode<Button>("SettingsButton");
		_exitButton = ui.GetNode<Button>("ExitButton");
		_reportOption = ui.GetNode<OptionButton>("ReportOption");
		_factionOption = ui.GetNode<OptionButton>("FactionOption");

		_continueButton = new Button();
		_continueButton.Name = "ContinueButton";
		_continueButton.Text = "继续游戏";
		ui.AddChild(_continueButton);
		ui.MoveChild(_continueButton, ui.GetChildCount() - 4);

		_devToolsButton = new Button();
		_devToolsButton.Name = "DevToolsButton";
		_devToolsButton.Text = "开发者工具";
		_devToolsButton.Visible = DisplaySettingsManager.DevModeEnabled;
		ui.AddChild(_devToolsButton);
		ui.MoveChild(_devToolsButton, ui.GetChildCount() - 2);

		_startButton.Pressed += OnStartPressed;
		_continueButton.Pressed += OnContinuePressed;
		_exitButton.Pressed += OnExitPressed;
		_settingsButton.Pressed += OnSettingsPressed;
		_devToolsButton.Pressed += OnDevToolsPressed;

		_settingsUi = new SettingsUiController(GetNode<Control>("SettingsPanel"));
		BuildReportOptions();

		ISaveRepository saveRepo = GameManager.Get<ISaveRepository>();
		_continueButton.Disabled = !saveRepo.HasSave(0);
	UiSfx.Bind(this);
	AudioManager.Instance?.PlayTitleMusic();
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
		_factionOption.Disabled = true;
		Sfx.Play(SfxEvents.UiStart);
		_startButton.Text = "加载中...";
		AdapterRegistry.Resolve<GameManager>().PendingLoadSlot = -1;
		ApplySelectedFaction();
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
		_factionOption.Disabled = true;
		_continueButton.Text = "加载中...";
		AdapterRegistry.Resolve<GameManager>().PendingLoadSlot = 0;
		AdapterRegistry.Resolve<GameManager>().SetPendingReplayReportId("");
		SaveData save = AdapterRegistry.Resolve<GameManager>().LoadSaveData(0);
		if (save != null)
			AdapterRegistry.Resolve<GameManager>().SetSelectedFaction(save.FactionId);
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

	private void OnDevToolsPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/ui/dev_tools/dev_tools_menu.tscn");
	}

	private void BuildReportOptions()
	{
		_reportOption.Clear();
		_reportOption.AddItem("随机使用战报", RandomReportItemId);
		_reportOption.Selected = 0;
		_reportOption.AddItem("不使用战报", NoReportItemId);

		List<BattleReport> reports = GameManager.Get<IBattleReportRepository>()?.LoadAll() ?? new();
		int firstReportItemId = _reportOption.ItemCount;
		for (int i = 0; i < reports.Count; i++)
		{
			BattleReport report = reports[i];
			int nightCount = report.Nights.Select(s => s.NightIndex).Distinct().Count();
			string item = $"{report.DisplayName}（{nightCount} 夜）";
			_reportOption.AddItem(item, firstReportItemId + i);
			_reportOption.SetItemMetadata(firstReportItemId + i, report.ReportId);
		}
	}

	private void ApplySelectedReportToGameManager()
	{
		if (_reportOption == null || AdapterRegistry.Resolve<GameManager>() == null)
			return;

		int selectedIndex = _reportOption.Selected;
		int selectedItemId = _reportOption.GetItemId(selectedIndex);
		if (selectedItemId == RandomReportItemId)
		{
			ApplyRandomReport();
			return;
		}

		if (selectedItemId == NoReportItemId)
		{
			AdapterRegistry.Resolve<GameManager>().SetPendingReplayReportId("");
			return;
		}

		Variant metadata = _reportOption.GetItemMetadata(selectedIndex);
		string reportId = metadata.VariantType == Variant.Type.Nil ? "" : metadata.AsString();
		AdapterRegistry.Resolve<GameManager>().SetPendingReplayReportId(reportId);
	}

	private void ApplySelectedFaction()
	{
		string factionId = _factionOption.GetItemId(_factionOption.Selected) == 1
			? FactionDef.HordeId
			: "alliance";
		AdapterRegistry.Resolve<Shop>()?.SetFaction(factionId);
		AdapterRegistry.Resolve<GameManager>()?.SetSelectedFaction(factionId);
	}

	private void ApplyRandomReport()
	{
		List<BattleReport> reports = GameManager.Get<IBattleReportRepository>()?.LoadAll() ?? new();
		if (reports.Count == 0)
		{
			AdapterRegistry.Resolve<GameManager>().SetPendingReplayReportId("");
			return;
		}

		int randomIndex = Random.Shared.Next(reports.Count);
		AdapterRegistry.Resolve<GameManager>().SetPendingReplayReportId(reports[randomIndex].ReportId);
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
