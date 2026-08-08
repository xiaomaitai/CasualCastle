// 法术栏 UI：选中施法/仪式建筑时替换手牌区，展示并触发可用仪式
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;
using System.Collections.Generic;

public sealed class SpellBarUiController
{
	private const float SlotWidth = 190f;
	private const float SlotHeight = 118f;

	private readonly ScrollContainer _scroll;
	private readonly HBoxContainer _slotRow;
	private readonly Control _handPanel;
	private readonly UnitSelectionController _selection;
	private readonly List<Button> _slotButtons = new();

	private Building _currentBuilding;
	private bool _inputBlocked;
	private bool _ritualInProgress;

	public SpellBarUiController(CanvasLayer uiRoot, UnitSelectionController selection)
	{
		_selection = selection;
		_scroll = uiRoot.GetNode<ScrollContainer>("SpellBarPanel");
		_slotRow = uiRoot.GetNode<HBoxContainer>("SpellBarPanel/SpellSlots");
		_handPanel = uiRoot.GetNode<Control>("HandPanel");
		_scroll.Visible = false;
		_handPanel.Visible = true;
	}

	public void Process()
	{
		Building building = _selection.SelectedBuilding;
		if (building != _currentBuilding)
		{
			_currentBuilding = building;
			RebuildSlots(building);
		}
		else if (building != null && GodotObject.IsInstanceValid(building) && building.IsRitualInProgress != _ritualInProgress)
		{
			RebuildSlots(building);
		}
		else if (_ritualInProgress && building != null && GodotObject.IsInstanceValid(building))
		{
			UpdateProgressText(building);
		}
		else if (!_ritualInProgress && building != null && GodotObject.IsInstanceValid(building))
		{
			RefreshAvailability(building);
		}

		RefreshVisibility();
	}

	public void SetInputBlocked(bool blocked)
	{
		_inputBlocked = blocked;
		if (_inputBlocked)
		{
			_currentBuilding = null;
			ClearSlots();
			RefreshVisibility();
		}
	}

	private void RebuildSlots(Building building)
	{
		ClearSlots();
		if (building == null || !GodotObject.IsInstanceValid(building) || !CanShow(building))
			return;

		if (building.IsRitualInProgress)
		{
			_ritualInProgress = true;
			AddProgressSlot(building);
			return;
		}

		IReadOnlyList<RitualDef> defs = RitualRules.GetAll();
		for (int i = 0; i < defs.Count; i++)
		{
			RitualDef def = defs[i];
			Button button = CreateSlotButton(building, def);
			_slotRow.AddChild(button);
			_slotButtons.Add(button);
		}
	}

	private Button CreateSlotButton(Building building, RitualDef def)
	{
		Button button = new()
		{
			Text = def.DisplayName + "\n" + GetCostText(def),
			Disabled = !IsAvailable(building, def),
			CustomMinimumSize = new Vector2(SlotWidth, SlotHeight),
		};
		button.AddThemeFontSizeOverride("font_size", 16);
		string ritualId = def.Id;
		button.Pressed += () => TryStartRitual(building, ritualId);
		return button;
	}

	private void AddProgressSlot(Building building)
	{
		Button button = new()
		{
			Disabled = true,
			CustomMinimumSize = new Vector2(SlotWidth * 1.4f, SlotHeight),
		};
		button.AddThemeFontSizeOverride("font_size", 17);
		_slotRow.AddChild(button);
		_slotButtons.Add(button);
		UpdateProgressText(building);
	}

	private void UpdateProgressText(Building building)
	{
		if (_slotButtons.Count != 1)
			return;
		RitualDef def = RitualRules.Get(building.ActiveRitualId);
		string name = def?.DisplayName ?? building.ActiveRitualId;
		_slotButtons[0].Text = "仪式中：" + name + "\n"
			+ Mathf.CeilToInt(building.RitualRemainingSeconds) + "s / " + Mathf.CeilToInt(building.RitualTotalSeconds) + "s";
	}

	private void RefreshAvailability(Building building)
	{
		IReadOnlyList<RitualDef> defs = RitualRules.GetAll();
		if (_slotButtons.Count != defs.Count)
			return;
		for (int i = 0; i < _slotButtons.Count; i++)
			_slotButtons[i].Disabled = !IsAvailable(building, defs[i]);
	}

	private void ClearSlots()
	{
		foreach (Button button in _slotButtons)
		{
			_slotRow.RemoveChild(button);
			button.QueueFree();
		}
		_slotButtons.Clear();
		_ritualInProgress = false;
	}

	private void TryStartRitual(Building building, string ritualId)
	{
		if (_inputBlocked || building == null || !GodotObject.IsInstanceValid(building))
			return;
		if (!building.TryStartRitual(ritualId))
			return;
		Sfx.Play(SfxEvents.Upgrade);
	}

	private void RefreshVisibility()
	{
		bool show = !_inputBlocked
			&& _currentBuilding != null
			&& GodotObject.IsInstanceValid(_currentBuilding)
			&& CanShow(_currentBuilding);
		_scroll.Visible = show;
		_handPanel.Visible = !show;
	}

	private static bool CanShow(Building building)
	{
		if (building == null || building.IsDestroyed || !building.IsPlayerOwned)
			return false;
		return BuildingActionRules.CanUseSpellBar(building.TypeId);
	}

	private static bool IsAvailable(Building building, RitualDef def)
	{
		if (building.GetCastle() == null)
			return false;
		int coreLevel = RitualCircleRules.GetCoreLevel(building.GetCastle().GetBuildingStates());
		if (coreLevel < def.RequiredCoreLevel)
			return false;
		if (def.CostType == RitualCostType.Gold)
			return AdapterRegistry.Resolve<Shop>().CanAfford(def.CostAmount);
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		return blackMagic != null && blackMagic.Value >= def.CostAmount;
	}

	private static string GetCostText(RitualDef def)
	{
		string cost = def.CostType == RitualCostType.Gold ? $"{def.CostAmount} 金币" : $"{def.CostAmount} 黑魔法";
		return cost + "·" + def.DurationSeconds.ToString("0") + "s";
	}
}
