// 用 MultiMeshInstance2D 随机散布草丛与石子的装饰层工厂。
using System;
using Godot;

public static class BattlefieldDecorationFactory
{
	private const int DecorTextureSize = 128;
	private const float GrassDensity = 0.000022f;
	private const float StoneDensity = 0.000011f;
	private const float MinScale = 0.6f;
	private const float MaxScale = 1.4f;
	private const int GrassBladeCount = 26;
	private const int PebbleCount = 6;
	private const int GrassLayerSeed = 101;
	private const int StoneLayerSeed = 202;
	private const int GrassTextureSeed = 303;
	private const int StoneTextureSeed = 404;
	private static readonly Color GrassBladeBase = new Color(0.30f, 0.52f, 0.20f);
	private static readonly Color GrassBladeHighlight = new Color(0.46f, 0.66f, 0.26f);
	private static readonly Color PebbleBase = new Color(0.54f, 0.55f, 0.58f);
	private static readonly Color PebbleDark = new Color(0.36f, 0.37f, 0.39f);

	public static MultiMeshInstance2D CreateGrassLayer(Rect2 worldRect) =>
		CreateScatterLayer(CreateGrassTuftTexture(), worldRect, GrassDensity, GrassLayerSeed);

	public static MultiMeshInstance2D CreateStoneLayer(Rect2 worldRect) =>
		CreateScatterLayer(CreatePebbleTexture(), worldRect, StoneDensity, StoneLayerSeed);

	private static MultiMeshInstance2D CreateScatterLayer(
		Texture2D texture, Rect2 worldRect, float density, int seed)
	{
		int count = Mathf.RoundToInt(worldRect.Size.X * worldRect.Size.Y * density);
		MultiMesh multiMesh = CreateMultiMesh(count);
		ScatterInstances(multiMesh, worldRect, seed);
		return CreateLayer(multiMesh, texture);
	}

	private static MultiMesh CreateMultiMesh(int count)
	{
		QuadMesh mesh = new QuadMesh();
		mesh.Size = new Vector2(DecorTextureSize, DecorTextureSize);
		MultiMesh multiMesh = new MultiMesh();
		multiMesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform2D;
		multiMesh.Mesh = mesh;
		multiMesh.InstanceCount = count;
		return multiMesh;
	}

	private static void ScatterInstances(MultiMesh multiMesh, Rect2 worldRect, int seed)
	{
		Random rng = new Random(seed);
		for (int i = 0; i < multiMesh.InstanceCount; i++)
		{
			float x = worldRect.Position.X + (float)rng.NextDouble() * worldRect.Size.X;
			float y = worldRect.Position.Y + (float)rng.NextDouble() * worldRect.Size.Y;
			float scale = MinScale + (float)rng.NextDouble() * (MaxScale - MinScale);
			float rotation = (float)rng.NextDouble() * Mathf.Pi * 2f;
			Transform2D transform = new Transform2D(rotation, new Vector2(scale, scale), 0f, new Vector2(x, y));
			multiMesh.SetInstanceTransform2D(i, transform);
		}
	}

	private static MultiMeshInstance2D CreateLayer(MultiMesh multiMesh, Texture2D texture)
	{
		MultiMeshInstance2D layer = new MultiMeshInstance2D();
		layer.Multimesh = multiMesh;
		layer.Texture = texture;
		layer.ZIndex = -5;
		return layer;
	}

	private static Texture2D CreateGrassTuftTexture()
	{
		Image image = CreateTransparentImage();
		Random rng = new Random(GrassTextureSeed);
		for (int i = 0; i < GrassBladeCount; i++)
			DrawGrassBlade(image, rng);
		return ImageTexture.CreateFromImage(image);
	}

	private static Texture2D CreatePebbleTexture()
	{
		Image image = CreateTransparentImage();
		Random rng = new Random(StoneTextureSeed);
		for (int i = 0; i < PebbleCount; i++)
			DrawPebble(image, rng);
		return ImageTexture.CreateFromImage(image);
	}

	private static Image CreateTransparentImage()
	{
		Image image = Image.CreateEmpty(DecorTextureSize, DecorTextureSize, false, Image.Format.Rgba8);
		image.Fill(new Color(0f, 0f, 0f, 0f));
		return image;
	}

	private static void DrawGrassBlade(Image image, Random rng)
	{
		float baseX = 10f + (float)rng.NextDouble() * 108f;
		float baseY = 100f + (float)rng.NextDouble() * 26f;
		float height = 34f + (float)rng.NextDouble() * 38f;
		float lean = -10f + (float)rng.NextDouble() * 20f;
		float halfWidth = 2.5f + (float)rng.NextDouble() * 2f;
		Color color = rng.NextDouble() < 0.5 ? GrassBladeBase : GrassBladeHighlight;
		for (float t = 0f; t <= 1f; t += 0.04f)
		{
			float y = baseY - t * height;
			float x = baseX + t * lean;
			float width = halfWidth * (1f - t * 0.7f);
			DrawBladeRow(image, x, y, width, color);
		}
	}

	private static void DrawBladeRow(Image image, float x, float y, float width, Color color)
	{
		int xi0 = Mathf.FloorToInt(x - width);
		int xi1 = Mathf.CeilToInt(x + width);
		int yi = Mathf.RoundToInt(y);
		for (int xi = xi0; xi <= xi1; xi++)
		{
			if (xi < 0 || xi >= DecorTextureSize || yi < 0 || yi >= DecorTextureSize)
				continue;
			float falloff = 1f - Mathf.Abs(xi - x) / Mathf.Max(width, 0.001f);
			Color pixel = color;
			pixel.A = Mathf.Max(image.GetPixel(xi, yi).A, falloff);
			image.SetPixel(xi, yi, pixel);
		}
	}

	private static void DrawPebble(Image image, Random rng)
	{
		float cx = 20f + (float)rng.NextDouble() * 88f;
		float cy = 20f + (float)rng.NextDouble() * 88f;
		float radius = 9f + (float)rng.NextDouble() * 13f;
		Color color = rng.NextDouble() < 0.5 ? PebbleBase : PebbleDark;
		int x0 = Mathf.FloorToInt(cx - radius);
		int x1 = Mathf.CeilToInt(cx + radius);
		int y0 = Mathf.FloorToInt(cy - radius);
		int y1 = Mathf.CeilToInt(cy + radius);
		for (int y = y0; y <= y1; y++)
			for (int x = x0; x <= x1; x++)
				DrawPebblePixel(image, x, y, cx, cy, radius, color);
	}

	private static void DrawPebblePixel(
		Image image, int x, int y, float cx, float cy, float radius, Color color)
	{
		if (x < 0 || x >= DecorTextureSize || y < 0 || y >= DecorTextureSize)
			return;
		float dx = x - cx;
		float dy = y - cy;
		float edge = Mathf.Clamp(1f - Mathf.Sqrt(dx * dx + dy * dy) / radius, 0f, 1f);
		float shade = 1f + (PebbleHash(x, y) - 0.5f) * 0.24f;
		Color pixel = color * shade;
		pixel.A = Mathf.Max(image.GetPixel(x, y).A, edge);
		image.SetPixel(x, y, pixel);
	}

	private static float PebbleHash(int x, int y)
	{
		uint h = (uint)(x * 374761393 + y * 668265263);
		h = (h ^ (h >> 13)) * 1274126177u;
		h ^= h >> 16;
		return (h & 0xFFFFu) / 65535f;
	}
}
