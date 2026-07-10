using Godot;

public partial class CameraController : Camera2D
{
	[Export]
	public float MinZoom = 0.5f;

	[Export]
	public float MaxZoom = 2.0f;

	[Export]
	public float ZoomStep = 0.1f;

	[Export]
	public Rect2 Boundary = new Rect2(0, 0, 1920, 1080);

	private Vector2 _dragStartPosition;
	private bool _isDragging;

	public override void _Ready()
	{
		AnchorMode = AnchorModeEnum.DragCenter;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
			HandleMouseButton(mouseButton);

		if (@event is InputEventMouseMotion mouseMotion && _isDragging)
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
		if (mouseButton.ButtonIndex == MouseButton.Middle)
		{
			if (mouseButton.Pressed)
			{
				_isDragging = true;
				_dragStartPosition = GetGlobalMousePosition();
			}
			else
			{
				_isDragging = false;
			}
		}
	}

	private void HandleDrag(InputEventMouseMotion mouseMotion)
	{
		Vector2 mouseDelta = GetGlobalMousePosition() - _dragStartPosition;
		_dragStartPosition = GetGlobalMousePosition();
		Position -= mouseDelta / Zoom;
		ClampToBoundary();
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
