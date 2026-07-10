using Godot;

public static class BuildingIcons
{
	private const int Width = 48;
	private const int Height = 36;

	private static Texture2D _pause;
	private static Texture2D _repair;
	private static Texture2D _repairBlocked;
	private static Texture2D _combineProhibit;

	public static Vector2I IconSize => new(Width, Height);
	public static Vector2 CursorHotspot => new(Width * 0.5f, Height * 0.5f);

	public static Texture2D Pause => _pause ??= CreatePauseIcon();
	public static Texture2D Repair => _repair ??= CreateRepairIcon();
	public static Texture2D RepairBlocked => _repairBlocked ??= CreateRepairBlockedIcon();
	public static Texture2D CombineProhibit => _combineProhibit ??= CreateCombineProhibitIcon();

	private static Texture2D CreatePauseIcon()
	{
		Image image = CreateCanvas();
		Color color = new(0.9f, 0.94f, 1f, 1f);
		FillRoundedRect(image, 13, 7, 6, 22, 2, color);
		FillRoundedRect(image, 29, 7, 6, 22, 2, color);
		return ToTexture(image);
	}

	private static Texture2D CreateRepairIcon()
	{
		Image image = CreateCanvas();
		DrawRepair(image);
		return ToTexture(image);
	}

	private static Texture2D CreateRepairBlockedIcon()
	{
		Image image = CreateCanvas();
		DrawRepair(image);
		DrawProhibitOverlay(image);
		return ToTexture(image);
	}

	private static Texture2D CreateCombineProhibitIcon()
	{
		Image image = CreateCanvas();
		DrawProhibitOverlay(image);
		return ToTexture(image);
	}

	private static void DrawRepair(Image image)
	{
		Color head = new(0.95f, 0.78f, 0.32f, 1f);
		Color handle = new(0.7f, 0.48f, 0.24f, 1f);

		FillRoundedRect(image, 7, 5, 20, 9, 2, head);
		for (int i = 0; i < 16; i++)
		{
			int x = 18 + i;
			int y = 12 + i / 2;
			FillRect(image, x, y, 3, 3, handle);
		}
	}

	private static void DrawProhibitOverlay(Image image)
	{
		Color red = new(0.9f, 0.22f, 0.2f, 1f);
		DrawRing(image, 24, 18, 12, 3, red);
		DrawLine(image, 15, 7, 33, 29, 3, red);
	}

	private static Image CreateCanvas()
	{
		Image image = Image.CreateEmpty(Width, Height, false, Image.Format.Rgba8);
		image.Fill(new Color(0, 0, 0, 0));
		return image;
	}

	private static Texture2D ToTexture(Image image) => ImageTexture.CreateFromImage(image);

	private static void FillRect(Image image, int x, int y, int w, int h, Color color)
	{
		for (int row = y; row < y + h; row++)
		{
			for (int col = x; col < x + w; col++)
				SetPixel(image, col, row, color);
		}
	}

	private static void FillRoundedRect(Image image, int x, int y, int w, int h, int radius, Color color)
	{
		for (int row = y; row < y + h; row++)
		{
			for (int col = x; col < x + w; col++)
			{
				if (!IsInsideRoundedRect(col, row, x, y, w, h, radius))
					continue;
				SetPixel(image, col, row, color);
			}
		}
	}

	private static bool IsInsideRoundedRect(int px, int py, int x, int y, int w, int h, int radius)
	{
		if (px < x || py < y || px >= x + w || py >= y + h)
			return false;

		int right = x + w - 1;
		int bottom = y + h - 1;

		if (px < x + radius && py < y + radius)
			return DistanceSquared(px, py, x + radius, y + radius) <= radius * radius;
		if (px > right - radius && py < y + radius)
			return DistanceSquared(px, py, right - radius, y + radius) <= radius * radius;
		if (px < x + radius && py > bottom - radius)
			return DistanceSquared(px, py, x + radius, bottom - radius) <= radius * radius;
		if (px > right - radius && py > bottom - radius)
			return DistanceSquared(px, py, right - radius, bottom - radius) <= radius * radius;

		return true;
	}

	private static void DrawRing(Image image, int cx, int cy, int radius, int thickness, Color color)
	{
		int outer = radius + thickness;
		for (int y = cy - outer; y <= cy + outer; y++)
		{
			for (int x = cx - outer; x <= cx + outer; x++)
			{
				float dist = Mathf.Sqrt(DistanceSquared(x, y, cx, cy));
				if (dist > radius + thickness || dist < radius)
					continue;
				SetPixel(image, x, y, color);
			}
		}
	}

	private static void DrawLine(Image image, int x0, int y0, int x1, int y1, int thickness, Color color)
	{
		int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
		for (int i = 0; i <= steps; i++)
		{
			float t = steps == 0 ? 0f : i / (float)steps;
			int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t));
			int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));
			FillRect(image, x - thickness / 2, y - thickness / 2, thickness, thickness, color);
		}
	}

	private static void SetPixel(Image image, int x, int y, Color color)
	{
		if (x < 0 || y < 0 || x >= Width || y >= Height)
			return;
		image.SetPixel(x, y, color);
	}

	private static int DistanceSquared(int x0, int y0, int x1, int y1)
	{
		int dx = x1 - x0;
		int dy = y1 - y0;
		return dx * dx + dy * dy;
	}
}
