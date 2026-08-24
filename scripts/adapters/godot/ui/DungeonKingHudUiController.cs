// 魔王 HUD：等级、舒适度与技能数常驻面板
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;

public partial class DungeonKingHudUiController : Control
{
	private Label _label;
	private ProgressBar _comfortBar;
	private ComfortService _comfort;
	private KingExperienceService _xp;
	private int _skillCount;

	public override void _Ready()
	{
		_label = GetNode<Label>("KingHudPanel/Label");
		_comfortBar = GetNode<ProgressBar>("KingHudPanel/ComfortBar");
		_comfort = GameManager.Get<ComfortService>();
		_xp = GameManager.Get<KingExperienceService>();
		if (_comfort != null)
			_comfort.ValueChanged += OnComfortChanged;
		if (_xp != null)
			_xp.LevelUp += OnLevelUp;
		AdapterRegistry.Register<DungeonKingHudUiController>(this);
		Visible = AdapterRegistry.Resolve<GameManager>().SelectedFaction == FactionDef.DungeonId;
		Refresh();
	}

	public override void _ExitTree()
	{
		if (_comfort != null)
			_comfort.ValueChanged -= OnComfortChanged;
		if (_xp != null)
			_xp.LevelUp -= OnLevelUp;
		AdapterRegistry.Unregister<DungeonKingHudUiController>(this);
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
