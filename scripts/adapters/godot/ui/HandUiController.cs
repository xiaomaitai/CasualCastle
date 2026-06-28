using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;

public sealed class HandUiController
{
    private const float DragThreshold = 8f;

    private readonly Node _owner;
    private readonly Button[] _handButtons = new Button[MaxHandSize];
    private readonly Control.GuiInputEventHandler[] _handGuiInputHandlers = new Control.GuiInputEventHandler[MaxHandSize];
    private readonly Label _placementHintLabel;

    private readonly HandService _handService;
    private readonly ShopService _shopService;
    private readonly GameManager _gameManager;
    private readonly BuildingSystem _buildingSystem;

    private bool _inputBlocked;
    private bool _dragging;
    private int _dragHandIndex = -1;
    private int _pendingHandIndex = -1;
    private Vector2 _dragStartPosition;
    private int _selectedIndex = -1;
    private const int MaxHandSize = 7;

    public bool IsDragging => _dragging;
    public bool IsPlacementActive => _dragging || _handService.HasSelection;

    public HandUiController(Node owner, CanvasLayer uiRoot, HandService handService)
    {
        _owner = owner;
        _handService = handService;
        _shopService = AdapterRegistry.Resolve<ShopService>();
        _gameManager = AdapterRegistry.Resolve<GameManager>();
        _buildingSystem = AdapterRegistry.Resolve<BuildingSystem>();
        _placementHintLabel = uiRoot.GetNode<Label>("PlacementHintLabel");

        for (int i = 0; i < MaxHandSize; i++)
        {
            _handButtons[i] = uiRoot.GetNode<Button>($"HandPanel/HandSlot{i + 1}");
            int handIndex = i;
            _handButtons[i].Pressed += () => OnHandButtonPressed(handIndex);
            _handGuiInputHandlers[i] = inputEvent => OnHandGuiInput(handIndex, inputEvent);
            _handButtons[i].GuiInput += _handGuiInputHandlers[i];
        }

        _handService.HandChanged += RefreshDisplay;
        _handService.SelectionChanged += OnSelectionChanged;
        RefreshDisplay();
    }

    public void Dispose()
    {
        for (int i = 0; i < MaxHandSize; i++)
            _handButtons[i].GuiInput -= _handGuiInputHandlers[i];

        _handService.HandChanged -= RefreshDisplay;
        _handService.SelectionChanged -= OnSelectionChanged;
    }

    public void SetInputBlocked(bool blocked)
    {
        _inputBlocked = blocked;
        RefreshDisplay();

        if (_inputBlocked)
        {
            CancelDrag();
            _handService.ClearSelection();
            _gameManager.PlayerCastle.ClearPlacementPreview();
        }
    }

    public void Process()
    {
        UpdatePendingDrag();
        UpdatePlacementPreview();
    }

    public bool TryHandleEscape()
    {
        if (CancelDrag())
            return true;

        return ClearSelection();
    }

    public bool HandleInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && !mouseButton.Pressed)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (_dragging)
                {
                    TryCompleteDrag(mouseButton.GlobalPosition);
                    CancelDrag();
                    return true;
                }

                if (_pendingHandIndex >= 0)
                {
                    _pendingHandIndex = -1;
                    return false;
                }
            }

            if (mouseButton.ButtonIndex == MouseButton.Right && _dragging)
            {
                CancelDrag();
                return true;
            }
        }

        if (@event is InputEventMouseButton pressedMouseButton && pressedMouseButton.Pressed)
            return HandleMouseInput(pressedMouseButton);

        return false;
    }

    public bool CancelDrag()
    {
        if (!_dragging && _pendingHandIndex < 0)
            return false;

        _dragging = false;
        _dragHandIndex = -1;
        _pendingHandIndex = -1;
        _gameManager.PlayerCastle.ClearPlacementPreview();
        RefreshHighlight();
        UpdatePlacementHint();
        return true;
    }

    private bool HandleMouseInput(InputEventMouseButton mouseButton)
    {
        if (mouseButton.ButtonIndex == MouseButton.Right)
            return ClearSelection();

        if (mouseButton.ButtonIndex != MouseButton.Left)
            return false;

        if (_inputBlocked || _dragging || _gameManager.CurrentState == GameManager.GameState.GameOver)
            return false;

        if (!_handService.HasSelection)
            return false;

        TryPlaceAtMouse(mouseButton.GlobalPosition);
        return true;
    }

    private bool ClearSelection()
    {
        if (!_handService.HasSelection)
            return false;

        _handService.ClearSelection();
        return true;
    }

    private void OnHandButtonPressed(int handIndex)
    {
        if (_inputBlocked)
            return;

        _handService.SelectCard(handIndex);
    }

    private void OnHandGuiInput(int handIndex, InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseButton
            || !mouseButton.Pressed
            || mouseButton.ButtonIndex != MouseButton.Left)
            return;

        if (_inputBlocked || _gameManager.CurrentState == GameManager.GameState.GameOver)
            return;

        if (handIndex >= _handService.Hand.Count)
            return;

        _pendingHandIndex = handIndex;
        _dragStartPosition = mouseButton.GlobalPosition;
    }

    private void UpdatePendingDrag()
    {
        if (_pendingHandIndex < 0 || _dragging)
            return;

        if (!Input.IsMouseButtonPressed(MouseButton.Left))
            return;

        Vector2 mouseGlobal = _owner.GetViewport().GetMousePosition();
        if (mouseGlobal.DistanceTo(_dragStartPosition) < DragThreshold)
            return;

        _dragging = true;
        _dragHandIndex = _pendingHandIndex;
        _pendingHandIndex = -1;
        RefreshHighlight();
        UpdatePlacementHint();
    }

    private void TryCompleteDrag(Vector2 globalPosition)
    {
        Castle playerCastle = _gameManager.PlayerCastle;
        if (playerCastle == null || _dragHandIndex < 0)
            return;

        if (!playerCastle.TryGetGridFromGlobalPoint(globalPosition, out int gridX, out int gridY))
            return;

        _handService.TryPlaceAtIndex(_dragHandIndex, gridX, gridY);
    }

    private void RefreshDisplay()
    {
        for (int i = 0; i < MaxHandSize; i++)
        {
            Button button = _handButtons[i];

            if (i < _handService.Hand.Count)
            {
                CardData card = _handService.Hand[i];
                button.Visible = true;
                button.Text = card.Name;
                button.Disabled = _inputBlocked;
            }
            else
            {
                button.Visible = true;
                button.Text = "空";
                button.Disabled = true;
            }
        }

        RefreshHighlight();
    }

    private void OnSelectionChanged(int selectedIndex)
    {
        _selectedIndex = selectedIndex;
        RefreshHighlight();
        UpdatePlacementHint();
    }

    private void UpdatePlacementHint()
    {
        _placementHintLabel.Visible = (_selectedIndex >= 0 || _dragging) && !_inputBlocked;
    }

    private void RefreshHighlight()
    {
        for (int i = 0; i < MaxHandSize; i++)
        {
            bool highlighted = i == _selectedIndex || (_dragging && i == _dragHandIndex);
            _handButtons[i].Modulate = highlighted
                ? new Color(1f, 1f, 0.75f)
                : Colors.White;
        }
    }

    private void UpdatePlacementPreview()
    {
        Castle playerCastle = _gameManager.PlayerCastle;
        if (playerCastle == null)
            return;

        bool showPreview = !_inputBlocked
            && (_dragging || _handService.HasSelection);
        if (!showPreview)
        {
            playerCastle.ClearPlacementPreview();
            return;
        }

        Vector2 mouseGlobal = _owner.GetViewport().GetMousePosition();
        if (!playerCastle.TryGetGridFromGlobalPoint(mouseGlobal, out int gridX, out int gridY))
        {
            playerCastle.ClearPlacementPreview();
            return;
        }

        string buildingType = GetPreviewBuildingType();
        bool valid = _buildingSystem.CanPlace(playerCastle, buildingType, gridX, gridY);
        playerCastle.SetPlacementPreview(true, gridX, gridY, valid, buildingType);
    }

    private string GetPreviewBuildingType()
    {
        if (_dragging && _dragHandIndex >= 0 && _dragHandIndex < _handService.Hand.Count)
            return _handService.Hand[_dragHandIndex].BuildingType;

        if (_handService.HasSelection)
            return _handService.SelectedCard.BuildingType;

        return "Barracks";
    }

    private void TryPlaceAtMouse(Vector2 globalPosition)
    {
        Castle playerCastle = _gameManager.PlayerCastle;
        if (playerCastle == null)
            return;

        if (!playerCastle.TryGetGridFromGlobalPoint(globalPosition, out int gridX, out int gridY))
            return;

        _handService.TryPlaceSelected(gridX, gridY);
    }
}
