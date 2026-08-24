// UI 总控：组合各面板控制器并分发输入
using CasualCastle.Adapters.Godot;
using CasualCastle.Adapters.Godot.Dev;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Battle;
using CasualCastle.Domain.Shared;
using Godot;

public partial class UIManager : Node2D
{
	public static UIManager Instance { get; private set; }

	private const string TitleScene = "res://scenes/ui/title_screen.tscn";

	private HudUiController _hudUi;
	private ShopUiController _shopUi;
	private BuildingSellController _buildingSell;
	private HandUiController _handUi;
	private BuildingInfoUiController _buildingInfoUi;
	private BuildingManageUiController _buildingManageUi;
	private CombineProhibitUiController _combineProhibitUi;
	private PauseMenuUiController _pauseMenuUi;
	private CheatMenuUiController _cheatMenu;
	private RuntimeInfoPanelController _runtimeInfoPanel;
	private BgmInfoController _bgmInfo;
	private UnitInfoUiController _unitInfoUi;
	private UnitSelectionController _unitSelection;
	private SpellBarUiController _spellBarUi;
	private BuildingActionRingController _buildingActionRing;
	private TavernBubble _tavernBubble;
	private TavernPartyService _tavernParty;
	private AttributePanelController _attributePanel;
	private GameOverUiController _gameOverUi;
	private SettingsUiController _settingsUi;
	private UiInteractionState _interactionState;
	private UiInputRouter _inputRouter;
	private bool _gameOver;
	private bool _isChangingScene;

	public override void _Ready()
	{
		Instance = this;
		_interactionState = new UiInteractionState();
		_inputRouter = new UiInputRouter();
		SetProcess(true);
		SetProcessInput(true);
		CallDeferred(nameof(InitializeUI));
	}

	public override void _ExitTree()
	{
		if (AdapterRegistry.Resolve<GameManager>() != null)
		{
			AdapterRegistry.Resolve<GameManager>().GameStateChanged -= OnGameStateChanged;
			AdapterRegistry.Resolve<GameManager>().PhaseChanged -= OnPhaseChanged;
		}


		_combineProhibitUi?.Dispose();
		if (_pauseMenuUi != null)
			_pauseMenuUi.OpenChanged -= OnPauseMenuOpenChanged;
		if (_settingsUi != null)
			_settingsUi.OpenChanged -= OnSettingsOpenChanged;
		if (_shopUi != null)
			_shopUi.OpenChanged -= OnShopOpenChanged;

		AdapterRegistry.Unregister<UnitSelectionController>(_unitSelection);

		if (Instance == this)
			Instance = null;
	}

	public override void _Process(double _)
	{
		EnsureTavernBubble();
		_tavernBubble?.Process(_unitSelection.SelectedBuilding, _tavernParty);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if (keyEvent.Keycode == Key.Escape)
			{
				if (_inputRouter.HandleEscape())
				{
					GetViewport().SetInputAsHandled();
					return;
				}

				if (_pauseMenuUi?.Open() == true)
				{
					GetViewport().SetInputAsHandled();
					return;
				}
			}

			if (HandleGameHotkey(keyEvent.Keycode))
			{
				GetViewport().SetInputAsHandled();
				return;
			}
		}

		if (_interactionState.InputBlocked)
			return;

		if (_cheatMenu?.IsOpen == true)
			return;

		if (_inputRouter.HandleInput(@event))
		{
			GetViewport().SetInputAsHandled();
			return;
		}
	}

	private bool HandleGameHotkey(Key keycode)
	{
		switch (keycode)
		{
			case >= Key.Key1 and <= Key.Key9:
				int slotIndex = (int)(keycode - Key.Key1);
				if (_shopUi != null && _shopUi.IsOpen && slotIndex < Shop.OfferCount)
					return _shopUi.TryBuySlot(slotIndex);
				return _handUi != null && _handUi.SelectCardIndex(slotIndex);
			case Key.Space:
				return !_gameOver && _pauseMenuUi != null && _pauseMenuUi.Toggle();
			case Key.B:
				return _shopUi != null && _shopUi.ToggleOpen();
			case Key.S:
				if (_handUi != null && _handUi.IsPlacementActive)
					return false;
				return TrySellSelectedBuilding();
			case Key.M:
				AdapterRegistry.Resolve<DisplaySettingsManager>()?.ToggleMute();
				return true;
		}
		return false;
	}

	private bool TrySellSelectedBuilding()
	{
		if (_buildingSell == null || _unitSelection?.SelectedBuilding == null)
			return false;
		return _buildingSell.TrySell(_unitSelection.SelectedBuilding);
	}

	private void EnsureTavernBubble()
	{
		if (GodotObject.IsInstanceValid(_tavernBubble))
			return;
		_tavernBubble = new TavernBubble();
		GetParent()?.GetNodeOrNull<Node2D>("Battlefield")?.AddChild(_tavernBubble);
	}

	private void InitializeUI()
	{
		CanvasLayer uiRoot = GetParent()?.GetNodeOrNull<CanvasLayer>("UI");
		if (uiRoot == null) return;

		_hudUi = uiRoot.GetNode<HudUiController>("Hud");
		Shop shopService = AdapterRegistry.Resolve<Shop>();
		_shopUi = uiRoot.GetNode<ShopUiController>("Shop");
		_handUi = uiRoot.GetNode<HandUiController>("Hand");
		Castle playerCastle = GetParent().GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
		_buildingSell = new BuildingSellController { PlayerCastle = playerCastle, Shop = shopService, ShopUi = _shopUi };
		AddChild(_buildingSell);
		_buildingInfoUi = uiRoot.GetNode<BuildingInfoUiController>("BuildingInfo");
		_unitInfoUi = uiRoot.GetNode<UnitInfoUiController>("UnitInfo");
		_unitSelection = new UnitSelectionController();
		AddChild(_unitSelection);
		AdapterRegistry.Register<UnitSelectionController>(_unitSelection);
		_spellBarUi = uiRoot.GetNode<SpellBarUiController>("SpellBar");
		_buildingActionRing = uiRoot.GetNode<BuildingActionRingController>("ActionRingHost");
		_tavernParty = GameManager.Get<TavernPartyService>();
		_tavernBubble = new TavernBubble();
		GetParent()?.GetNodeOrNull<Node2D>("Battlefield")?.AddChild(_tavernBubble);
		_attributePanel = new AttributePanelController { Selection = _unitSelection };
		AddChild(_attributePanel);
		ButtonGroup toolGroup = new ButtonGroup();
		_buildingManageUi = new BuildingManageUiController { Hud = _hudUi, ToolGroup = toolGroup };
		AddChild(_buildingManageUi);
		_combineProhibitUi = new CombineProhibitUiController(this, uiRoot, _hudUi, toolGroup);
		_cheatMenu = uiRoot.GetNode<CheatMenuUiController>("CheatMenu");
		_runtimeInfoPanel = uiRoot.GetNode<RuntimeInfoPanelController>("RuntimeInfoPanel");
		_bgmInfo = new BgmInfoController { Label = _hudUi.BgmInfoLabel };
		AddChild(_bgmInfo);
		_settingsUi = uiRoot.GetNode<SettingsUiController>("SettingsPanel");
		_pauseMenuUi = uiRoot.GetNode<PauseMenuUiController>("PauseMenu");
		_pauseMenuUi.GoToTitleAction = GoToTitle;
		_pauseMenuUi.OpenSettingsAction = OpenSettings;
		_gameOverUi = uiRoot.GetNode<GameOverUiController>("GameOver");
		_gameOverUi.GoToTitleAction = GoToTitle;
		_gameOverUi.RestartMatchAction = RestartMatch;

		UiSfx.Bind(uiRoot);
		RegisterRouting();
		_pauseMenuUi.OpenChanged += OnPauseMenuOpenChanged;
		_settingsUi.OpenChanged += OnSettingsOpenChanged;
		_shopUi.OpenChanged += OnShopOpenChanged;

		if (AdapterRegistry.Resolve<GameManager>() != null)
		{
			AdapterRegistry.Resolve<GameManager>().GameStateChanged += OnGameStateChanged;
			AdapterRegistry.Resolve<GameManager>().PhaseChanged += OnPhaseChanged;
		}

		OnGameStateChanged(AdapterRegistry.Resolve<GameManager>().CurrentState);
	}

	private void RegisterRouting()
	{
		_inputRouter.RegisterEsc(100, () => _settingsUi?.Close() == true);
		_inputRouter.RegisterEsc(90, () => _cheatMenu?.TryHandleEscape() == true);
		_inputRouter.RegisterEsc(80, () => _runtimeInfoPanel?.TryHandleEscape() == true);
		_inputRouter.RegisterEsc(70, () => _combineProhibitUi?.TryHandleEscape() == true);
		_inputRouter.RegisterEsc(60, () => _buildingManageUi?.TryHandleEscape() == true);
		_inputRouter.RegisterEsc(50, () => _buildingActionRing?.TryHandleEscape() == true);
		_inputRouter.RegisterEsc(40, () => _pauseMenuUi?.Close() == true);
		_inputRouter.RegisterEsc(30, () => _handUi?.TryHandleEscape() == true);
		_inputRouter.RegisterEsc(20, () => _shopUi?.CancelDrag() == true);
		_inputRouter.RegisterEsc(10, () => _shopUi?.Close() == true);

		_inputRouter.RegisterInput(70, e => _combineProhibitUi?.HandleInput(e) == true);
		_inputRouter.RegisterInput(60, e => _buildingManageUi?.HandleInput(e) == true);
		_inputRouter.RegisterInput(50, e => _buildingSell?.HandleInput(e) == true);
		_inputRouter.RegisterInput(40, e => _handUi?.HandleInput(e) == true);
		_inputRouter.RegisterInput(30, e => _shopUi?.HandleInput(e) == true);
		_inputRouter.RegisterInput(20, e =>
		{
			EnsureTavernBubble();
			return _tavernBubble?.HandleInput(e, _tavernParty) == true;
		});
		_inputRouter.RegisterInput(10, e => _unitSelection?.HandleInput(e) == true);
	}

	public void OnGameStateChanged(GameManager.GameState state)
	{
		bool show = state == GameManager.GameState.GameOver;
		_gameOver = show;

		_hudUi.SetGameOverVisible(show);
		_shopUi.SetGameOver(show);
		_buildingSell?.SetInputBlocked(_gameOver);
		_handUi.SetInputBlocked(_gameOver);
		_unitSelection.SetInputBlocked(_gameOver);
		_spellBarUi?.SetInputBlocked(_gameOver);
		_buildingInfoUi.SetInputBlocked(_gameOver);
		_buildingManageUi.SetGameOver(_gameOver);
		_buildingActionRing?.SetGameOver(_gameOver);
		_combineProhibitUi.SetGameOver(_gameOver);
		_cheatMenu.SetGameOver(_gameOver);
		_runtimeInfoPanel?.SetGameOver(_gameOver);
		_pauseMenuUi.SetGameOver(_gameOver);
		_gameOverUi.SetState(state);
		RefreshInteractionState();
	}

	private void OnPhaseChanged(MatchPhase phase)
	{
		if (_gameOver || !DisplaySettingsManager.AutoPauseNightEnabled)
			return;
		if (phase == MatchPhase.Combat)
			_pauseMenuUi?.Open();
	}

	private void RefreshInteractionState()
	{
		_interactionState.Update(
			_gameOver,
			_pauseMenuUi?.IsOpen == true,
			_settingsUi?.IsOpen == true,
			_shopUi?.IsOpen == true);
	}

	private void UpdateHandInputBlocked()
	{
		bool blocked = _pauseMenuUi?.IsOpen == true || _settingsUi?.IsOpen == true || _gameOver;
		_handUi?.SetInputBlocked(blocked);
		_buildingSell?.SetInputBlocked(blocked);
		_spellBarUi?.SetInputBlocked(blocked);
		_buildingActionRing?.SetInputBlocked(blocked);
	}

	private void OpenSettings()
	{
		_settingsUi?.Open();
	}

	private void OnSettingsOpenChanged(bool open)
	{
		if (open)
			_buildingManageUi?.SetInputBlocked(true);
		else if (_pauseMenuUi?.IsOpen != true)
		{
			_buildingManageUi?.SetInputBlocked(_gameOver);
			_combineProhibitUi?.SetInputBlocked(_gameOver);
		}

		UpdateHandInputBlocked();
		RefreshInteractionState();
	}

	private void OnShopOpenChanged(bool open)
	{
		RefreshInteractionState();
	}

	private void OnPauseMenuOpenChanged(bool open)
	{		if (open)
		{
			_shopUi?.Close();
			_handUi?.TryHandleEscape();
			_shopUi?.CancelDrag();
			_buildingManageUi?.SetInputBlocked(true);
			_combineProhibitUi?.SetInputBlocked(true);
		}
		else
		{
			_buildingManageUi?.SetInputBlocked(_gameOver);
			_combineProhibitUi?.SetInputBlocked(_gameOver);
		}

		UpdateHandInputBlocked();
		RefreshInteractionState();
	}

	public void GoToTitle()
	{
		if (_isChangingScene)
			return;

		_isChangingScene = true;
		SetProcessInput(false);
		AdapterRegistry.Resolve<GameManager>()?.SetPaused(false);
		AdapterRegistry.Resolve<Hand>().ResetHand();
		AdapterRegistry.Resolve<BattleReportSystem>()?.ClearCurrentCache();
		GetTree().ChangeSceneToFile(TitleScene);
	}

	public void RestartMatch()
	{
		if (_isChangingScene)
			return;

		_isChangingScene = true;
		SetProcessInput(false);
		AdapterRegistry.Resolve<GameManager>()?.SetPaused(false);
		AdapterRegistry.Resolve<Hand>()?.ResetHand();
		AdapterRegistry.Resolve<BattleReportSystem>()?.ClearCurrentCache();
		GetTree().ReloadCurrentScene();
	}
}
