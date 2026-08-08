// HUD UI：血条、金币、阶段与跳过按钮
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System;

public sealed class HudUiController
{
	private readonly ProgressBar _playerHealthBar;
	private readonly ProgressBar _enemyHealthBar;
	private readonly Label _phaseLabel;
	private readonly Label _phaseTimerLabel;
	private readonly Button _newDayButton;
	private readonly Button _skipPhaseButton;
	private readonly Label _goldLabel;
	private readonly Label _blackMagicLabel;

	public HudUiController(CanvasLayer uiRoot)
	{
		_playerHealthBar = uiRoot.GetNode<ProgressBar>("PlayerHealthBar");
		_enemyHealthBar = uiRoot.GetNode<ProgressBar>("EnemyHealthBar");
		_phaseLabel = uiRoot.GetNode<Label>("PhasePanel/PhaseLabel");
		_phaseTimerLabel = uiRoot.GetNode<Label>("PhasePanel/PhaseTimerLabel");
		_newDayButton = uiRoot.GetNode<Button>("PhasePanel/NewDayButton");
		_skipPhaseButton = uiRoot.GetNode<Button>("PhasePanel/SkipPhaseButton");
		_goldLabel = uiRoot.GetNode<Label>("GoldLabel");
		_blackMagicLabel = uiRoot.GetNode<Label>("BlackMagicLabel");

		_newDayButton.Pressed += OnNewDayPressed;
		_newDayButton.Text = AdapterRegistry.Resolve<GameManager>().IsUndeadBattle ? "进入夜晚" : "新的一天";
		_skipPhaseButton.Pressed += OnSkipPhasePressed;
		_newDayButton.Visible = false;
		_skipPhaseButton.Visible = false;

		AdapterRegistry.Resolve<GameManager>().PlayerHealthChanged += UpdatePlayerHealth;
		AdapterRegistry.Resolve<GameManager>().EnemyHealthChanged += UpdateEnemyHealth;
		AdapterRegistry.Resolve<GameManager>().PhaseChanged += OnPhaseChanged;

		UpdatePlayerHealth(AdapterRegistry.Resolve<GameManager>().PlayerHealth);
		UpdateEnemyHealth(AdapterRegistry.Resolve<GameManager>().EnemyHealth);
		UpdatePhaseDisplay();

		if (AdapterRegistry.Resolve<Shop>() != null)
		{
			AdapterRegistry.Resolve<Shop>().GoldChanged += UpdateGoldDisplay;
			UpdateGoldDisplay(AdapterRegistry.Resolve<Shop>().Gold);
		}
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		if (blackMagic != null)
		{
			blackMagic.ValueChanged += UpdateBlackMagicDisplay;
			UpdateBlackMagicDisplay(blackMagic.Value);
		}
	}

	public void Dispose()
	{
		_newDayButton.Pressed -= OnNewDayPressed;
		_skipPhaseButton.Pressed -= OnSkipPhasePressed;

		AdapterRegistry.Resolve<GameManager>().PlayerHealthChanged -= UpdatePlayerHealth;
		AdapterRegistry.Resolve<GameManager>().EnemyHealthChanged -= UpdateEnemyHealth;
		AdapterRegistry.Resolve<GameManager>().PhaseChanged -= OnPhaseChanged;

		if (AdapterRegistry.Resolve<Shop>() != null)
			AdapterRegistry.Resolve<Shop>().GoldChanged -= UpdateGoldDisplay;
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		if (blackMagic != null)
			blackMagic.ValueChanged -= UpdateBlackMagicDisplay;
	}

	public void Process()
	{
		UpdatePhaseDisplay();
	}

	public void SetGameOverVisible(bool visible)
	{
		_newDayButton.Visible = false;
		_skipPhaseButton.Visible = false;
	}

	private void OnNewDayPressed()
	{
		AdapterRegistry.Resolve<GameManager>().AdvancePhase();
	}

	private void OnSkipPhasePressed()
	{
		AdapterRegistry.Resolve<GameManager>().AdvancePhase();
	}

	private void OnPhaseChanged(MatchPhase phase)
	{
		UpdatePhaseDisplay();
	}

	private void UpdatePlayerHealth(int health)
	{
		_playerHealthBar.MaxValue = AdapterRegistry.Resolve<GameManager>().PlayerMaxHealth;
		_playerHealthBar.SetValue(health);
	}

	private void UpdateEnemyHealth(int health)
	{
		_enemyHealthBar.MaxValue = AdapterRegistry.Resolve<GameManager>().EnemyMaxHealth;
		_enemyHealthBar.SetValue(health);
	}

	private void UpdateGoldDisplay(int gold)
	{
		_goldLabel.Text = $"金币：{gold}";
	}

	private void UpdateBlackMagicDisplay(int value)
	{
		Shop shop = AdapterRegistry.Resolve<Shop>();
		bool isNecromancer = shop != null && shop.FactionId == FactionDef.NecromancerId;
		_blackMagicLabel.Visible = isNecromancer;
		_blackMagicLabel.Text = $"黑魔法：{value}";
	}

	private void UpdatePhaseDisplay()
	{
		GameManager gm = AdapterRegistry.Resolve<GameManager>();
		_phaseLabel.Text = gm.IsCombine ? "合成" : gm.IsNightDisplayed ? "夜晚" : "白天";
		if (gm.IsCombat)
		{
			_phaseTimerLabel.Text = FormatTime(gm.PhaseTimeRemaining);
			_newDayButton.Visible = false;
			_skipPhaseButton.Visible = DisplaySettingsManager.DevModeEnabled;
		}
		else
		{
			_phaseTimerLabel.Text = "";
			_newDayButton.Visible = gm.IsShop;
			_skipPhaseButton.Visible = false;
		}
	}

	private static string FormatTime(float seconds)
	{
		int total = Math.Max(0, (int)Math.Ceiling(seconds));
		return $"{total / 60}:{total % 60:D2}";
	}
}
