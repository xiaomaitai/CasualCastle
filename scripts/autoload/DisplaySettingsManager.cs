// === Godot Window Adapter ===
// Belongs to adapters/godot layer. Directly manipulates Godot DisplayServer/Window APIs.
// No domain rules involved — purely engine-level configuration.

using Godot;
using System;

public enum DisplayWindowMode
{
	Windowed,
	BorderlessFullscreen,
}

public partial class DisplaySettingsManager : Node
{
	public static DisplaySettingsManager Instance { get; private set; }

	private const string SettingsPath = "user://display_settings.cfg";
	private const string Section = "display";

	public DisplayWindowMode WindowMode { get; private set; } = DisplayWindowMode.BorderlessFullscreen;
	public Vector2I OutputResolution { get; private set; } = new(GameConfig.DesignWidth, GameConfig.DesignHeight);

	public override void _Ready()
	{
		Instance = this;
		Load();
		Apply();
	}

	public override void _ExitTree()
	{
		if (Instance == this)
			Instance = null;
	}

	public void SaveAndApply(DisplayWindowMode mode, Vector2I resolution)
	{
		WindowMode = mode;
		OutputResolution = resolution;
		Save();
		Apply();
	}

	public int FindResolutionIndex(Vector2I resolution)
	{
		for (int i = 0; i < GameConfig.OutputResolutions.Length; i++)
		{
			if (GameConfig.OutputResolutions[i] == resolution)
				return i;
		}

		return 0;
	}

	private void Load()
	{
		var config = new ConfigFile();
		if (config.Load(SettingsPath) != Error.Ok)
			return;

		WindowMode = (DisplayWindowMode)(int)config.GetValue(Section, "window_mode", (int)WindowMode);
		int width = (int)config.GetValue(Section, "width", OutputResolution.X);
		int height = (int)config.GetValue(Section, "height", OutputResolution.Y);
		OutputResolution = new Vector2I(width, height);
	}

	private void Save()
	{
		var config = new ConfigFile();
		config.SetValue(Section, "window_mode", (int)WindowMode);
		config.SetValue(Section, "width", OutputResolution.X);
		config.SetValue(Section, "height", OutputResolution.Y);
		config.Save(SettingsPath);
	}

	private void Apply()
	{
		var window = GetTree().Root;
		window.ContentScaleSize = new Vector2I(GameConfig.DesignWidth, GameConfig.DesignHeight);
		window.ContentScaleMode = Window.ContentScaleModeEnum.CanvasItems;
		window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;

		if (WindowMode == DisplayWindowMode.BorderlessFullscreen)
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
			return;
		}

		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		DisplayServer.WindowSetSize(OutputResolution);
		CenterWindow(OutputResolution);
	}

	private static void CenterWindow(Vector2I size)
	{
		int screen = DisplayServer.WindowGetCurrentScreen();
		Vector2I screenSize = DisplayServer.ScreenGetSize(screen);
		DisplayServer.WindowSetPosition((screenSize - size) / 2);
	}
}
