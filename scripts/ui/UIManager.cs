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
    private PauseMenuUiController _pauseMenuUi;
    private GameOverUiController _gameOverUi;
    private SettingsUiController _settingsUi;
    private bool _gameOver;

    public override void _Ready()
    {
        Instance = this;
        SetProcess(true);
        SetProcessInput(true);
        InitializeUI();
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GameStateChanged -= OnGameStateChanged;

        _hudUi?.Dispose();
        if (_shopUi != null)
        {
            _shopUi.Dispose();
        }
        _handUi?.Dispose();
        _buildingManageUi?.Dispose();
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
        var uiRoot = GetParent()?.GetNodeOrNull<CanvasLayer>("UI");
        if (uiRoot == null) return;

        _hudUi = new HudUiController(uiRoot);
        _shopUi = new ShopUiController(this, uiRoot);
        _handUi = new HandUiController(this, uiRoot);
        _buildingInfoUi = new BuildingInfoUiController(this, uiRoot);
        _buildingManageUi = new BuildingManageUiController(this, uiRoot);
        _settingsUi = new SettingsUiController(uiRoot.GetNode<Control>("SettingsPanel"));
        _pauseMenuUi = new PauseMenuUiController(uiRoot, GoToTitle, OpenSettings);
        _gameOverUi = new GameOverUiController(uiRoot, GoToTitle);

        _pauseMenuUi.OpenChanged += OnPauseMenuOpenChanged;
        _settingsUi.OpenChanged += OnSettingsOpenChanged;

        if (GameManager.Instance != null)
            GameManager.Instance.GameStateChanged += OnGameStateChanged;

        OnGameStateChanged(GameManager.Instance.CurrentState);
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
            _buildingManageUi?.SetInputBlocked(_gameOver);

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
        }
        else
        {
            _buildingManageUi?.SetInputBlocked(_gameOver);
        }

        UpdateHandInputBlocked();
    }

    public void GoToTitle()
    {
        GameManager.Instance?.SetPaused(false);
        CardSystem.Instance?.ResetHand();
        GetTree().ChangeSceneToFile(TitleScene);
    }
}
