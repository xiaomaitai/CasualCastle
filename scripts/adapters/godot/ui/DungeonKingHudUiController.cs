// 魔王 HUD：等级、舒适度与技能数常驻面板
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;

public partial class DungeonKingHudUiController : Control
{
	private const string GraceSkillId = "slaanesh_grace";

	private Label _label;
	private ProgressBar _comfortBar;
	private Button _graceButton;
	private ComfortService _comfort;
	private KingExperienceService _xp;
	private KingSkillSet _skillSet;
	private DemonKingController _demonKing;
	private int _skillCount;
	private int _lastCooldownSeconds = -1;

	public override void _Ready()
	{
		_label = GetNode<Label>("KingHudPanel/Label");
		_comfortBar = GetNode<ProgressBar>("KingHudPanel/ComfortBar");
		_graceButton = GetNode<Button>("GraceButton");
		_comfort = GameManager.Get<ComfortService>();
		_xp = GameManager.Get<KingExperienceService>();
		_skillSet = GameManager.Get<KingSkillSet>();
		if (_comfort != null)
			_comfort.ValueChanged += OnComfortChanged;
		if (_xp != null)
			_xp.LevelUp += OnLevelUp;
		AdapterRegistry.Register<DungeonKingHudUiController>(this);
		Visible = AdapterRegistry.Resolve<GameManager>().SelectedFaction == FactionDef.DungeonId;
		_demonKing = AdapterRegistry.Resolve<DemonKingController>();
		if (_demonKing != null)
			_demonKing.GraceStateChanged += RefreshGraceButton;
		_graceButton.Pressed += OnGracePressed;
		Refresh();
		RefreshGraceButton();
	}

	public override void _ExitTree()
	{
		if (_comfort != null)
			_comfort.ValueChanged -= OnComfortChanged;
		if (_xp != null)
			_xp.LevelUp -= OnLevelUp;
		if (_demonKing != null)
			_demonKing.GraceStateChanged -= RefreshGraceButton;
		AdapterRegistry.Unregister<DungeonKingHudUiController>(this);
	}

	public override void _Process(double _)
	{
		UpdateGraceCooldown();
	}

	public void SetSkillCount(int count)
	{
		_skillCount = count;
		Refresh();
	}

	private void OnComfortChanged(float _)
	{
		Refresh();
	}

	private void OnLevelUp(int _)
	{
		Refresh();
	}

	private void OnGracePressed()
	{
		_demonKing?.TryActivateGrace();
	}

	private void RefreshGraceButton()
	{
		if (_graceButton == null)
			return;
		bool learned = _skillSet != null && _skillSet.Contains(GraceSkillId);
		_graceButton.Visible = learned;
		if (!learned)
			return;
		_graceButton.Disabled = _demonKing == null || !_demonKing.CanActivateGrace;
		_lastCooldownSeconds = -1;
	}

	private void UpdateGraceCooldown()
	{
		if (!_graceButton.Visible || _demonKing == null)
			return;
		int seconds = Mathf.CeilToInt(_demonKing.GraceCooldownRemaining);
		if (seconds == _lastCooldownSeconds)
			return;
		_lastCooldownSeconds = seconds;
		_graceButton.Text = seconds > 0 ? $"色孽恩赐（{seconds}s）" : "色孽恩赐";
		_graceButton.Disabled = !_demonKing.CanActivateGrace;
	}

	private void Refresh()
	{
		if (_comfortBar != null)
		{
			_comfortBar.MaxValue = _comfort?.MaxComfort ?? DemonKingRules.MaxComfort;
			_comfortBar.Value = _comfort?.Value ?? 0f;
		}

		int level = _xp?.Level ?? 1;
		float comfort = _comfort?.Value ?? 0f;
		_label.Text = $"魔王 Lv.{level} · 技能 {_skillCount} · 舒适 {comfort:F0}";
	}
}
