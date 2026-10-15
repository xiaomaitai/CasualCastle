// HUD 组件：血条、金币、阶段面板与顶部按钮（hud.tscn 场景根）
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System;

public partial class HudUiController : Control
{
	public const float SpeedMin = 1f;
	public const float SpeedMax = 4f;
	public const float SpeedStep = 1f;
	public const float SpeedDefault = 1.0f;
	private const float DangerFlashFadeSeconds = 0.45f;
	private const float GoldDeltaOffsetY = -18f;
	private const int UnsetGold = -1;
	private static readonly Color GoldGainColor = new(0.45f, 0.95f, 0.5f, 1f);
	private static readonly Color GoldSpendColor = new(1f, 0.45f, 0.4f, 1f);

	private ProgressBar _playerHealthBar;
	private ProgressBar _enemyHealthBar;
	private Label _goldLabel;
	private Label _blackMagicLabel;
	private Label _bgmInfoLabel;
	private Label _phaseLabel;
	private Label _phaseTimerLabel;
	private Label _speedLabel;
	private Button _newDayButton;
	private Button _skipPhaseButton;
	private Button _cheatButton;
	private Button _infoButton;
	private Button _menuButton;
	private Button _buildingPauseToolButton;
	private Button _buildingRepairToolButton;
	private Button _combineProhibitToolButton;
	private HSlider _speedSlider;
	private Panel _speedPanel;
	private ColorRect _dangerFlash;
	private int _lastPlayerHealth = -1;
	private int _lastGold = UnsetGold;

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
	public Button BuildingPauseToolButton => _buildingPauseToolButton ??= GetNode<Button>("BuildingPauseToolButton");
	public Button BuildingRepairToolButton => _buildingRepairToolButton ??= GetNode<Button>("BuildingRepairToolButton");
	public Button CombineProhibitToolButton => _combineProhibitToolButton ??= GetNode<Button>("CombineProhibitToolButton");
	public Label SpeedLabel => _speedLabel ??= GetNode<Label>("SpeedPanel/SpeedLabel");
	public HSlider SpeedSlider => _speedSlider ??= GetNode<HSlider>("SpeedPanel/SpeedSlider");
	public Panel SpeedPanel => _speedPanel ??= GetNode<Panel>("SpeedPanel");

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
		_lastPlayerHealth = gm.PlayerHealth;
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

		SpeedPanel.Visible = true;
		SpeedSlider.MinValue = SpeedMin;
		SpeedSlider.MaxValue = SpeedMax;
		SpeedSlider.Step = SpeedStep;
		SpeedSlider.Value = settings.GameSpeed;
		SpeedSlider.ValueChanged += OnSpeedChanged;
		UpdateSpeedLabel();
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

		SpeedSlider.ValueChanged -= OnSpeedChanged;
		Engine.TimeScale = SpeedDefault;
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

	private void OnSpeedChanged(double value)
	{
		Engine.TimeScale = value;
		AdapterRegistry.Resolve<DisplaySettingsManager>().SetGameSpeed((float)value);
		UpdateSpeedLabel();
		Sfx.Play(SfxEvents.SliderChange);
	}

	private void UpdateSpeedLabel()
	{
		SpeedLabel.Text = $"{SpeedSlider.Value:0.0}×";
	}

	private void OnPhaseChanged(MatchPhase phase)
	{
		UpdatePhaseDisplay();
	}

	private void UpdatePlayerHealth(int health)
	{
		if (_lastPlayerHealth >= 0 && health < _lastPlayerHealth)
			PlayDangerFlash();
		_lastPlayerHealth = health;
		PlayerHealthBar.MaxValue = AdapterRegistry.Resolve<GameManager>().PlayerMaxHealth;
		PlayerHealthBar.SetValue(health);
	}

	private void PlayDangerFlash()
	{
		Tween tween = CreateTween();
		_dangerFlash ??= GetNode<ColorRect>("DangerFlash");
		_dangerFlash.Modulate = Colors.White;
		_dangerFlash.Visible = true;
		tween.TweenProperty(_dangerFlash, "modulate:a", 0f, DangerFlashFadeSeconds);
		tween.TweenCallback(Callable.From(() => _dangerFlash.Visible = false));
	}

	private void UpdateEnemyHealth(int health)
	{
		EnemyHealthBar.MaxValue = AdapterRegistry.Resolve<GameManager>().EnemyMaxHealth;
		EnemyHealthBar.SetValue(health);
	}

	private void UpdateGoldDisplay(int gold)
	{
		GoldLabel.Text = $"金币：{gold}";
		PlayGoldPop(GoldLabel);
		PlayGoldDelta(gold);
	}

	private void PlayGoldDelta(int gold)
	{
		int previous = _lastGold;
		_lastGold = gold;
		if (previous == UnsetGold || previous == gold)
			return;
		int delta = gold - previous;
		Color color = delta > 0 ? GoldGainColor : GoldSpendColor;
		string text = delta > 0 ? $"+{delta}" : $"-{-delta}";
		Vector2 anchor = GoldLabel.GlobalPosition - GlobalPosition + new Vector2(0f, GoldDeltaOffsetY);
		UiFloatingText.Spawn(this, anchor, text, color);
	}

	private void PlayGoldPop(Label label)
	{
		Tween tween = CreateTween();
		label.PivotOffset = label.Size / 2f;
		tween.TweenProperty(label, "scale", new Vector2(1.15f, 1.15f), 0.08f);
		tween.TweenProperty(label, "scale", Vector2.One, 0.14f);
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
		PhaseLabel.Text = gm.IsNightDisplayed ? "夜晚" : "白天";
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
