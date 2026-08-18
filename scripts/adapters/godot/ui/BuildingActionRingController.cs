// 建筑行动环组件：选中建筑时在环上提供卖出/暂停/升级/特殊行动按钮（building_action_ring.tscn 场景根）
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;
using System.Collections.Generic;

public partial class BuildingActionRingController : Control
{
	private const float ButtonSize = 64f;
	private const float RingRadius = 150f;
	private const float SlotStep = Mathf.Pi / 6f;
	private const int ButtonCornerRadius = 32;

	private const string SellKind = "Sell";
	private const string PauseKind = "Pause";
	private const string UpgradeKind = "Upgrade";
	private const string SpecialKind = "Special";

	private BuildingActionRing _ring;
	private UnitSelectionController _selection;
	private SpellBarUiController _spellBarUi;
	private readonly List<Button> _buttons = new();
	private readonly List<string> _buttonKinds = new();
	private readonly List<RingAction> _currentActions = new();

	private Building _currentBuilding;
	private bool _gameOver;
	private bool _inputBlocked;
	private Control _sellConfirmOverlay;
	private Label _sellConfirmMessage;
	private Button _sellConfirmOk;
	private Button _sellConfirmCancel;
	private Building _pendingSell;
	private UiProfileKind _profile;

	private UnitSelectionController Selection => _selection ??= AdapterRegistry.Resolve<UnitSelectionController>();
	private SpellBarUiController SpellBarUi => _spellBarUi ??= AdapterRegistry.Resolve<SpellBarUiController>();

	public override void _Ready()
	{
		_ring = GetNode<BuildingActionRing>("BuildingActionRing");
		DisplaySettingsManager settings = AdapterRegistry.Resolve<DisplaySettingsManager>();
		_profile = settings.UiProfile;
		ApplyProfile(_profile);
		settings.UiProfileChanged += ApplyProfile;
		_ring.Visible = false;
		_sellConfirmOverlay = GetNode<Control>("SellConfirmOverlay");
		_sellConfirmMessage = GetNode<Label>("SellConfirmOverlay/SellConfirmPanel/MessageLabel");
		_sellConfirmOk = GetNode<Button>("SellConfirmOverlay/SellConfirmPanel/ConfirmButton");
		_sellConfirmCancel = GetNode<Button>("SellConfirmOverlay/SellConfirmPanel/CancelButton");
		_sellConfirmOk.Pressed += ConfirmSell;
		_sellConfirmCancel.Pressed += CloseSellConfirm;
	}

	public override void _ExitTree()
	{
		AdapterRegistry.Resolve<DisplaySettingsManager>().UiProfileChanged -= ApplyProfile;
		ClearButtons();
		_sellConfirmOk.Pressed -= ConfirmSell;
		_sellConfirmCancel.Pressed -= CloseSellConfirm;
		_sellConfirmOverlay.Visible = false;
		SpellBarUi.SetHiddenByRing(false);
	}

	private void ApplyProfile(UiProfileKind kind)
	{
		_profile = kind;
		_ring.ApplyProfile(kind);
		foreach (Button button in _buttons)
			ApplyButtonStyle(button);
	}

	public bool TryHandleEscape()
	{
		if (!_sellConfirmOverlay.Visible)
			return false;

		CloseSellConfirm();
		return true;
	}

	public void SetGameOver(bool gameOver)
	{
		_gameOver = gameOver;
		if (_gameOver)
			HideRing();
	}

	public void SetInputBlocked(bool blocked)
	{
		_inputBlocked = blocked;
		if (_inputBlocked)
			HideRing();
	}

	public override void _Process(double _)
	{
		if (_gameOver || _inputBlocked)
		{
			HideRing();
			return;
		}

		Building building = Selection.SelectedBuilding;
		if (!CanShowRing(building))
		{
			HideRing();
			return;
		}

		_ring.Position = WorldToScreen(building.GlobalPosition);
		_ring.Visible = true;
		if (building != _currentBuilding)
		{
			_currentBuilding = building;
			RebuildButtons(building);
		}
		else
		{
			RefreshButtonStates(building);
		}
	}

	private Vector2 WorldToScreen(Vector2 worldPosition)
	{
		return _ring.GetViewport().GetCanvasTransform() * worldPosition;
	}

	private void HideRing()
	{
		if (_currentBuilding == null && !_ring.Visible)
			return;

		_currentBuilding = null;
		ClearButtons();
		_ring.Visible = false;
		SpellBarUi.SetHiddenByRing(false);
	}

	private void RebuildButtons(Building building)
	{
		ClearButtons();
		List<RingAction> actions = BuildActions(building);
		float nextAngle = SlotStep;
		for (int i = 0; i < actions.Count; i++)
		{
			float angle = ResolveAngle(actions[i].Kind, ref nextAngle);
			CreateButton(building, actions[i], angle);
		}
	}

	private static float ResolveAngle(string kind, ref float nextAngle)
	{
		if (kind == SellKind)
			return 0f;
		if (kind == PauseKind)
			return -SlotStep;

		float angle = nextAngle;
		nextAngle += SlotStep;
		return angle;
	}

	private List<RingAction> BuildActions(Building building)
	{
		List<RingAction> actions = new();
		if (CanSell(building))
			actions.Add(BuildSellAction(building));
		if (CanManage(building))
			actions.Add(BuildPauseAction(building));
		AddUpgradeActions(actions, building);
		if (building.CanUseSpellBar())
			actions.Add(BuildSpecialAction());
		return actions;
	}

	private void CreateButton(Building building, RingAction action, float angle)
	{
		Button button = new Button
		{
			CustomMinimumSize = new Vector2(ButtonSize, ButtonSize),
			Icon = action.Icon,
			ExpandIcon = true,
			Text = "",
			TooltipText = action.Tooltip,
			ToggleMode = action.Kind == PauseKind || action.Kind == SpecialKind,
		};
		ApplyButtonStyle(button);
		button.SetPressedNoSignal(action.Toggled);
		button.Disabled = !action.Enabled;
		Vector2 offset = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * RingRadius;
		button.Position = offset - new Vector2(ButtonSize * 0.5f, ButtonSize * 0.5f);
		WireButton(button, building, action);
		_ring.AddChild(button);
		_buttons.Add(button);
		_buttonKinds.Add(action.Kind);
		_currentActions.Add(action);
	}

	private void WireButton(Button button, Building building, RingAction action)
	{
		switch (action.Kind)
		{
			case SellKind:
				button.Pressed += () => TrySell(building);
				break;
			case PauseKind:
				button.Toggled += pressed => TogglePause(building, pressed);
				break;
			case UpgradeKind:
				button.Pressed += () => TryUpgrade(building, action.Recipe);
				break;
			case SpecialKind:
				button.Toggled += ToggleSpellBar;
				break;
		}
	}

	private void RefreshButtonStates(Building building)
	{
		for (int i = 0; i < _buttons.Count; i++)
		{
			string kind = _buttonKinds[i];
			if (kind == PauseKind)
				_buttons[i].SetPressedNoSignal(building.IsManuallyPaused);
			else if (kind == SpecialKind)
				_buttons[i].SetPressedNoSignal(!SpellBarUi.IsHiddenByRing);
			else if (kind == UpgradeKind)
				_buttons[i].Disabled = !IsUpgradeEnabled(building, _currentActions[i].Recipe);
		}
	}

	private static bool IsUpgradeEnabled(Building building, UpgradeRecipe recipe)
	{
		UpgradeRules rules = GameManager.Get<UpgradeRules>();
		IBuildingRepository repo = GameManager.Get<IBuildingRepository>();
		Castle castle = building.GetCastle();
		if (castle == null)
			return false;

		List<IBuildingState> states = castle.GetBuildingStates();
		int cost = rules.CalculateCost(building.TypeId, recipe.ResultTypeId, repo);
		if (cost <= 0)
			return false;

		bool canUpgrade = rules.CanUpgrade(building, repo,
			GetCoreLevel(building, states), HordeCoreRules.CountBurrows(states));
		return canUpgrade && AdapterRegistry.Resolve<Shop>().CanAfford(cost);
	}

	private static RingAction BuildSellAction(Building building)
	{
		int price = SellRules.GetSellPrice(GameManager.Get<IBuildingRepository>().GetValue(building.TypeId));
		return new RingAction
		{
			Kind = SellKind,
			Icon = BuildingIcons.Sell,
			Tooltip = $"出售（返还 {price} 金币）",
			Enabled = true,
		};
	}

	private static RingAction BuildPauseAction(Building building)
	{
		return new RingAction
		{
			Kind = PauseKind,
			Icon = BuildingIcons.Pause,
			Tooltip = "暂停建筑",
			Enabled = true,
			Toggled = building.IsManuallyPaused,
		};
	}

	private static void AddUpgradeActions(List<RingAction> actions, Building building)
	{
		UpgradeRules rules = GameManager.Get<UpgradeRules>();
		IBuildingRepository repo = GameManager.Get<IBuildingRepository>();
		Castle castle = building.GetCastle();
		if (castle == null)
			return;

		foreach (UpgradeRecipe recipe in rules.GetRecipes())
		{
			if (recipe.SourceTypeId != building.TypeId)
				continue;

			int cost = rules.CalculateCost(building.TypeId, recipe.ResultTypeId, repo);
			if (cost <= 0)
				continue;

			string resultName = AdapterRegistry.Resolve<BuildingSystem>().GetDisplayName(recipe.ResultTypeId);
			actions.Add(new RingAction
			{
				Kind = UpgradeKind,
				Icon = BuildingIcons.Upgrade,
				Tooltip = $"升级为{resultName}（{cost} 金币）",
				Enabled = IsUpgradeEnabled(building, recipe),
				Recipe = recipe,
			});
		}
	}

	private static int GetCoreLevel(Building building, List<IBuildingState> states)
	{
		bool isNecromancer = GameManager.Get<IBuildingRepository>().GetFactionId(building.TypeId) == FactionDef.NecromancerId;
		return isNecromancer ? RitualCircleRules.GetCoreLevel(states) : HordeCoreRules.GetCoreLevel(states);
	}

	private RingAction BuildSpecialAction()
	{
		return new RingAction
		{
			Kind = SpecialKind,
			Icon = BuildingIcons.Ritual,
			Tooltip = "仪式",
			Enabled = true,
			Toggled = !SpellBarUi.IsHiddenByRing,
		};
	}

	private void TrySell(Building building)
	{
		if (building == null || !GodotObject.IsInstanceValid(building) || !CanSell(building))
			return;

		if (DisplaySettingsManager.ConfirmSellEnabled)
		{
			_pendingSell = building;
			ShowSellConfirm(building);
			return;
		}

		SellBuilding(building);
	}

	private void ShowSellConfirm(Building building)
	{
		int price = SellRules.GetSellPrice(GameManager.Get<IBuildingRepository>().GetValue(building.TypeId));
		_sellConfirmMessage.Text = $"出售「{building.DisplayName}」将返还 {price} 金币，确定出售？";
		_sellConfirmOverlay.Visible = true;
		Sfx.Play(SfxEvents.ButtonClick);
	}

	private void ConfirmSell()
	{
		Building building = _pendingSell;
		CloseSellConfirm();
		if (building == null || !GodotObject.IsInstanceValid(building))
			return;

		SellBuilding(building);
	}

	private void CloseSellConfirm()
	{
		_pendingSell = null;
		_sellConfirmOverlay.Visible = false;
	}

	private void SellBuilding(Building building)
	{
		if (building == null || !GodotObject.IsInstanceValid(building) || !CanSell(building))
			return;

		Castle castle = building.GetCastle();
		if (castle == null)
			return;

		int price = SellRules.GetSellPrice(GameManager.Get<IBuildingRepository>().GetValue(building.TypeId));
		AdapterRegistry.Resolve<Shop>().AddGold(price);
		castle.ReleaseBuildingFootprint(building);
		building.GetParent()?.RemoveChild(building);
		building.QueueFree();
		GameManager.Get<AdjacencyService>()?.RefreshCastle(castle.GetBuildingStates());
		Sfx.Play(SfxEvents.CoinChange);
	}

	private void TogglePause(Building building, bool pressed)
	{
		if (building == null || !GodotObject.IsInstanceValid(building))
			return;

		building.SetManuallyPaused(pressed);
		Sfx.Play(SfxEvents.ButtonClick);
	}

	private void ToggleSpellBar(bool pressed)
	{
		SpellBarUi.SetHiddenByRing(!pressed);
		Sfx.Play(SfxEvents.ButtonClick);
	}

	private void TryUpgrade(Building building, UpgradeRecipe recipe)
	{
		if (building == null || !GodotObject.IsInstanceValid(building) || recipe == null)
			return;

		Castle castle = building.GetCastle();
		if (castle == null)
			return;

		float healthRatio = building.MaxHealth > 0 ? building.Health / (float)building.MaxHealth : 1f;
		List<IBuildingState> states = castle.GetBuildingStates();
		IUpgradeUseCase useCase = GameManager.Get<IUpgradeUseCase>();
		IBuildingState upgraded = useCase.TryUpgrade(
			building, recipe.ResultTypeId, healthRatio,
			AdapterRegistry.Resolve<Shop>().TrySpendGold,
			new UpgradeBuildingFactory(castle),
			GetCoreLevel(building, states),
			HordeCoreRules.CountBurrows(states));
		if (upgraded == null)
			return;

		Sfx.Play(SfxEvents.Upgrade);
	}

	private void ClearButtons()
	{
		foreach (Button button in _buttons)
		{
			_ring.RemoveChild(button);
			button.QueueFree();
		}
		_buttons.Clear();
		_buttonKinds.Clear();
		_currentActions.Clear();
	}

	private static bool CanShowRing(Building building)
	{
		if (building == null || !GodotObject.IsInstanceValid(building) || building.IsDestroyed)
			return false;
		if (!building.IsPlayerOwned)
			return false;
		return !GameManager.Get<IBuildingRepository>().IsCoreBuilding(building.TypeId);
	}

	private static bool CanSell(Building building)
	{
		if (building == null || !GodotObject.IsInstanceValid(building) || building.IsDestroyed)
			return false;
		if (building.IsRitualInProgress || !building.IsPlayerOwned)
			return false;
		return !GameManager.Get<IBuildingRepository>().IsCoreBuilding(building.TypeId);
	}

	private static bool CanManage(Building building)
	{
		if (building == null || building.GetCastle() == null)
			return false;
		if (!building.GetCastle().IsPlayerCastle)
			return false;
		return !AdapterRegistry.Resolve<BuildingSystem>().IsCoreBuilding(building.TypeId);
	}

	private void ApplyButtonStyle(Button button)
	{
		button.AddThemeStyleboxOverride("normal", CreateButtonStyle(button, false, 0));
		button.AddThemeStyleboxOverride("hover", CreateButtonStyle(button, true, 1));
		button.AddThemeStyleboxOverride("pressed", CreateButtonStyle(button, true, 2));
		button.AddThemeStyleboxOverride("hover_pressed", CreateButtonStyle(button, true, 3));
		button.AddThemeStyleboxOverride("disabled", CreateDisabledStyle(button));
	}

	private StyleBox CreateButtonStyle(Button button, bool hover, int offset)
	{
		if (_profile == UiProfileKind.Cartoon)
		{
			Color color = hover ? new Color(1f, 0.94f, 0.75f, 1f) : new Color(1f, 0.9f, 0.62f, 1f);
			return HandDrawnUiStyles.CreatePanel(color, Variant(button, offset), 5f);
		}
		StyleBoxFlat style = new();
		style.BgColor = new Color(0.08f, 0.12f, 0.2f, 0.95f);
		style.BorderColor = hover ? new Color(1f, 0.88f, 0.5f, 1f) : new Color(0.55f, 0.65f, 0.8f, 1f);
		style.SetBorderWidthAll(2);
		style.SetCornerRadiusAll(ButtonCornerRadius);
		return style;
	}

	private StyleBox CreateDisabledStyle(Button button)
	{
		if (_profile == UiProfileKind.Cartoon)
			return HandDrawnUiStyles.CreatePanel(new Color(1f, 0.99f, 0.96f, 1f), Variant(button, 4), 5f);
		StyleBoxFlat style = new();
		style.BgColor = new Color(0.12f, 0.14f, 0.18f, 0.7f);
		style.BorderColor = new Color(0.35f, 0.38f, 0.45f, 0.8f);
		style.SetBorderWidthAll(2);
		style.SetCornerRadiusAll(ButtonCornerRadius);
		return style;
	}

	private static int Variant(Control control, int offset)
	{
		return Mathf.Abs(control.GetInstanceId().GetHashCode() + offset);
	}

	private sealed class RingAction
	{
		public string Kind;
		public Texture2D Icon;
		public string Tooltip;
		public bool Enabled;
		public bool Toggled;
		public UpgradeRecipe Recipe;
	}
}