// 粗描边动画Profile的不规则手绘框纹理工厂
using System.Collections.Generic;
using Godot;

public static class HandDrawnUiStyles
{
	private const int TextureSize = 128;
	private const float TextureMargin = 30f;
	private const float DefaultContentMargin = 8f;
	private static readonly Color Ink = new(0.025f, 0.035f, 0.045f, 1f);
	private static readonly Dictionary<string, Texture2D> Textures = new();

	public static StyleBoxTexture CreatePanel(Color background, int variant, float contentMargin = DefaultContentMargin)
	{
		StyleBoxTexture style = CreateStyle(GetPanelTexture(background, variant));
		SetContentMargins(style, contentMargin);
		return style;
	}

	public static StyleBoxTexture CreateCircle(Color background, int variant)
	{
		StyleBoxTexture style = CreateStyle(GetCircleTexture(background, variant));
		style.TextureMarginLeft = 38f;
		style.TextureMarginTop = 38f;
		style.TextureMarginRight = 38f;
		style.TextureMarginBottom = 38f;
		return style;
	}

	private static StyleBoxTexture CreateStyle(Texture2D texture)
	{
		StyleBoxTexture style = new();
		style.Texture = texture;
		style.TextureMarginLeft = TextureMargin;
		style.TextureMarginTop = TextureMargin;
		style.TextureMarginRight = TextureMargin;
		style.TextureMarginBottom = TextureMargin;
		return style;
	}

	private static void SetContentMargins(StyleBoxTexture style, float margin)
	{
		style.ContentMarginLeft = margin;
		style.ContentMarginTop = margin;
		style.ContentMarginRight = margin;
		style.ContentMarginBottom = margin;
	}

	private static Texture2D GetPanelTexture(Color background, int variant)
	{
		int normalized = Mathf.Abs(variant) % 6;
		string key = $"panel:{background.ToHtml()}:{normalized}";
		if (!Textures.ContainsKey(key))
			Textures[key] = CreateTexture(BuildPanelSvg(background, normalized));
		return Textures[key];
	}

	private static Texture2D GetCircleTexture(Color background, int variant)
	{
		int normalized = Mathf.Abs(variant) % 4;
		string key = $"circle:{background.ToHtml()}:{normalized}";
		if (!Textures.ContainsKey(key))
			Textures[key] = CreateTexture(BuildCircleSvg(background, normalized));
		return Textures[key];
	}

	private static Texture2D CreateTexture(string svg)
	{
		Image image = new();
		image.LoadSvgFromString(svg);
		return ImageTexture.CreateFromImage(image);
	}

	private static string BuildPanelSvg(Color background, int variant)
	{
		string path = GetPanelPath(variant);
		return $"<svg xmlns='http://www.w3.org/2000/svg' width='{TextureSize}' height='{TextureSize}' viewBox='0 0 128 128'><path d='{path}' fill='#{background.ToHtml(false)}' stroke='#{Ink.ToHtml(false)}' stroke-width='7' stroke-linecap='round' stroke-linejoin='round'/></svg>";
	}

	private static string BuildCircleSvg(Color background, int variant)
	{
		string path = GetCirclePath(variant);
		return $"<svg xmlns='http://www.w3.org/2000/svg' width='{TextureSize}' height='{TextureSize}' viewBox='0 0 128 128'><path d='{path}' fill='#{background.ToHtml(false)}' stroke='#{Ink.ToHtml(false)}' stroke-width='7' stroke-linecap='round' stroke-linejoin='round'/></svg>";
	}

	private static string GetPanelPath(int variant)
	{
		string[] paths =
		{
			"M10 13 C28 8 46 12 64 9 C83 7 101 12 117 10 C121 31 116 47 119 64 C121 84 116 101 118 116 C96 120 80 116 62 119 C44 121 27 116 11 118 C8 98 12 82 9 63 C7 45 12 29 10 13 Z",
			"M12 9 C30 13 47 8 65 11 C83 14 99 8 116 12 C118 29 114 48 120 65 C116 83 121 99 115 118 C98 115 82 121 64 117 C45 114 29 120 10 115 C13 96 8 80 11 62 C14 44 8 28 12 9 Z",
			"M9 12 C26 9 43 14 61 10 C80 7 98 13 119 11 C116 30 121 45 117 63 C114 82 120 99 116 117 C98 120 79 115 62 118 C43 121 26 115 12 119 C8 100 13 82 10 65 C7 47 12 31 9 12 Z",
			"M13 11 C31 7 47 13 66 9 C83 12 100 8 116 13 C120 30 115 46 119 65 C121 82 115 100 118 115 C100 119 84 116 65 120 C47 116 30 121 9 116 C12 99 8 81 11 63 C14 45 9 28 13 11 Z",
			"M10 10 C28 14 45 8 63 12 C81 8 100 14 118 9 C115 29 120 46 117 64 C120 83 115 100 119 118 C99 115 82 120 64 116 C46 121 28 115 10 119 C13 99 8 82 12 63 C8 45 13 29 10 10 Z",
			"M12 12 C29 8 48 11 64 9 C82 13 101 8 116 11 C121 28 115 47 119 63 C116 82 121 100 115 117 C98 121 81 115 63 119 C45 116 28 121 11 116 C8 99 13 81 9 64 C13 46 8 29 12 12 Z"
		};
		return paths[variant];
	}

	private static string GetCirclePath(int variant)
	{
		string[] paths =
		{
			"M64 9 C88 7 111 25 118 50 C123 75 110 101 88 114 C64 125 37 116 20 97 C4 78 7 49 20 29 C31 14 46 11 64 9 Z",
			"M64 10 C86 6 108 22 117 45 C126 68 114 94 94 110 C74 124 46 120 27 104 C8 88 4 61 15 38 C25 18 43 12 64 10 Z",
			"M63 8 C87 11 108 26 116 49 C123 72 111 99 89 113 C67 126 39 117 22 99 C5 81 7 52 18 32 C29 13 45 7 63 8 Z",
			"M64 11 C84 7 108 20 116 43 C126 67 116 93 97 108 C77 125 49 122 29 107 C9 92 3 65 14 41 C23 20 43 10 64 11 Z"
		};
		return paths[variant];
	}
}