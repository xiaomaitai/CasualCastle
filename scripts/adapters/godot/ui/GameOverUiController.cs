// 结算弹窗组件（game_over.tscn 场景根）
using CasualCastle.Adapters.Godot;
using Godot;
using System;

public partial class GameOverUiController : Control
{
	private ColorRect _overlay;
	private Panel _panel;
	private Label _label;
	private Button _restartButton;
	private Button _saveReportButton;
	private Button _discardAndBackButton;

	public Action GoToTitleAction { get; set; }
	public Action RestartMatchAction { get; set; }

	public override void _Ready()
	{
		_overlay = GetNode<ColorRect>("GameOverOverlay");
		_panel = GetNode<Panel>("GameOverPanel");
		_label = GetNode<Label>("GameOverPanel/GameOverLabel");
		_restartButton = GetNode<Button>("GameOverPanel/RestartButton");
		_saveReportButton = GetNode<Button>("GameOverPanel/SaveReportButton");
		_discardAndBackButton = GetNode<Button>("GameOverPanel/DiscardAndBackButton");

		_restartButton.Pressed += OnRestartPressed;
		_saveReportButton.Pressed += OnSaveReportConfirmed;
		_discardAndBackButton.Pressed += OnDiscardReportConfirmed;
	}

	public override void _ExitTree()
	{
		_restartButton.Pressed -= OnRestartPressed;
		_saveReportButton.Pressed -= OnSaveReportConfirmed;
		_discardAndBackButton.Pressed -= OnDiscardReportConfirmed;
	}

	public void SetState(GameManager.GameState state)
	{
		bool show = state == GameManager.GameState.GameOver;
		_overlay.Visible = show;
		_panel.Visible = show;

		if (show)
		{
			_label.Text = AdapterRegistry.Resolve<GameManager>().PlayerHealth > 0 ? "胜利！" : "失败！";
			bool hasSnapshots = AdapterRegistry.Resolve<BattleReportSystem>()?.HasCurrentSnapshots == true;
			_saveReportButton.Visible = hasSnapshots;
			_discardAndBackButton.Text = hasSnapshots ? "不保存并返回标题" : "返回标题";
		}
	}

	private void OnRestartPressed()
	{
		RestartMatchAction?.Invoke();
	}

	private void OnSaveReportConfirmed()
	{
		AdapterRegistry.Resolve<BattleReportSystem>()?.SaveCurrentReport();
		GoToTitleAction?.Invoke();
	}

	private void OnDiscardReportConfirmed()
	{
		AdapterRegistry.Resolve<BattleReportSystem>()?.DiscardCurrentReport();
		GoToTitleAction?.Invoke();
	}
}
