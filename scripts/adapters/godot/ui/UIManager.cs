using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;

public partial class UIManager : Node2D
{
    public static UIManager Instance { get; private set; }

    private const string TitleScene = "res://scenes/ui/title_screen.tscn";

    private HudUiController _hudUi;
    private ShopUiController _shopUi;
    private HandUiController _handUi;
    private BuildingInfoUiController _buildingInfoUi;
    private BuildingManageUiController _buildingManageUi;
    private FusionProhibitUiController _fusionProhibitUi;
    private PauseMenuUiController _pauseMenuUi;
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
        _fusionProhibitUi?.Dispose();
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
        _shopUi?.Process();
        _buildingManageUi?.Process();
        if (_shopUi?.IsDragging != true)
            _handUi?.Process();

        _buildingInfoUi?.SetPlacementActive(
            _handUi?.IsPlacementActive == true || _shopUi?.IsDragging == true);
        _buildingInfoUi?.SetPauseOpen(_pauseMenuUi?.IsOpen == true || _settingsUi?.IsOpen == true);
        _buildingInfoUi?.Process();
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

            if (_fusionProhibitUi?.TryHandleEscape() == true)
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

        if (_fusionProhibitUi?.HandleInput(@event) == true)
        {
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_buildingManageUi?.HandleInput(@event) == true)
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
        }
    }

    private void InitializeUI()
    {
        CanvasLayer uiRoot = GetParent()?.GetNodeOrNull<CanvasLayer>("UI");
        if (uiRoot == null) return;

        _hudUi = new HudUiController(uiRoot);
        Shop shopService = AdapterRegistry.Resolve<Shop>();
        _shopUi = new ShopUiController(this, uiRoot, shopService);
        Hand handService = AdapterRegistry.Resolve<Hand>();
        _handUi = new HandUiController(this, uiRoot, handService);
        _buildingInfoUi = new BuildingInfoUiController(this, uiRoot);
        ButtonGroup toolGroup = new ButtonGroup();
        _buildingManageUi = new BuildingManageUiController(this, uiRoot, toolGroup);
        _fusionProhibitUi = new FusionProhibitUiController(this, uiRoot, toolGroup);
        _settingsUi = new SettingsUiController(uiRoot.GetNode<Control>("SettingsPanel"));
        _pauseMenuUi = new PauseMenuUiController(uiRoot, GoToTitle, OpenSettings);
        _gameOverUi = new GameOverUiController(uiRoot, GoToTitle);

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
        _handUi.SetInputBlocked(_gameOver);
        _buildingInfoUi.SetInputBlocked(_gameOver);
        _buildingManageUi.SetGameOver(_gameOver);
        _fusionProhibitUi.SetGameOver(_gameOver);
        _pauseMenuUi.SetGameOver(_gameOver);
        _gameOverUi.SetState(state);
    }

    private void UpdateHandInputBlocked()
    {
        _handUi?.SetInputBlocked(_pauseMenuUi?.IsOpen == true || _settingsUi?.IsOpen == true || _gameOver);
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
            _fusionProhibitUi?.SetInputBlocked(_gameOver);
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
            _fusionProhibitUi?.SetInputBlocked(true);
        }
        else
        {
            _buildingManageUi?.SetInputBlocked(_gameOver);
            _fusionProhibitUi?.SetInputBlocked(_gameOver);
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
        AdapterRegistry.Resolve<BattleReportSystem>()?.DiscardCurrentReport();
        GetTree().ChangeSceneToFile(TitleScene);
    }
}
