using Godot;

public sealed class HandUiController
{
    private readonly Node _owner;
    private readonly Button[] _handButtons = new Button[CardSystem.MaxHandSize];
    private readonly Label _placementHintLabel;

    private bool _inputBlocked;

    public HandUiController(Node owner, CanvasLayer uiRoot)
    {
        _owner = owner;
        _placementHintLabel = uiRoot.GetNode<Label>("PlacementHintLabel");

        for (int i = 0; i < CardSystem.MaxHandSize; i++)
        {
            _handButtons[i] = uiRoot.GetNode<Button>($"HandPanel/HandSlot{i + 1}");
            int handIndex = i;
            _handButtons[i].Pressed += () => OnHandButtonPressed(handIndex);
        }

        if (CardSystem.Instance != null)
        {
            CardSystem.Instance.HandChanged += RefreshDisplay;
            CardSystem.Instance.SelectionChanged += OnSelectionChanged;
            RefreshDisplay();
        }
    }

    public void Dispose()
    {
        if (CardSystem.Instance != null)
        {
            CardSystem.Instance.HandChanged -= RefreshDisplay;
            CardSystem.Instance.SelectionChanged -= OnSelectionChanged;
        }
    }

    public void SetInputBlocked(bool blocked)
    {
        _inputBlocked = blocked;
        RefreshDisplay();

        if (_inputBlocked)
        {
            CardSystem.Instance?.ClearSelection();
            GameManager.Instance.PlayerCastle?.ClearPlacementPreview();
        }
    }

    public void Process()
    {
        UpdatePlacementPreview();
    }

    public bool HandleInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
            return HandleMouseInput(mouseButton);

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo
            && keyEvent.Keycode == Key.Escape)
            return ClearSelection();

        return false;
    }

    private bool HandleMouseInput(InputEventMouseButton mouseButton)
    {
        if (mouseButton.ButtonIndex == MouseButton.Right)
            return ClearSelection();

        if (mouseButton.ButtonIndex != MouseButton.Left)
            return false;

        if (_inputBlocked || GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
            return false;

        if (CardSystem.Instance?.HasSelection != true)
            return false;

        TryPlaceAtMouse(mouseButton.GlobalPosition);
        return true;
    }

    private bool ClearSelection()
    {
        if (CardSystem.Instance?.HasSelection != true)
            return false;

        CardSystem.Instance.ClearSelection();
        return true;
    }

    private void OnHandButtonPressed(int handIndex)
    {
        if (_inputBlocked)
            return;

        CardSystem.Instance?.SelectCard(handIndex);
    }

    private void RefreshDisplay()
    {
        if (CardSystem.Instance == null)
            return;

        for (int i = 0; i < CardSystem.MaxHandSize; i++)
        {
            Button button = _handButtons[i];

            if (i < CardSystem.Instance.Hand.Count)
            {
                CardData card = CardSystem.Instance.Hand[i];
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
    }

    private void OnSelectionChanged(int selectedIndex)
    {
        for (int i = 0; i < CardSystem.MaxHandSize; i++)
        {
            _handButtons[i].Modulate = i == selectedIndex
                ? new Color(1f, 1f, 0.75f)
                : Colors.White;
        }

        _placementHintLabel.Visible = selectedIndex >= 0 && !_inputBlocked;
    }

    private void UpdatePlacementPreview()
    {
        Castle playerCastle = GameManager.Instance.PlayerCastle;
        if (playerCastle == null)
            return;

        if (_inputBlocked || CardSystem.Instance?.HasSelection != true)
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

        bool valid = playerCastle.IsCellPassable(gridX, gridY);
        playerCastle.SetPlacementPreview(true, gridX, gridY, valid);
    }

    private static void TryPlaceAtMouse(Vector2 globalPosition)
    {
        Castle playerCastle = GameManager.Instance.PlayerCastle;
        if (playerCastle == null || CardSystem.Instance == null)
            return;

        if (!playerCastle.TryGetGridFromGlobalPoint(globalPosition, out int gridX, out int gridY))
            return;

        CardSystem.Instance.TryPlaceSelected(playerCastle, gridX, gridY);
    }
}
