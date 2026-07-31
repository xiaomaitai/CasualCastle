// 程序化生成可无缝平铺的草地、泥地与石板底纹纹理。
using Godot;

public static class SeamlessTileFactory
{
	private const int TileSize = 256;
	private const int LatticeSize = 12;
	private const int StoneSlabSize = 64;
	private const int StoneJointWidth = 4;
	private const float GrainStrength = 0.12f;
	private const float StoneJointStrength = 0.28f;
	private const uint NoiseSeed = 7919u;
	private static readonly Color GrassBase = new Color(0.42f, 0.62f, 0.30f);
	private static readonly Color GrassVariation = new Color(0.14f, 0.20f, 0.08f);
	private static readonly Color MudBase = new Color(0.36f, 0.27f, 0.17f);
	private static readonly Color MudVariation = new Color(0.10f, 0.08f, 0.05f);
	private static readonly Color StoneBase = new Color(0.46f, 0.46f, 0.49f);
	private static readonly Color StoneVariation = new Color(0.09f, 0.09f, 0.10f);

	public static Texture2D CreateGrassTexture() => CreateNoiseTexture(GrassBase, GrassVariation);

	public static Texture2D CreateMudTexture() => CreateNoiseTexture(MudBase, MudVariation);

	public static Texture2D CreateStoneTexture() => CreateStoneImage();

	private static Texture2D CreateNoiseTexture(Color baseColor, Color variation)
	{
		Image image = Image.CreateEmpty(TileSize, TileSize, false, Image.Format.Rgba8);
		for (int y = 0; y < TileSize; y++)
		{
			for (int x = 0; x < TileSize; x++)
			{
				float u = (float)x / TileSize;
				float v = (float)y / TileSize;
				float noise = SamplePeriodicNoise(u, v);
				float grain = Hash01(x, y) - 0.5f;
				Color pixel = baseColor + (noise - 0.5f) * variation * 2f;
				pixel += variation * grain * GrainStrength * 2f;
				image.SetPixel(x, y, pixel);
			}
		}
		return ImageTexture.CreateFromImage(image);
	}

	private static Texture2D CreateStoneImage()
	{
		Image image = Image.CreateEmpty(TileSize, TileSize, false, Image.Format.Rgba8);
		for (int y = 0; y < TileSize; y++)
		{
			for (int x = 0; x < TileSize; x++)
			{
				float u = (float)x / TileSize;
				float v = (float)y / TileSize;
				float noise = SamplePeriodicNoise(u, v);
				float grain = Hash01(x, y) - 0.5f;
				Color pixel = StoneBase + (noise - 0.5f) * StoneVariation * 2f;
				pixel += StoneVariation * grain * GrainStrength * 2f;
				pixel *= 1f - JointFactor(x, y) * StoneJointStrength;
				image.SetPixel(x, y, pixel);
			}
		}
		return ImageTexture.CreateFromImage(image);
	}

	private static float SamplePeriodicNoise(float u, float v)
	{
		float x = u * LatticeSize;
		float y = v * LatticeSize;
		int x0 = Mathf.FloorToInt(x);
		int y0 = Mathf.FloorToInt(y);
		float sx = SmoothStep(x - x0);
		float sy = SmoothStep(y - y0);
		float top = Mathf.Lerp(LatticeHash(x0, y0), LatticeHash(x0 + 1, y0), sx);
		float bottom = Mathf.Lerp(LatticeHash(x0, y0 + 1), LatticeHash(x0 + 1, y0 + 1), sx);
		return Mathf.Lerp(top, bottom, sy);
	}

	private static float LatticeHash(int x, int y)
	{
		int wx = Mod(x, LatticeSize);
		int wy = Mod(y, LatticeSize);
		uint h = (uint)(wx * 374761393 + wy * 668265263) ^ NoiseSeed;
		h = (h ^ (h >> 13)) * 1274126177u;
		h ^= h >> 16;
		return (h & 0xFFFFFFu) / 16777216f;
	}

	private static float Hash01(int x, int y)
	{
		uint h = (uint)(x * 1000003 + y * 974711) ^ NoiseSeed;
		h = (h ^ (h >> 11)) * 16777619u;
		return (h & 0xFFFFFFu) / 16777216f;
	}

	private static float SmoothStep(float t) => t * t * (3f - 2f * t);

	private static int Mod(int value, int modulus)
	{
		int result = value % modulus;
		return result < 0 ? result + modulus : result;
	}

	private static float JointFactor(int x, int y)
	{
		float dx = EdgeDistance(x);
		float dy = EdgeDistance(y);
		float edge = 1f - Mathf.Min(dx, dy);
		return Mathf.Clamp(edge / StoneJointWidth, 0f, 1f);
	}

	private static float EdgeDistance(int value)
	{
		int offset = value % StoneSlabSize;
		return Mathf.Min(offset, StoneSlabSize - 1 - offset);
	}
}
