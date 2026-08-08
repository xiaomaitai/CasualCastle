// UI 总控：组合各面板控制器并分发输入
using CasualCastle.Adapters.Godot;
using CasualCastle.Adapters.Godot.Dev;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Battle;
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
	private UnitInfoUiController _unitInfoUi;
	private UnitSelectionController _unitSelection;
	private SpellBarUiController _spellBarUi;
	private TavernBubble _tavernBubble;
	private TavernPartyService _tavernParty;
	private AttributePanelController _attributePanel;
	private GameOverUiController _gameOverUi;
	private SettingsUiController _settingsUi;
	private bool _gameOver;
	private bool _isChangingScene;

	public override void _Ready()
	{
		Instance = this;
		SetProcess(true);
		SetProcessInput(true);
		InitializeUI();
	}

	public override void _ExitTree()
	{
		if (AdapterRegistry.Resolve<GameManager>() != null)
			AdapterRegistry.Resolve<GameManager>().GameStateChanged -= OnGameStateChanged;

		_hudUi?.Dispose();
		if (_shopUi != null)
		{
			_shopUi.Dispose();
		}
		_handUi?.Dispose();
		_buildingManageUi?.Dispose();
		_combineProhibitUi?.Dispose();
		_cheatMenu?.Dispose();
		_runtimeInfoPanel?.Dispose();
		if (_pauseMenuUi != null)
		{
			_pauseMenuUi.OpenChanged -= OnPauseMenuOpenChanged;
			_pauseMenuUi.Dispose();
		}
		if (_settingsUi != null)
			_settingsUi.OpenChanged -= OnSettingsOpenChanged;
		_gameOverUi?.Dispose();
		_settingsUi?.Dispose();

		if (Instance == this)
			Instance = null;
	}

	public override void _Process(double _)
	{
		_hudUi?.Process();
		_runtimeInfoPanel?.Process();
		_shopUi?.Process();
		_buildingManageUi?.Process();
		_buildingSell?.Process();
		if (_shopUi?.IsDragging != true)
			_handUi?.Process();

		_buildingInfoUi?.SetPlacementActive(
			_handUi?.IsPlacementActive == true || _shopUi?.IsDragging == true);
		_buildingInfoUi?.SetPauseOpen(_pauseMenuUi?.IsOpen == true || _settingsUi?.IsOpen == true);
		_buildingInfoUi?.Process();
		_unitInfoUi?.Process();
		_unitSelection?.Process();
		_spellBarUi?.Process();
		EnsureTavernBubble();
		_tavernBubble?.Process(_unitSelection.SelectedBuilding, _tavernParty);
		_attributePanel?.Process();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo
			&& keyEvent.Keycode == Key.Escape)
		{
			if (_settingsUi?.Close() == true)
			{
				GetViewport().SetInputAsHandled();
				return;
			}

			if (_cheatMenu?.TryHandleEscape() == true)
			{
				GetViewport().SetInputAsHandled();
				return;
			}
			if (_runtimeInfoPanel?.TryHandleEscape() == true)
			{
				GetViewport().SetInputAsHandled();
				return;
			}

			if (_combineProhibitUi?.TryHandleEscape() == true)
			{
				GetViewport().SetInputAsHandled();
				return;
			}

			if (_buildingManageUi?.TryHandleEscape() == true)
			{
				GetViewport().SetInputAsHandled();
				return;
			}

			if (_pauseMenuUi?.Close() == true)
			{
				GetViewport().SetInputAsHandled();
				return;
			}

			if (_handUi?.TryHandleEscape() == true)
			{
				GetViewport().SetInputAsHandled();
				return;
			}

			if (_shopUi?.CancelDrag() == true)
			{
				GetViewport().SetInputAsHandled();
				return;
			}

			if (_shopUi?.Close() == true)
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

		if (_pauseMenuUi?.IsOpen == true)
			return;

		if (_settingsUi?.IsOpen == true)
			return;

		if (_cheatMenu?.IsOpen == true)
			return;

		if (_combineProhibitUi?.HandleInput(@event) == true)
		{
			GetViewport().SetInputAsHandled();
			return;
		}

		if (_buildingManageUi?.HandleInput(@event) == true)
		{
			GetViewport().SetInputAsHandled();
			return;
		}

		if (_buildingSell?.HandleInput(@event) == true)
		{
			GetViewport().SetInputAsHandled();
			return;
		}

		if (_handUi?.HandleInput(@event) == true)
		{
			GetViewport().SetInputAsHandled();
			return;
		}

		if (_shopUi?.HandleInput(@event) == true)
		{
			GetViewport().SetInputAsHandled();
			return;
		}

		EnsureTavernBubble();
		if (_tavernBubble?.HandleInput(@event, _tavernParty) == true)
		{
			GetViewport().SetInputAsHandled();
			return;
		}

		if (_unitSelection?.HandleInput(@event) == true)
		{
			GetViewport().SetInputAsHandled();
			return;
		}
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

		_hudUi = new HudUiController(uiRoot);
		Shop shopService = AdapterRegistry.Resolve<Shop>();
		_shopUi = new ShopUiController(this, uiRoot, shopService);
		Hand handService = AdapterRegistry.Resolve<Hand>();
		_handUi = new HandUiController(this, uiRoot, handService, _shopUi);
		Castle playerCastle = GetParent().GetNode<Castle>("Battlefield/PlayerSide/PlayerCastle");
		_buildingSell = new BuildingSellController(this, playerCastle, shopService, _shopUi);
		_buildingInfoUi = new BuildingInfoUiController(this, uiRoot);
		_unitInfoUi = new UnitInfoUiController(this, uiRoot);
		_unitSelection = new UnitSelectionController(this);
		_spellBarUi = new SpellBarUiController(uiRoot, _unitSelection);
		_tavernParty = GameManager.Get<TavernPartyService>();
		_tavernBubble = new TavernBubble();
		GetParent()?.GetNodeOrNull<Node2D>("Battlefield")?.AddChild(_tavernBubble);
		_attributePanel = new AttributePanelController(this, uiRoot, _unitSelection);
		ButtonGroup toolGroup = new ButtonGroup();
		_buildingManageUi = new BuildingManageUiController(this, uiRoot, toolGroup);
		_combineProhibitUi = new CombineProhibitUiController(this, uiRoot, toolGroup);
		_cheatMenu = new CheatMenuUiController(uiRoot);
		_runtimeInfoPanel = new RuntimeInfoPanelController(uiRoot);
		_settingsUi = new SettingsUiController(uiRoot.GetNode<Control>("SettingsPanel"));
		_pauseMenuUi = new PauseMenuUiController(uiRoot, GoToTitle, OpenSettings);
		_gameOverUi = new GameOverUiController(uiRoot, GoToTitle);

		UiSfx.Bind(uiRoot);
		_pauseMenuUi.OpenChanged += OnPauseMenuOpenChanged;
		_settingsUi.OpenChanged += OnSettingsOpenChanged;

		if (AdapterRegistry.Resolve<GameManager>() != null)
			AdapterRegistry.Resolve<GameManager>().GameStateChanged += OnGameStateChanged;

		OnGameStateChanged(AdapterRegistry.Resolve<GameManager>().CurrentState);
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
		_combineProhibitUi.SetGameOver(_gameOver);
		_cheatMenu.SetGameOver(_gameOver);
		_runtimeInfoPanel?.SetGameOver(_gameOver);
		_pauseMenuUi.SetGameOver(_gameOver);
		_gameOverUi.SetState(state);
	}

	private void UpdateHandInputBlocked()
	{
		bool blocked = _pauseMenuUi?.IsOpen == true || _settingsUi?.IsOpen == true || _gameOver;
		_handUi?.SetInputBlocked(blocked);
		_buildingSell?.SetInputBlocked(blocked);
		_spellBarUi?.SetInputBlocked(blocked);
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
	}

	private void OnPauseMenuOpenChanged(bool open)
	{
		if (open)
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
}
