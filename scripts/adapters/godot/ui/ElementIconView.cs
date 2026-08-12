// 攻击元素图标：按元素类型绘制彩色圆底字符图标
using Godot;
using System.Collections.Generic;

public sealed partial class ElementIconView : Control
{
	private const float IconSize = 24f;
	private const float IconRadius = 10f;
	private const int GlyphFontSize = 15;
	private const float GlyphY = 17f;
	private static readonly Color GlyphColor = new(0.06f, 0.08f, 0.12f, 1f);
	private static readonly Dictionary<string, ElementStyle> Styles = new()
	{
		{ "fire", new ElementStyle("火焰", new Color(0.95f, 0.42f, 0.2f, 1f), "火") },
		{ "poison", new ElementStyle("毒素", new Color(0.45f, 0.8f, 0.35f, 1f), "毒") },
		{ "shadow", new ElementStyle("暗影", new Color(0.62f, 0.42f, 0.9f, 1f), "影") },
		{ "frost", new ElementStyle("冰霜", new Color(0.55f, 0.82f, 1f, 1f), "冰") },
		{ "lightning", new ElementStyle("雷电", new Color(0.95f, 0.85f, 0.3f, 1f), "雷") },
		{ "holy", new ElementStyle("神圣", new Color(0.98f, 0.92f, 0.65f, 1f), "圣") },
	};

	private ElementStyle _style;

	public override void _Ready()
	{
		CustomMinimumSize = new Vector2(IconSize, IconSize);
		SizeFlagsVertical = SizeFlags.ShrinkCenter;
		MouseFilter = MouseFilterEnum.Stop;
	}

	public void Configure(string elementId)
	{
		_style = Styles[elementId];
		TooltipText = _style.DisplayName + "元素";
		QueueRedraw();
	}

	public override void _Draw()
	{
		DrawCircle(new Vector2(IconSize / 2f, IconSize / 2f), IconRadius, _style.Color);
		Font font = GetThemeDefaultFont();
		DrawString(font, new Vector2(0f, GlyphY), _style.Glyph, HorizontalAlignment.Center, IconSize, GlyphFontSize, GlyphColor);
	}

	private sealed class ElementStyle
	{
		public readonly string DisplayName;
		public readonly Color Color;
		public readonly string Glyph;
		public ElementStyle(string displayName, Color color, string glyph)
		{
			DisplayName = displayName;
			Color = color;
			Glyph = glyph;
		}
	}
}
