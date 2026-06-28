using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;

public sealed class HandUiController
{
    private const float DragThreshold = 8f;

    private readonly Node _owner;
    private readonly Button[] _handButtons = new Button[CardSystem.MaxHandSize];
    private readonly Control.GuiInputEventHandler[] _handGuiInputHandlers = new Control.GuiInputEventHandler[CardSystem.MaxHandSize];
    private readonly Label _placementHintLabel;

    private bool _inputBlocked;
    private bool _dragging;
    private int _dragHandIndex = -1;
    private int _pendingHandIndex = -1;
    private Vector2 _dragStartPosition;
    private int _selectedIndex = -1;

    public bool IsDragging => _dragging;
    public bool IsPlacementActive => _dragging || AdapterRegistry.Resolve<CardSystem>()?.HasSelection == true;

    public HandUiController(Node owner, CanvasLayer uiRoot)
    {
        _owner = owner;
        _placementHintLabel = uiRoot.GetNode<Label>("PlacementHintLabel");

        for (int i = 0; i < CardSystem.MaxHandSize; i++)
        {
            _handButtons[i] = uiRoot.GetNode<Button>($"HandPanel/HandSlot{i + 1}");
            int handIndex = i;
            _handButtons[i].Pressed += () => OnHandButtonPressed(handIndex);
            _handGuiInputHandlers[i] = inputEvent => OnHandGuiInput(handIndex, inputEvent);
            _handButtons[i].GuiInput += _handGuiInputHandlers[i];
        }

        if (AdapterRegistry.Resolve<CardSystem>() != null)
        {
            AdapterRegistry.Resolve<CardSystem>().HandChanged += RefreshDisplay;
            AdapterRegistry.Resolve<CardSystem>().SelectionChanged += OnSelectionChanged;
            RefreshDisplay();
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < CardSystem.MaxHandSize; i++)
            _handButtons[i].GuiInput -= _handGuiInputHandlers[i];

        if (AdapterRegistry.Resolve<CardSystem>() != null)
        {
            AdapterRegistry.Resolve<CardSystem>().HandChanged -= RefreshDisplay;
            AdapterRegistry.Resolve<CardSystem>().SelectionChanged -= OnSelectionChanged;
        }
    }

    public void SetInputBlocked(bool blocked)
    {
        _inputBlocked = blocked;
        RefreshDisplay();

        if (_inputBlocked)
        {
            CancelDrag();
            AdapterRegistry.Resolve<CardSystem>()?.ClearSelection();
            AdapterRegistry.Resolve<GameManager>().PlayerCastle?.ClearPlacementPreview();
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
        AdapterRegistry.Resolve<GameManager>().PlayerCastle?.ClearPlacementPreview();
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

        if (_inputBlocked || _dragging || AdapterRegistry.Resolve<GameManager>().CurrentState == GameManager.GameState.GameOver)
            return false;

        if (AdapterRegistry.Resolve<CardSystem>()?.HasSelection != true)
            return false;

        TryPlaceAtMouse(mouseButton.GlobalPosition);
        return true;
    }

    private bool ClearSelection()
    {
        if (AdapterRegistry.Resolve<CardSystem>()?.HasSelection != true)
            return false;

        AdapterRegistry.Resolve<CardSystem>().ClearSelection();
        return true;
    }

    private void OnHandButtonPressed(int handIndex)
    {
        if (_inputBlocked)
            return;

        AdapterRegistry.Resolve<CardSystem>()?.SelectCard(handIndex);
    }

    private void OnHandGuiInput(int handIndex, InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseButton
            || !mouseButton.Pressed
            || mouseButton.ButtonIndex != MouseButton.Left)
            return;

        if (_inputBlocked || AdapterRegistry.Resolve<GameManager>().CurrentState == GameManager.GameState.GameOver)
            return;

        if (AdapterRegistry.Resolve<CardSystem>() == null || handIndex >= AdapterRegistry.Resolve<CardSystem>().Hand.Count)
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
        Castle playerCastle = AdapterRegistry.Resolve<GameManager>().PlayerCastle;
        if (playerCastle == null || AdapterRegistry.Resolve<CardSystem>() == null || _dragHandIndex < 0)
            return;

        if (!playerCastle.TryGetGridFromGlobalPoint(globalPosition, out int gridX, out int gridY))
            return;

        AdapterRegistry.Resolve<CardSystem>().TryPlaceAtIndex(_dragHandIndex, playerCastle, gridX, gridY);
    }

    private void RefreshDisplay()
    {
        if (AdapterRegistry.Resolve<CardSystem>() == null)
            return;

        for (int i = 0; i < CardSystem.MaxHandSize; i++)
        {
            Button button = _handButtons[i];

            if (i < AdapterRegistry.Resolve<CardSystem>().Hand.Count)
            {
                CardData card = AdapterRegistry.Resolve<CardSystem>().Hand[i];
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
        for (int i = 0; i < CardSystem.MaxHandSize; i++)
        {
            bool highlighted = i == _selectedIndex || (_dragging && i == _dragHandIndex);
            _handButtons[i].Modulate = highlighted
                ? new Color(1f, 1f, 0.75f)
                : Colors.White;
        }
    }

    private void UpdatePlacementPreview()
    {
        Castle playerCastle = AdapterRegistry.Resolve<GameManager>().PlayerCastle;
        if (playerCastle == null)
            return;

        bool showPreview = !_inputBlocked
            && (_dragging || AdapterRegistry.Resolve<CardSystem>()?.HasSelection == true);
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
        bool valid = AdapterRegistry.Resolve<BuildingSystem>()?.CanPlace(playerCastle, buildingType, gridX, gridY) == true;
        playerCastle.SetPlacementPreview(true, gridX, gridY, valid, buildingType);
    }

    private string GetPreviewBuildingType()
    {
        if (AdapterRegistry.Resolve<CardSystem>() == null)
            return "Barracks";

        if (_dragging && _dragHandIndex >= 0 && _dragHandIndex < AdapterRegistry.Resolve<CardSystem>().Hand.Count)
            return AdapterRegistry.Resolve<CardSystem>().Hand[_dragHandIndex].BuildingType;

        if (AdapterRegistry.Resolve<CardSystem>().HasSelection)
            return AdapterRegistry.Resolve<CardSystem>().SelectedCard.BuildingType;

        return "Barracks";
    }

    private static void TryPlaceAtMouse(Vector2 globalPosition)
    {
        Castle playerCastle = AdapterRegistry.Resolve<GameManager>().PlayerCastle;
        if (playerCastle == null || AdapterRegistry.Resolve<CardSystem>() == null)
            return;

        if (!playerCastle.TryGetGridFromGlobalPoint(globalPosition, out int gridX, out int gridY))
            return;

        AdapterRegistry.Resolve<CardSystem>().TryPlaceSelected(playerCastle, gridX, gridY);
    }
}
