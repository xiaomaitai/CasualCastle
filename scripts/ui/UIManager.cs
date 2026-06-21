using Godot;

public partial class UIManager : Node2D
{
    public static UIManager Instance { get; private set; }

    private const string TitleScene = "res://scenes/ui/title_screen.tscn";

    private HudUiController _hudUi;
    private ShopUiController _shopUi;
    private HandUiController _handUi;
    private GameOverUiController _gameOverUi;
    private bool _gameOver;

    public override void _Ready()
    {
        Instance = this;
        SetProcess(true);
        InitializeUI();
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GameStateChanged -= OnGameStateChanged;

        if (_shopUi != null)
            _shopUi.OpenChanged -= OnShopOpenChanged;

        _hudUi?.Dispose();
        _shopUi?.Dispose();
        _handUi?.Dispose();
        _gameOverUi?.Dispose();

        if (Instance == this)
            Instance = null;
    }

    public override void _Process(double _)
    {
        _hudUi?.Process();
        _handUi?.Process();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo
            && keyEvent.Keycode == Key.Escape
            && _shopUi?.Close() == true)
        {
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_handUi?.HandleInput(@event) == true)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    private void InitializeUI()
    {
        var uiRoot = GetParent()?.GetNodeOrNull<CanvasLayer>("UI");
        if (uiRoot == null) return;

        _hudUi = new HudUiController(uiRoot);
        _shopUi = new ShopUiController(uiRoot);
        _handUi = new HandUiController(this, uiRoot);
        _gameOverUi = new GameOverUiController(uiRoot, GoToTitle);

        _shopUi.OpenChanged += OnShopOpenChanged;

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
        _handUi.SetInputBlocked(_gameOver || _shopUi.IsOpen);
        _gameOverUi.SetState(state);
    }

    private void OnShopOpenChanged(bool open)
    {
        _handUi.SetInputBlocked(_gameOver || open);
    }

    private void GoToTitle()
    {
        CardSystem.Instance?.ResetHand();
        GetTree().ChangeSceneToFile(TitleScene);
    }
}
