using Godot;

public partial class CameraController : Camera2D
{
	private const float DragThreshold = 8f;
	private const float EdgeScrollMargin = 48f;
	private const float EdgeScrollSpeed = 700f;

	[Export]
	public float MinZoom = 0.5f;

	[Export]
	public float MaxZoom = 2.0f;

	[Export]
	public float ZoomStep = 0.1f;

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
		if (@event is InputEventMouseButton wheelEvent)
		{
			if (wheelEvent.ButtonIndex == MouseButton.WheelUp && wheelEvent.Pressed)
				ZoomIn();
			if (wheelEvent.ButtonIndex == MouseButton.WheelDown && wheelEvent.Pressed)
				ZoomOut();
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
		Position -= screenDelta / Zoom * DisplaySettingsManager.Instance.CameraDragSpeed;
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
		if (mousePosition.Y < EdgeScrollMargin)
			direction.Y = -1f;
		else if (mousePosition.Y > viewportSize.Y - EdgeScrollMargin)
			direction.Y = 1f;
		return direction;
	}

	private void ZoomIn()
	{
		Vector2 newZoom = Zoom + new Vector2(ZoomStep, ZoomStep);
		if (newZoom.X > MaxZoom)
			newZoom = new Vector2(MaxZoom, MaxZoom);
		Zoom = newZoom;
		ClampToBoundary();
	}

	private void ZoomOut()
	{
		Vector2 newZoom = Zoom - new Vector2(ZoomStep, ZoomStep);
		if (newZoom.X < MinZoom)
			newZoom = new Vector2(MinZoom, MinZoom);
		Zoom = newZoom;
		ClampToBoundary();
	}

	private void ResetView()
	{
		Position = Boundary.GetCenter();
		Zoom = Vector2.One;
	}

	private void ClampToBoundary()
	{
		Vector2 halfViewportSize = GetViewportRect().Size / 2.0f / Zoom;
		float left = Boundary.Position.X + halfViewportSize.X;
		float right = Boundary.End.X - halfViewportSize.X;
		float top = Boundary.Position.Y + halfViewportSize.Y;
		float bottom = Boundary.End.Y - halfViewportSize.Y;

		Vector2 clampedPosition = Position;
		clampedPosition.X = Mathf.Clamp(clampedPosition.X, left, right);
		clampedPosition.Y = Mathf.Clamp(clampedPosition.Y, top, bottom);
		Position = clampedPosition;
	}
}
