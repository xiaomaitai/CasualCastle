using Godot;

public partial class CameraController : Camera2D
{
	private const float DragThreshold = 8f;
	private const float EdgeScrollMargin = 48f;
	private const float EdgeScrollSpeed = 700f;
	private const float WheelScrollStep = 300f;

	[Export]
	public Rect2 Boundary = BattlefieldMetrics.WorldRect;

	private Vector2 _dragStartScreenPosition;
	private bool _dragCandidate;
	private bool _isDragging;

	public override void _Ready()
	{
		AnchorMode = AnchorModeEnum.DragCenter;
	}

	public override void _Process(double delta)
	{
		if (_isDragging || _dragCandidate)
			return;
		if (!GetWindow().HasFocus())
			return;

		Vector2 mousePosition = GetViewport().GetMousePosition();
		if (!GetViewportRect().HasPoint(mousePosition))
			return;
		if (IsBlockingUiHovered())
			return;

		Vector2 direction = ComputeEdgeScrollDirection(mousePosition, GetViewportRect().Size);
		if (direction == Vector2.Zero)
			return;

		Position += direction * EdgeScrollSpeed * (float)delta / Zoom;
		ClampToBoundary();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
			HandleMouseButton(mouseButton);

		if (@event is InputEventMouseMotion mouseMotion && _dragCandidate)
			HandleDrag(mouseMotion);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton wheelEvent && wheelEvent.Pressed)
		{
			if (wheelEvent.ButtonIndex == MouseButton.WheelUp)
				Position += Vector2.Left * WheelScrollStep;
			if (wheelEvent.ButtonIndex == MouseButton.WheelDown)
				Position += Vector2.Right * WheelScrollStep;
			ClampToBoundary();
		}

		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.Home)
			ResetView();
	}

	private void HandleMouseButton(InputEventMouseButton mouseButton)
	{
		if (mouseButton.ButtonIndex != MouseButton.Middle)
			return;

		if (mouseButton.Pressed)
		{
			_dragCandidate = true;
			_isDragging = false;
			_dragStartScreenPosition = GetViewport().GetMousePosition();
		}
		else
		{
			_dragCandidate = false;
			_isDragging = false;
		}
	}

	private void HandleDrag(InputEventMouseMotion mouseMotion)
	{
		Vector2 currentScreenPosition = GetViewport().GetMousePosition();
		if (!_isDragging)
		{
			if (currentScreenPosition.DistanceTo(_dragStartScreenPosition) < DragThreshold)
				return;
			_isDragging = true;
		}

		Vector2 screenDelta = currentScreenPosition - _dragStartScreenPosition;
		_dragStartScreenPosition = currentScreenPosition;
		Position -= new Vector2(screenDelta.X / Zoom.X * DisplaySettingsManager.Instance.CameraDragSpeed, 0f);
		ClampToBoundary();
	}

	private bool IsBlockingUiHovered()
	{
		Control hoveredControl = GetViewport().GuiGetHoveredControl();
		return hoveredControl != null && hoveredControl.MouseFilter == Control.MouseFilterEnum.Stop;
	}

	private Vector2 ComputeEdgeScrollDirection(Vector2 mousePosition, Vector2 viewportSize)
	{
		Vector2 direction = Vector2.Zero;
		if (mousePosition.X < EdgeScrollMargin)
			direction.X = -1f;
		else if (mousePosition.X > viewportSize.X - EdgeScrollMargin)
			direction.X = 1f;
		return direction;
	}

	private void ResetView()
	{
		Position = new Vector2(Boundary.GetCenter().X, Position.Y);
	}

	private void ClampToBoundary()
	{
		Vector2 halfViewportSize = GetViewportRect().Size / 2.0f / Zoom;
		float left = Boundary.Position.X + halfViewportSize.X;
		float right = Boundary.End.X - halfViewportSize.X;
		Position = new Vector2(Mathf.Clamp(Position.X, left, right), Position.Y);
	}
}
