// 190概念稿黑边白底手绘动画界面共享样式
using CasualCastle.Domain.Building;
using Godot;

public static class CartoonUiStyles
{
	private const int ButtonFontSize = 18;
	private const int CardFontSize = 17;
	private const int HudFontSize = 24;
	private const int SmallFontSize = 16;
	private const int TextOutlineSize = 2;
	private static readonly Color Ink = new(0.025f, 0.035f, 0.045f, 1f);
	private static readonly Color Paper = new(1f, 0.99f, 0.96f, 1f);
	private static readonly Color PaleYellow = new(1f, 0.9f, 0.62f, 1f);
	private static readonly Color PaleYellowHover = new(1f, 0.94f, 0.75f, 1f);
	private static readonly Color PaleYellowPressed = new(0.95f, 0.82f, 0.48f, 1f);
	private static readonly Color Muted = new(0.4f, 0.38f, 0.33f, 1f);
	private static readonly Color HealthFill = new(0.9f, 0.27f, 0.23f, 1f);

	public static void ApplyHud(Control hud)
	{
		StylePanel(hud.GetNode<Panel>("TopBarBackground"));
		StyleProgress(hud.GetNode<ProgressBar>("PlayerHealthBar"));
		StyleProgress(hud.GetNode<ProgressBar>("EnemyHealthBar"));
		StyleHudLabel(hud.GetNode<Label>("GoldLabel"));
		StyleHudLabel(hud.GetNode<Label>("BlackMagicLabel"));
		StyleHudLabel(hud.GetNode<Label>("PhasePanel/PhaseLabel"));
		StyleHudLabel(hud.GetNode<Label>("PhasePanel/PhaseTimerLabel"));
		StyleHudButtons(hud);
		StyleShopButton(hud.GetNode<Button>("ShopButton"));
		StyleToolButton(hud.GetNode<Button>("BuildingPauseToolButton"));
		StyleToolButton(hud.GetNode<Button>("BuildingRepairToolButton"));
		StyleToolButton(hud.GetNode<Button>("CombineProhibitToolButton"));
	}

	public static void ApplyHand(Control handPanel, Button useItemButton)
	{
		StylePanel(handPanel.GetNode<Panel>("HandBackdrop"));
		StyleButton(useItemButton);
		StyleOutlinedText(handPanel.GetNode<Label>("PlacementHintLabel"), Ink);
	}

	public static void SetShopCircleHighlight(Panel panel, bool highlighted)
	{
		Color background = highlighted ? PaleYellowPressed : PaleYellow;
		StyleBoxTexture style = HandDrawnUiStyles.CreateCircle(background, Variant(panel));
		panel.AddThemeStyleboxOverride("panel", style);
	}

	public static void ApplyCardButton(Button button, bool selected, bool occupied)
	{
		Color background = selected ? PaleYellowPressed : PaleYellow;
		button.AddThemeStyleboxOverride("normal", CreateButtonStyle(button, background, 0));
		button.AddThemeStyleboxOverride("hover", CreateButtonStyle(button, PaleYellowHover, 1));
		button.AddThemeStyleboxOverride("pressed", CreateButtonStyle(button, PaleYellowPressed, 2));
		button.AddThemeStyleboxOverride("disabled", CreateButtonStyle(button, Paper, 3));
		button.AddThemeColorOverride("font_color", occupied ? Ink : Muted);
		button.AddThemeColorOverride("font_disabled_color", occupied ? Ink : Muted);
		button.AddThemeFontSizeOverride("font_size", CardFontSize);
	}

	public static void ApplyShop(CanvasLayer uiRoot)
	{
		StylePanel(uiRoot.GetNode<Panel>("ShopPanel"));
		uiRoot.GetNode<ColorRect>("ShopPanel/ShopPanelBackground").Color = Colors.Transparent;
		StyleHudLabel(uiRoot.GetNode<Label>("ShopPanel/ShopTitleLabel"));
		StyleHudLabel(uiRoot.GetNode<Label>("ShopPanel/ShopGoldLabel"));
		StyleButton(uiRoot.GetNode<Button>("ShopPanel/ShopCloseButton"));
		StyleButton(uiRoot.GetNode<Button>("ShopPanel/ShopRefreshButton"));
		for (int i = 0; i < Shop.OfferCount; i++)
			StyleOffer(uiRoot, i);
	}

	private static void StyleHudButtons(Control hud)
	{
		StyleButton(hud.GetNode<Button>("PhasePanel/NewDayButton"));
		StyleButton(hud.GetNode<Button>("PhasePanel/SkipPhaseButton"));
		StyleButton(hud.GetNode<Button>("PhasePanel/CheatButton"));
		StyleButton(hud.GetNode<Button>("PhasePanel/InfoButton"));
		StyleButton(hud.GetNode<Button>("MenuButton"));
	}

	private static void StyleOffer(CanvasLayer uiRoot, int index)
	{
		string path = $"ShopPanel/OfferSlot{index + 1}";
		Label label = uiRoot.GetNode<Label>($"{path}/OfferLabel");
		StyleOutlinedText(label, Ink);
		label.AddThemeFontSizeOverride("font_size", ButtonFontSize);
		StyleButton(uiRoot.GetNode<Button>($"{path}/BuyButton"));
	}

	private static void StylePanel(Panel panel)
	{
		StyleBoxTexture style = HandDrawnUiStyles.CreatePanel(Paper, Variant(panel));
		panel.AddThemeStyleboxOverride("panel", style);
	}

	private static void StyleProgress(ProgressBar progress)
	{
		progress.AddThemeStyleboxOverride("background", HandDrawnUiStyles.CreatePanel(Paper, Variant(progress)));
		progress.AddThemeStyleboxOverride("fill", HandDrawnUiStyles.CreatePanel(HealthFill, Variant(progress, 1)));
		StyleOutlinedText(progress, Paper);
	}

	private static void StyleHudLabel(Label label)
	{
		StyleOutlinedText(label, Ink);
		label.AddThemeFontSizeOverride("font_size", HudFontSize);
	}

	private static void StyleToolButton(Button button)
	{
		StyleButton(button);
		button.AddThemeFontSizeOverride("font_size", SmallFontSize);
	}

	private static void StyleShopButton(Button button)
	{
		StyleBoxEmpty style = new();
		button.AddThemeStyleboxOverride("normal", style);
		button.AddThemeStyleboxOverride("hover", style);
		button.AddThemeStyleboxOverride("pressed", style);
		button.AddThemeStyleboxOverride("disabled", style);
		button.AddThemeStyleboxOverride("focus", style);
	}

	private static void StyleButton(Button button)
	{
		button.AddThemeStyleboxOverride("normal", CreateButtonStyle(button, PaleYellow, 0));
		button.AddThemeStyleboxOverride("hover", CreateButtonStyle(button, PaleYellowHover, 1));
		button.AddThemeStyleboxOverride("pressed", CreateButtonStyle(button, PaleYellowPressed, 2));
		button.AddThemeStyleboxOverride("disabled", CreateButtonStyle(button, Paper, 3));
		button.AddThemeStyleboxOverride("focus", CreateButtonStyle(button, PaleYellowHover, 4));
		ApplyButtonText(button);
	}

	private static void ApplyButtonText(Button button)
	{
		button.AddThemeColorOverride("font_color", Ink);
		button.AddThemeColorOverride("font_hover_color", Ink);
		button.AddThemeColorOverride("font_pressed_color", Ink);
		button.AddThemeColorOverride("font_disabled_color", Muted);
		button.AddThemeFontSizeOverride("font_size", ButtonFontSize);
	}

	private static StyleBoxTexture CreateButtonStyle(Button button, Color background, int offset)
	{
		return HandDrawnUiStyles.CreatePanel(background, Variant(button, offset), 6f);
	}

	private static int Variant(Control control, int offset = 0)
	{
		return Mathf.Abs(control.GetPath().ToString().GetHashCode() + offset);
	}

	private static void StyleOutlinedText(Control control, Color color)
	{
		control.AddThemeColorOverride("font_color", color);
		control.AddThemeColorOverride("font_outline_color", Paper);
		control.AddThemeConstantOverride("outline_size", TextOutlineSize);
	}
}