// HUD 组件：血条、金币、阶段面板与顶部按钮（hud.tscn 场景根）
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System;

public partial class HudUiController : Control
{
	private ProgressBar _playerHealthBar;
	private ProgressBar _enemyHealthBar;
	private Label _goldLabel;
	private Label _blackMagicLabel;
	private Label _bgmInfoLabel;
	private Label _phaseLabel;
	private Label _phaseTimerLabel;
	private Button _newDayButton;
	private Button _skipPhaseButton;
	private Button _cheatButton;
	private Button _infoButton;
	private Button _menuButton;
	private Panel _shopButtonCircle;
	private Button _shopButton;
	private Button _buildingPauseToolButton;
	private Button _buildingRepairToolButton;
	private Button _combineProhibitToolButton;

	public ProgressBar PlayerHealthBar => _playerHealthBar ??= GetNode<ProgressBar>("PlayerHealthBar");
	public ProgressBar EnemyHealthBar => _enemyHealthBar ??= GetNode<ProgressBar>("EnemyHealthBar");
	public Label GoldLabel => _goldLabel ??= GetNode<Label>("GoldLabel");
	public Label BlackMagicLabel => _blackMagicLabel ??= GetNode<Label>("BlackMagicLabel");
	public Label BgmInfoLabel => _bgmInfoLabel ??= GetNode<Label>("BgmInfoLabel");
	public Label PhaseLabel => _phaseLabel ??= GetNode<Label>("PhasePanel/PhaseLabel");
	public Label PhaseTimerLabel => _phaseTimerLabel ??= GetNode<Label>("PhasePanel/PhaseTimerLabel");
	public Button NewDayButton => _newDayButton ??= GetNode<Button>("PhasePanel/NewDayButton");
	public Button SkipPhaseButton => _skipPhaseButton ??= GetNode<Button>("PhasePanel/SkipPhaseButton");
	public Button CheatButton => _cheatButton ??= GetNode<Button>("PhasePanel/CheatButton");
	public Button InfoButton => _infoButton ??= GetNode<Button>("PhasePanel/InfoButton");
	public Button MenuButton => _menuButton ??= GetNode<Button>("MenuButton");
	public Panel ShopButtonCircle => _shopButtonCircle ??= GetNode<Panel>("ShopButtonCircle");
	public Button ShopButton => _shopButton ??= GetNode<Button>("ShopButton");
	public Button BuildingPauseToolButton => _buildingPauseToolButton ??= GetNode<Button>("BuildingPauseToolButton");
	public Button BuildingRepairToolButton => _buildingRepairToolButton ??= GetNode<Button>("BuildingRepairToolButton");
	public Button CombineProhibitToolButton => _combineProhibitToolButton ??= GetNode<Button>("CombineProhibitToolButton");

	public override void _Ready()
	{
		DisplaySettingsManager settings = AdapterRegistry.Resolve<DisplaySettingsManager>();
		ApplyProfile(settings.UiProfile);
		settings.UiProfileChanged += ApplyProfile;

		NewDayButton.Pressed += OnNewDayPressed;
		NewDayButton.Text = AdapterRegistry.Resolve<GameManager>().IsUndeadBattle ? "进入夜晚" : "新的一天";
		SkipPhaseButton.Pressed += OnSkipPhasePressed;
		NewDayButton.Visible = false;
		SkipPhaseButton.Visible = false;

		GameManager gm = AdapterRegistry.Resolve<GameManager>();
		gm.PlayerHealthChanged += UpdatePlayerHealth;
		gm.EnemyHealthChanged += UpdateEnemyHealth;
		gm.PhaseChanged += OnPhaseChanged;

		UpdatePlayerHealth(gm.PlayerHealth);
		UpdateEnemyHealth(gm.EnemyHealth);
		UpdatePhaseDisplay();

		Shop shop = AdapterRegistry.Resolve<Shop>();
		if (shop != null)
		{
			shop.GoldChanged += UpdateGoldDisplay;
			UpdateGoldDisplay(shop.Gold);
		}
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		if (blackMagic != null)
		{
			blackMagic.ValueChanged += UpdateBlackMagicDisplay;
			UpdateBlackMagicDisplay(blackMagic.Value);
		}
	}

	public override void _ExitTree()
	{
		AdapterRegistry.Resolve<DisplaySettingsManager>().UiProfileChanged -= ApplyProfile;
		NewDayButton.Pressed -= OnNewDayPressed;
		SkipPhaseButton.Pressed -= OnSkipPhasePressed;

		GameManager gm = AdapterRegistry.Resolve<GameManager>();
		gm.PlayerHealthChanged -= UpdatePlayerHealth;
		gm.EnemyHealthChanged -= UpdateEnemyHealth;
		gm.PhaseChanged -= OnPhaseChanged;

		if (AdapterRegistry.Resolve<Shop>() != null)
			AdapterRegistry.Resolve<Shop>().GoldChanged -= UpdateGoldDisplay;
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		if (blackMagic != null)
			blackMagic.ValueChanged -= UpdateBlackMagicDisplay;
	}

	public override void _Process(double _)
	{
		UpdatePhaseDisplay();
	}

	public void SetGameOverVisible(bool visible)
	{
		NewDayButton.Visible = false;
		SkipPhaseButton.Visible = false;
	}

	private void ApplyProfile(UiProfileKind kind)
	{
		UiProfileStyles.ApplyHud(this, kind);
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
		PlayerHealthBar.MaxValue = AdapterRegistry.Resolve<GameManager>().PlayerMaxHealth;
		PlayerHealthBar.SetValue(health);
	}

	private void UpdateEnemyHealth(int health)
	{
		EnemyHealthBar.MaxValue = AdapterRegistry.Resolve<GameManager>().EnemyMaxHealth;
		EnemyHealthBar.SetValue(health);
	}

	private void UpdateGoldDisplay(int gold)
	{
		GoldLabel.Text = $"金币：{gold}";
	}

	private void UpdateBlackMagicDisplay(int value)
	{
		Shop shop = AdapterRegistry.Resolve<Shop>();
		bool isNecromancer = shop != null && shop.FactionId == FactionDef.NecromancerId;
		BlackMagicLabel.Visible = isNecromancer;
		BlackMagicLabel.Text = $"黑魔法：{value}";
	}

	private void UpdatePhaseDisplay()
	{
		GameManager gm = AdapterRegistry.Resolve<GameManager>();
		PhaseLabel.Text = gm.IsCombine ? "合成" : gm.IsNightDisplayed ? "夜晚" : "白天";
		if (gm.IsCombat)
		{
			PhaseTimerLabel.Text = FormatTime(gm.PhaseTimeRemaining);
			NewDayButton.Visible = false;
			SkipPhaseButton.Visible = DisplaySettingsManager.DevModeEnabled;
		}
		else
		{
			PhaseTimerLabel.Text = "";
			NewDayButton.Visible = gm.IsShop;
			SkipPhaseButton.Visible = false;
		}
	}

	private static string FormatTime(float seconds)
	{
		int total = Math.Max(0, (int)Math.Ceiling(seconds));
		return $"{total / 60}:{total % 60:D2}";
	}
}
