// 魔王技能三选一弹窗：升级时选择一项技能
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;
using System;
using System.Collections.Generic;

public partial class DungeonSkillPickUiController : Control
{
	private const float ButtonHeight = 96f;
	private const int ButtonFontSize = 24;

	private Label _titleLabel;
	private VBoxContainer _optionList;
	private readonly List<Button> _buttons = new();
	private Action<string> _onChosen;

	public override void _Ready()
	{
		_titleLabel = GetNode<Label>("PickPanel/TitleLabel");
		_optionList = GetNode<VBoxContainer>("PickPanel/OptionList");
		AdapterRegistry.Register<DungeonSkillPickUiController>(this);
	}

	public override void _ExitTree()
	{
		AdapterRegistry.Unregister<DungeonSkillPickUiController>(this);
	}

	public void ShowChoices(int level, List<string> skillIds, Action<string> onChosen)
	{
		_onChosen = onChosen;
		_titleLabel.Text = $"魔王升级至 Lv{level}，选择一项技能";
		ClearOptions();
		ISkillRepository skillRepo = GameManager.Get<ISkillRepository>();
		foreach (string skillId in skillIds)
		{
			SkillDef def = skillRepo?.Get(skillId);
			if (def == null)
				continue;
			Button button = new Button();
			string godName = GetGodName(DemonKingSkillPool.GetGod(skillId));
			string mark = DemonKingSkillPool.IsUltimate(skillId) ? "【终极】" : "";
			button.Text = $"{mark}{def.DisplayName}（{godName}）\n{def.Description}";
			button.CustomMinimumSize = new Vector2(0f, ButtonHeight);
			button.AddThemeFontSizeOverride("font_size", ButtonFontSize);
			string captured = skillId;
			button.Pressed += () => Choose(captured);
			_optionList.AddChild(button);
			_buttons.Add(button);
		}

		Visible = true;
	}

	public void HidePanel()
	{
		Visible = false;
		_onChosen = null;
		ClearOptions();
	}

	private void Choose(string skillId)
	{
		Visible = false;
		Action<string> handler = _onChosen;
		_onChosen = null;
		ClearOptions();
		handler?.Invoke(skillId);
	}

	private void ClearOptions()
	{
		foreach (Button button in _buttons)
			button.QueueFree();
		_buttons.Clear();
	}

	private static string GetGodName(string godId)
	{
		switch (godId)
		{
			case DemonKingSkillPool.Khorne: return "恐虐";
			case DemonKingSkillPool.Tzeentch: return "奸奇";
			case DemonKingSkillPool.Slaanesh: return "色孽";
			case DemonKingSkillPool.Nurgle: return "纳垢";
			default: return "未知";
		}
	}
}
