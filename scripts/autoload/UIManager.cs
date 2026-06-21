using Godot;
using System;

public partial class UIManager : Node2D
{
    public static UIManager Instance { get; private set; }

    private ProgressBar _playerHealthBar;
    private ProgressBar _enemyHealthBar;
    private ColorRect _gameOverOverlay;
    private Panel _gameOverPanel;
    private Label _gameOverLabel;
    private Button _backToTitleButton;
    private Label _phaseLabel;
    private Label _phaseTimerLabel;
    private Button _skipPhaseButton;
    private Label _goldLabel;
    private Button _shopButton;
    private ColorRect _shopOverlay;
    private Panel _shopPanel;
    private Label _shopGoldLabel;
    private Button _shopCloseButton;
    private Button _shopRefreshButton;
    private readonly Button[] _shopBuyButtons = new Button[ShopSystem.OfferCount];
    private readonly Label[] _shopOfferLabels = new Label[ShopSystem.OfferCount];
    private readonly Button[] _handButtons = new Button[CardSystem.MaxHandSize];
    private Label _placementHintLabel;

    private bool _shopOpen;

    private const string TitleScene = "res://scenes/ui/title_screen.tscn";

    public override void _Ready()
    {
        Instance = this;
        SetProcess(true);
        InitializeUI();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public override void _Process(double _)
    {
        UpdatePhaseDisplay();
        UpdatePlacementPreview();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                if (CardSystem.Instance?.HasSelection == true)
                {
                    CardSystem.Instance.ClearSelection();
                    GetViewport().SetInputAsHandled();
                }
                return;
            }

            if (mouseButton.ButtonIndex != MouseButton.Left)
                return;

            if (_shopOpen || GameManager.Instance?.CurrentState == GameManager.GameState.GameOver)
                return;

            if (CardSystem.Instance?.HasSelection != true)
                return;

            TryPlaceAtMouse(mouseButton.GlobalPosition);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo
            && keyEvent.Keycode == Key.Escape)
        {
            if (_shopOpen)
            {
                CloseShop();
                GetViewport().SetInputAsHandled();
                return;
            }

            if (CardSystem.Instance?.HasSelection == true)
            {
                CardSystem.Instance.ClearSelection();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void InitializeUI()
    {
        var uiRoot = GetParent()?.GetNodeOrNull<CanvasLayer>("UI");
        if (uiRoot == null) return;

        _playerHealthBar = uiRoot.GetNode<ProgressBar>("PlayerHealthBar");
        _enemyHealthBar = uiRoot.GetNode<ProgressBar>("EnemyHealthBar");
        _gameOverOverlay = uiRoot.GetNode<ColorRect>("GameOverOverlay");
        _gameOverPanel = uiRoot.GetNode<Panel>("GameOverPanel");
        _gameOverLabel = uiRoot.GetNode<Label>("GameOverPanel/GameOverLabel");
        _backToTitleButton = uiRoot.GetNode<Button>("GameOverPanel/BackToTitleButton");
        _phaseLabel = uiRoot.GetNode<Label>("PhasePanel/PhaseLabel");
        _phaseTimerLabel = uiRoot.GetNode<Label>("PhasePanel/PhaseTimerLabel");
        _skipPhaseButton = uiRoot.GetNode<Button>("PhasePanel/SkipPhaseButton");
        _goldLabel = uiRoot.GetNode<Label>("GoldLabel");
        _shopButton = uiRoot.GetNode<Button>("ShopButton");
        _shopOverlay = uiRoot.GetNode<ColorRect>("ShopOverlay");
        _shopPanel = uiRoot.GetNode<Panel>("ShopPanel");
        _shopGoldLabel = uiRoot.GetNode<Label>("ShopPanel/ShopGoldLabel");
        _shopCloseButton = uiRoot.GetNode<Button>("ShopPanel/ShopCloseButton");
        _shopRefreshButton = uiRoot.GetNode<Button>("ShopPanel/ShopRefreshButton");
        _placementHintLabel = uiRoot.GetNode<Label>("PlacementHintLabel");

        for (int i = 0; i < ShopSystem.OfferCount; i++)
        {
            _shopOfferLabels[i] = uiRoot.GetNode<Label>($"ShopPanel/OfferSlot{i + 1}/OfferLabel");
            _shopBuyButtons[i] = uiRoot.GetNode<Button>($"ShopPanel/OfferSlot{i + 1}/BuyButton");
            int slotIndex = i;
            _shopBuyButtons[i].Pressed += () => OnBuyButtonPressed(slotIndex);
        }

        for (int i = 0; i < CardSystem.MaxHandSize; i++)
        {
            _handButtons[i] = uiRoot.GetNode<Button>($"HandPanel/HandSlot{i + 1}");
            int handIndex = i;
            _handButtons[i].Pressed += () => OnHandButtonPressed(handIndex);
        }

        _backToTitleButton.Pressed += GoToTitle;
        _skipPhaseButton.Pressed += OnSkipPhasePressed;
        _shopButton.Pressed += OnShopButtonPressed;
        _shopCloseButton.Pressed += CloseShop;
        _shopRefreshButton.Pressed += OnShopRefreshPressed;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerHealthChanged += UpdatePlayerHealth;
            GameManager.Instance.EnemyHealthChanged += UpdateEnemyHealth;
            GameManager.Instance.GameStateChanged += OnGameStateChanged;
            GameManager.Instance.PhaseChanged += OnPhaseChanged;
            UpdatePhaseDisplay();
        }

        if (ShopSystem.Instance != null)
        {
            ShopSystem.Instance.GoldChanged += UpdateGoldDisplay;
            ShopSystem.Instance.ShopAvailabilityChanged += UpdateShopButtonAvailability;
            ShopSystem.Instance.ShopOpenRequested += OpenShop;
            ShopSystem.Instance.ShopOffersChanged += RefreshShopOffers;
            UpdateGoldDisplay(ShopSystem.Instance.Gold);
            UpdateShopButtonAvailability(ShopSystem.Instance.IsShopAvailable);
            RefreshShopOffers();
        }

        if (CardSystem.Instance != null)
        {
            CardSystem.Instance.HandChanged += RefreshHandDisplay;
            CardSystem.Instance.SelectionChanged += OnHandSelectionChanged;
            RefreshHandDisplay();
        }
    }

    private void OnSkipPhasePressed()
    {
        GameManager.Instance?.AdvancePhase();
    }

    private void OnShopButtonPressed()
    {
        ShopSystem.Instance?.RequestOpenShop();
    }

    private void OnShopRefreshPressed()
    {
        ShopSystem.Instance?.RefreshOffers();
    }

    private void OnBuyButtonPressed(int slotIndex)
    {
        ShopSystem.Instance?.TryPurchase(slotIndex);
    }

    private void OnHandButtonPressed(int handIndex)
    {
        if (_shopOpen)
            return;

        CardSystem.Instance?.SelectCard(handIndex);
    }

    private void OnPhaseChanged(GameManager.GamePhase phase)
    {
        UpdatePhaseDisplay();
    }

    private void UpdatePhaseDisplay()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (_phaseLabel != null)
            _phaseLabel.Text = gm.IsDay ? "白天" : "夜晚";

        if (_phaseTimerLabel != null)
            _phaseTimerLabel.Text = FormatTime(gm.PhaseTimeRemaining);
    }

    private static string FormatTime(float seconds)
    {
        int total = Math.Max(0, (int)Math.Ceiling(seconds));
        return $"{total / 60}:{total % 60:D2}";
    }

    public void UpdatePlayerHealth(int health)
    {
        _playerHealthBar?.SetValue(health);
    }

    public void UpdateEnemyHealth(int health)
    {
        _enemyHealthBar?.SetValue(health);
    }

    public void UpdateGoldDisplay(int gold)
    {
        if (_goldLabel != null)
            _goldLabel.Text = $"金币：{gold}";

        if (_shopGoldLabel != null)
            _shopGoldLabel.Text = $"当前金币：{gold}";

        RefreshShopOffers();
    }

    private void UpdateShopButtonAvailability(bool available)
    {
        if (_shopButton == null) return;

        _shopButton.Disabled = !available || _shopOpen;
        _shopButton.Text = "商店";
    }

    private void OpenShop()
    {
        if (_shopOpen || ShopSystem.Instance?.IsShopAvailable != true)
            return;

        _shopOpen = true;
        CardSystem.Instance?.ClearSelection();
        _shopOverlay.Visible = true;
        _shopPanel.Visible = true;
        UpdateShopButtonAvailability(ShopSystem.Instance.IsShopAvailable);
        RefreshShopOffers();
        RefreshHandDisplay();
    }

    private void CloseShop()
    {
        if (!_shopOpen)
            return;

        _shopOpen = false;
        _shopOverlay.Visible = false;
        _shopPanel.Visible = false;

        if (ShopSystem.Instance != null)
            UpdateShopButtonAvailability(ShopSystem.Instance.IsShopAvailable);

        RefreshHandDisplay();
    }

    private void RefreshShopOffers()
    {
        if (ShopSystem.Instance == null)
            return;

        for (int i = 0; i < ShopSystem.OfferCount; i++)
        {
            CardData offer = ShopSystem.Instance.GetOffer(i);
            if (_shopOfferLabels[i] != null)
            {
                _shopOfferLabels[i].Text = offer == null
                    ? "暂无商品"
                    : $"{offer.Name}  费用：{offer.Cost}";
            }

            if (_shopBuyButtons[i] != null)
            {
                bool canBuy = offer != null && ShopSystem.Instance.CanAfford(offer.Cost);
                _shopBuyButtons[i].Disabled = offer == null || !canBuy;
                _shopBuyButtons[i].Text = canBuy ? "购买" : "金币不足";
            }
        }
    }

    private void RefreshHandDisplay()
    {
        if (CardSystem.Instance == null)
            return;

        for (int i = 0; i < CardSystem.MaxHandSize; i++)
        {
            Button button = _handButtons[i];
            if (button == null)
                continue;

            if (i < CardSystem.Instance.Hand.Count)
            {
                CardData card = CardSystem.Instance.Hand[i];
                button.Visible = true;
                button.Text = card.Name;
                button.Disabled = _shopOpen;
            }
            else
            {
                button.Visible = true;
                button.Text = "空";
                button.Disabled = true;
            }
        }
    }

    private void OnHandSelectionChanged(int selectedIndex)
    {
        for (int i = 0; i < CardSystem.MaxHandSize; i++)
        {
            Button button = _handButtons[i];
            if (button == null)
                continue;

            button.Modulate = i == selectedIndex
                ? new Color(1f, 1f, 0.75f)
                : Colors.White;
        }

        if (_placementHintLabel != null)
            _placementHintLabel.Visible = selectedIndex >= 0 && !_shopOpen;
    }

    private void UpdatePlacementPreview()
    {
        Castle playerCastle = GameManager.Instance?.PlayerCastle;
        if (playerCastle == null)
            return;

        if (_shopOpen || CardSystem.Instance?.HasSelection != true)
        {
            playerCastle.ClearPlacementPreview();
            return;
        }

        Vector2 mouseGlobal = GetViewport().GetMousePosition();
        if (!playerCastle.TryGetGridFromGlobalPoint(mouseGlobal, out int gridX, out int gridY))
        {
            playerCastle.ClearPlacementPreview();
            return;
        }

        bool valid = playerCastle.IsCellPassable(gridX, gridY);
        playerCastle.SetPlacementPreview(true, gridX, gridY, valid);
    }

    private void TryPlaceAtMouse(Vector2 globalPosition)
    {
        Castle playerCastle = GameManager.Instance?.PlayerCastle;
        if (playerCastle == null || CardSystem.Instance == null)
            return;

        if (!playerCastle.TryGetGridFromGlobalPoint(globalPosition, out int gridX, out int gridY))
            return;

        CardSystem.Instance.TryPlaceSelected(playerCastle, gridX, gridY);
    }

    public void OnGameStateChanged(GameManager.GameState state)
    {
        bool show = state == GameManager.GameState.GameOver;

        if (_gameOverOverlay != null)
            _gameOverOverlay.Visible = show;

        if (_gameOverPanel != null)
            _gameOverPanel.Visible = show;

        if (_skipPhaseButton != null)
            _skipPhaseButton.Visible = !show;

        if (show)
        {
            CloseShop();
            CardSystem.Instance?.ClearSelection();
        }

        if (show && _shopButton != null)
            _shopButton.Disabled = true;

        if (show && _gameOverLabel != null && GameManager.Instance != null)
            _gameOverLabel.Text = GameManager.Instance.PlayerHealth > 0 ? "胜利！" : "失败！";
    }

    private void GoToTitle()
    {
        CardSystem.Instance?.ResetHand();
        GetTree().ChangeSceneToFile(TitleScene);
    }
}
