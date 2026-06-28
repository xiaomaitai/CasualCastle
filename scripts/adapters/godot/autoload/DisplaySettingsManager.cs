using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Shared;
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
	public static bool DevModeEnabled { get; set; }

	public static readonly Vector2I[] OutputResolutions =
	{
		new(1920, 1080),
		new(1600, 900),
		new(1366, 768),
		new(1280, 720),
	};

	private const string SettingsPath = "user://display_settings.cfg";
	private const string Section = "display";

	public DisplayWindowMode WindowMode { get; private set; } = DisplayWindowMode.BorderlessFullscreen;
	public Vector2I OutputResolution { get; private set; } = new(GameRules.DesignWidth, GameRules.DesignHeight);

	public override void _Ready()
	{
		Instance = this;
		AdapterRegistry.Register<DisplaySettingsManager>(this);
		Load();
		Apply();
	}

	public override void _ExitTree()
	{
		if (Instance == this)
		{
			AdapterRegistry.Unregister<DisplaySettingsManager>(this);
			Instance = null;
		}
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
		for (int i = 0; i < OutputResolutions.Length; i++)
		{
			if (OutputResolutions[i] == resolution)
				return i;
		}

		return 0;
	}

	private void Load()
	{
		ConfigFile config = new ConfigFile();
		if (config.Load(SettingsPath) != Error.Ok)
			return;

		WindowMode = (DisplayWindowMode)(int)config.GetValue(Section, "window_mode", (int)WindowMode);
		int width = (int)config.GetValue(Section, "width", OutputResolution.X);
		int height = (int)config.GetValue(Section, "height", OutputResolution.Y);
		OutputResolution = new Vector2I(width, height);
	}

	private void Save()
	{
		ConfigFile config = new ConfigFile();
		config.SetValue(Section, "window_mode", (int)WindowMode);
		config.SetValue(Section, "width", OutputResolution.X);
		config.SetValue(Section, "height", OutputResolution.Y);
		config.Save(SettingsPath);
	}

	private void Apply()
	{
		Window window = GetTree().Root;
		window.ContentScaleSize = new Vector2I(GameRules.DesignWidth, GameRules.DesignHeight);
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
