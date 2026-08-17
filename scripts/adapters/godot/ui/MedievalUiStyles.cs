// 黑金中世纪交互界面共享样式
using CasualCastle.Domain.Building;
using Godot;

public static class MedievalUiStyles
{
	private const int PanelBorderWidth = 2;
	private const int ButtonBorderWidth = 2;
	private const int PanelCornerRadius = 4;
	private const int ButtonCornerRadius = 3;
	private const int ButtonFontSize = 18;
	private const int CardFontSize = 17;
	private const int HudFontSize = 24;
	private const int SmallFontSize = 16;
	private const int PanelShadowSize = 8;
	private const int ShopCircleRadius = 54;
	private const int ShopCircleHighlightBorder = 3;
	private const int ShopCircleHighlightShadow = 10;
	private const float PanelContentMargin = 8f;
	private static readonly Color PanelBackground = new(0.018f, 0.022f, 0.024f, 0.96f);
	private static readonly Color PanelBorder = new(0.63f, 0.46f, 0.22f, 1f);
	private static readonly Color PanelShadow = new(0f, 0f, 0f, 0.7f);
	private static readonly Color ButtonBackground = new(0.055f, 0.05f, 0.038f, 1f);
	private static readonly Color ButtonHover = new(0.14f, 0.105f, 0.05f, 1f);
	private static readonly Color ButtonPressed = new(0.2f, 0.14f, 0.055f, 1f);
	private static readonly Color ButtonDisabled = new(0.035f, 0.038f, 0.038f, 0.88f);
	private static readonly Color SelectedBackground = new(0.2f, 0.14f, 0.045f, 1f);
	private static readonly Color Gold = new(0.93f, 0.76f, 0.42f, 1f);
	private static readonly Color BrightGold = new(1f, 0.88f, 0.55f, 1f);
	private static readonly Color Text = new(0.88f, 0.85f, 0.76f, 1f);
	private static readonly Color MutedText = new(0.55f, 0.54f, 0.49f, 1f);
	private static readonly Color HealthBackground = new(0.07f, 0.045f, 0.035f, 1f);
	private static readonly Color HealthFill = new(0.3f, 0.65f, 0.3f, 1f);

	public static void ApplyHud(Control hud)
	{
		StyleTopBar(hud.GetNode<Panel>("TopBarBackground"));
		StyleProgress(hud.GetNode<ProgressBar>("PlayerHealthBar"));
		StyleProgress(hud.GetNode<ProgressBar>("EnemyHealthBar"));
		StyleHudLabel(hud.GetNode<Label>("GoldLabel"), Gold);
		StyleHudLabel(hud.GetNode<Label>("BlackMagicLabel"), new Color(0.7f, 0.46f, 0.88f, 1f));
		StyleHudLabel(hud.GetNode<Label>("PhasePanel/PhaseLabel"), Gold);
		StyleHudLabel(hud.GetNode<Label>("PhasePanel/PhaseTimerLabel"), Text);
		StyleButton(hud.GetNode<Button>("PhasePanel/NewDayButton"), true);
		StyleButton(hud.GetNode<Button>("PhasePanel/SkipPhaseButton"));
		StyleButton(hud.GetNode<Button>("PhasePanel/CheatButton"));
		StyleButton(hud.GetNode<Button>("PhasePanel/InfoButton"));
		StyleButton(hud.GetNode<Button>("MenuButton"), true);
		StyleShopButton(hud.GetNode<Button>("ShopButton"));
		StyleToolButton(hud.GetNode<Button>("BuildingPauseToolButton"));
		StyleToolButton(hud.GetNode<Button>("BuildingRepairToolButton"));
		StyleToolButton(hud.GetNode<Button>("CombineProhibitToolButton"));
	}

	public static void ApplyHand(Control handPanel, Button useItemButton)
	{
		StylePanel(handPanel.GetNode<Panel>("HandBackdrop"));
		StyleButton(useItemButton, true);
		Label hint = handPanel.GetNode<Label>("PlacementHintLabel");
		hint.AddThemeColorOverride("font_color", BrightGold);
		hint.AddThemeColorOverride("font_outline_color", Colors.Black);
		hint.AddThemeConstantOverride("outline_size", PanelBorderWidth);
	}

	public static void SetShopCircleHighlight(Panel panel, bool highlighted)
	{
		Color background = highlighted ? ButtonPressed : ButtonBackground;
		Color border = highlighted ? BrightGold : PanelBorder;
		int width = highlighted ? ShopCircleHighlightBorder : ButtonBorderWidth;
		StyleBoxFlat style = CreateFlatStyle(background, border, width, ShopCircleRadius);
		style.ShadowColor = highlighted ? Gold : PanelShadow;
		style.ShadowSize = highlighted ? ShopCircleHighlightShadow : PanelShadowSize;
		panel.AddThemeStyleboxOverride("panel", style);
	}

	public static void ApplyCardButton(Button button, bool selected, bool occupied)
	{
		Color background = selected ? SelectedBackground : ButtonBackground;
		Color border = selected ? BrightGold : PanelBorder;
		button.AddThemeStyleboxOverride("normal", CreateButtonStyle(background, border));
		button.AddThemeStyleboxOverride("hover", CreateButtonStyle(ButtonHover, BrightGold));
		button.AddThemeStyleboxOverride("pressed", CreateButtonStyle(ButtonPressed, BrightGold));
		button.AddThemeStyleboxOverride("disabled", CreateButtonStyle(ButtonDisabled, PanelBorder));
		button.AddThemeColorOverride("font_color", occupied ? Text : MutedText);
		button.AddThemeColorOverride("font_disabled_color", occupied ? Text : MutedText);
		button.AddThemeFontSizeOverride("font_size", CardFontSize);
	}

	public static void ApplyShop(CanvasLayer uiRoot)
	{
		Panel panel = uiRoot.GetNode<Panel>("ShopPanel");
		StylePanel(panel);
		uiRoot.GetNode<ColorRect>("ShopPanel/ShopPanelBackground").Color = Colors.Transparent;
		StyleHudLabel(uiRoot.GetNode<Label>("ShopPanel/ShopTitleLabel"), Gold);
		StyleHudLabel(uiRoot.GetNode<Label>("ShopPanel/ShopGoldLabel"), BrightGold);
		StyleButton(uiRoot.GetNode<Button>("ShopPanel/ShopCloseButton"));
		StyleButton(uiRoot.GetNode<Button>("ShopPanel/ShopRefreshButton"), true);
		for (int i = 0; i < Shop.OfferCount; i++)
			StyleOffer(uiRoot, i);
	}

	private static void StyleOffer(CanvasLayer uiRoot, int index)
	{
		string path = $"ShopPanel/OfferSlot{index + 1}";
		Label label = uiRoot.GetNode<Label>($"{path}/OfferLabel");
		label.AddThemeColorOverride("font_color", Text);
		label.AddThemeFontSizeOverride("font_size", ButtonFontSize);
		StyleButton(uiRoot.GetNode<Button>($"{path}/BuyButton"), true);
	}

	private static void StyleTopBar(Panel panel)
	{
		StyleBoxFlat style = new();
		style.BgColor = PanelBackground;
		style.BorderColor = PanelBorder;
		style.BorderWidthBottom = PanelBorderWidth;
		panel.AddThemeStyleboxOverride("panel", style);
	}

	private static void StylePanel(Panel panel)
	{
		StyleBoxFlat style = CreatePanelStyle();
		panel.AddThemeStyleboxOverride("panel", style);
	}

	private static void StyleProgress(ProgressBar progress)
	{
		progress.AddThemeStyleboxOverride("background", CreateFlatStyle(HealthBackground, PanelBorder, PanelBorderWidth, PanelCornerRadius));
		progress.AddThemeStyleboxOverride("fill", CreateFlatStyle(HealthFill, PanelBorder, PanelBorderWidth, PanelCornerRadius));
		progress.AddThemeColorOverride("font_color", Text);
		progress.AddThemeColorOverride("font_outline_color", Colors.Black);
		progress.AddThemeConstantOverride("outline_size", PanelBorderWidth);
	}

	private static void StyleHudLabel(Label label, Color color)
	{
		label.AddThemeColorOverride("font_color", color);
		label.AddThemeColorOverride("font_outline_color", Colors.Black);
		label.AddThemeConstantOverride("outline_size", PanelBorderWidth);
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

	private static void StyleButton(Button button, bool emphasized = false)
	{
		Color border = emphasized ? BrightGold : PanelBorder;
		button.AddThemeStyleboxOverride("normal", CreateButtonStyle(ButtonBackground, border));
		button.AddThemeStyleboxOverride("hover", CreateButtonStyle(ButtonHover, BrightGold));
		button.AddThemeStyleboxOverride("pressed", CreateButtonStyle(ButtonPressed, BrightGold));
		button.AddThemeStyleboxOverride("disabled", CreateButtonStyle(ButtonDisabled, PanelBorder));
		button.AddThemeStyleboxOverride("focus", CreateButtonStyle(ButtonHover, BrightGold));
		button.AddThemeColorOverride("font_color", Text);
		button.AddThemeColorOverride("font_hover_color", BrightGold);
		button.AddThemeColorOverride("font_pressed_color", BrightGold);
		button.AddThemeColorOverride("font_disabled_color", MutedText);
		button.AddThemeFontSizeOverride("font_size", ButtonFontSize);
	}

	private static StyleBoxFlat CreatePanelStyle()
	{
		StyleBoxFlat style = CreateFlatStyle(PanelBackground, PanelBorder, PanelBorderWidth, PanelCornerRadius);
		style.ShadowColor = PanelShadow;
		style.ShadowSize = PanelShadowSize;
		style.ContentMarginLeft = PanelContentMargin;
		style.ContentMarginTop = PanelContentMargin;
		style.ContentMarginRight = PanelContentMargin;
		style.ContentMarginBottom = PanelContentMargin;
		return style;
	}

	private static StyleBoxFlat CreateButtonStyle(Color background, Color border)
	{
		return CreateFlatStyle(background, border, ButtonBorderWidth, ButtonCornerRadius);
	}

	private static StyleBoxFlat CreateFlatStyle(Color background, Color border, int width, int radius)
	{
		StyleBoxFlat style = new();
		style.BgColor = background;
		style.BorderColor = border;
		style.SetBorderWidthAll(width);
		style.SetCornerRadiusAll(radius);
		return style;
	}
}
