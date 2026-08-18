// 法术栏组件：仪式技能槽（spell_bar.tscn 场景根）
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Building;
using Godot;
using System.Collections.Generic;

public partial class SpellBarUiController : Control
{
	private const float SlotWidth = 150f;
	private const float SlotHeight = 100f;

	private ScrollContainer _scroll;
	private HBoxContainer _slotRow;
	private Control _handPanel;
	private readonly List<Button> _slotButtons = new();

	private UnitSelectionController _selection;
	private Building _currentBuilding;
	private bool _inputBlocked;
	private bool _ritualInProgress;
	private bool _hiddenByRing;
	private UiProfileKind _profile;

	private UnitSelectionController Selection => _selection ??= AdapterRegistry.Resolve<UnitSelectionController>();

	public override void _Ready()
	{
		_scroll = GetNode<ScrollContainer>("SpellBarPanel");
		_slotRow = GetNode<HBoxContainer>("SpellBarPanel/SpellSlots");
		_handPanel = GetParent().GetNode<HandUiController>("Hand").HandPanel;
		_scroll.Visible = false;
		_handPanel.Visible = true;
		DisplaySettingsManager settings = AdapterRegistry.Resolve<DisplaySettingsManager>();
		_profile = settings.UiProfile;
		settings.UiProfileChanged += ApplyProfile;
		AdapterRegistry.Register<SpellBarUiController>(this);
	}

	public override void _ExitTree()
	{
		AdapterRegistry.Resolve<DisplaySettingsManager>().UiProfileChanged -= ApplyProfile;
		ClearSlots();
		AdapterRegistry.Unregister<SpellBarUiController>(this);
	}

	private void ApplyProfile(UiProfileKind kind)
	{
		_profile = kind;
		foreach (Button button in _slotButtons)
			UiProfileStyles.ApplyCardButton(button, false, true, kind);
	}

	public override void _Process(double _)
	{
		Building building = Selection.SelectedBuilding;
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

	public bool IsHiddenByRing => _hiddenByRing;

	public void SetHiddenByRing(bool hidden)
	{
		if (_hiddenByRing == hidden)
			return;

		_hiddenByRing = hidden;
		RefreshVisibility();
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

		RitualService ritualService = GameManager.Get<RitualService>();
		if (ritualService == null)
			return;
		IReadOnlyList<RitualDef> defs = ritualService.GetAvailableRituals();
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
		UiProfileStyles.ApplyCardButton(button, false, true, _profile);
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
		UiProfileStyles.ApplyCardButton(button, false, true, _profile);
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
		RitualService ritualService = GameManager.Get<RitualService>();
		if (ritualService == null)
			return;
		IReadOnlyList<RitualDef> defs = ritualService.GetAvailableRituals();
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
			&& !_hiddenByRing
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
		return building.CanUseSpellBar();
	}

	private static bool IsAvailable(Building building, RitualDef def)
	{
		if (building.GetCastle() == null)
			return false;
		RitualService ritualService = GameManager.Get<RitualService>();
		BlackMagicService blackMagic = GameManager.Get<BlackMagicService>();
		int coreLevel = RitualCircleRules.GetCoreLevel(building.GetCastle().GetBuildingStates());
		if (RitualRules.IsStudyRitual(def.Id))
		{
			if (ritualService == null || blackMagic == null || !HasUnlearnedBook(ritualService, coreLevel))
				return false;
			return blackMagic.Value >= DarkMagicStudyRules.GetCost(ritualService.StudyCompletionCount);
		}

		if (coreLevel < def.RequiredCoreLevel)
			return false;
		if (def.CostType == RitualCostType.Gold)
			return AdapterRegistry.Resolve<Shop>().CanAfford(def.CostAmount);
		return blackMagic != null && blackMagic.Value >= def.CostAmount;
	}

	private static bool HasUnlearnedBook(RitualService ritualService, int coreLevel)
	{
		Hand hand = AdapterRegistry.Resolve<Hand>();
		if (hand == null)
			return false;
		foreach (CardData card in hand.Cards)
		{
			if (ritualService.IsBookStudiable(card, coreLevel))
				return true;
		}
		return false;
	}

	private static string GetCostText(RitualDef def)
	{
		if (RitualRules.IsStudyRitual(def.Id))
		{
			RitualService ritualService = GameManager.Get<RitualService>();
			int studyCost = ritualService == null ? def.CostAmount : DarkMagicStudyRules.GetCost(ritualService.StudyCompletionCount);
			return studyCost + " 黑魔法·需魔法书·" + def.DurationSeconds.ToString("0") + "s";
		}
		string cost = def.CostType == RitualCostType.Gold ? $"{def.CostAmount} 金币" : $"{def.CostAmount} 黑魔法";
		return cost + "·" + def.DurationSeconds.ToString("0") + "s";
	}
}