using Godot;

public static class BuildingToolIcons
{
	private static Texture2D _pause;
	private static Texture2D _hammer;

	public static Texture2D Pause => _pause ??= CreatePauseIcon();
	public static Texture2D Hammer => _hammer ??= CreateHammerIcon();

	private static Texture2D CreatePauseIcon()
	{
		const int size = 32;
		Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
		image.Fill(new Color(0, 0, 0, 0));

		Color color = new Color(0.92f, 0.95f, 1f, 1f);
		FillRect(image, 9, 8, 5, 16, color);
		FillRect(image, 18, 8, 5, 16, color);

		return ImageTexture.CreateFromImage(image);
	}

	private static Texture2D CreateHammerIcon()
	{
		const int size = 32;
		Image image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
		image.Fill(new Color(0, 0, 0, 0));

		Color head = new Color(0.95f, 0.78f, 0.35f, 1f);
		Color handle = new Color(0.72f, 0.52f, 0.28f, 1f);

		FillRect(image, 8, 6, 16, 8, head);
		FillRect(image, 20, 12, 4, 14, handle);

		return ImageTexture.CreateFromImage(image);
	}

	private static void FillRect(Image image, int x, int y, int width, int height, Color color)
	{
		for (int row = y; row < y + height; row++)
		{
			for (int col = x; col < x + width; col++)
				image.SetPixel(col, row, color);
		}
	}
}
