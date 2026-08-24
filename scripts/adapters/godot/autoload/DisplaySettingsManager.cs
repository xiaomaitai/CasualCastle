// 显示设置管理：持久化窗口、操作、音频与界面外观选项
using CasualCastle.Adapters.Godot;
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
	public static bool ConfirmSellEnabled { get; set; }
	public static bool AutoPauseNightEnabled { get; set; }
	public static bool AutoResumeAfterPlaceEnabled { get; set; }
	public event Action<UiProfileKind> UiProfileChanged;

	private const int DesignWidth = 1920;
	private const int DesignHeight = 1080;
	public const float DragSpeedMin = 0.5f;
	public const float DragSpeedMax = 2.0f;
	public const float DragSpeedStep = 0.1f;
	public const float DefaultDragSpeed = 1.0f;
	public const float GameSpeedMin = 1f;
	public const float GameSpeedMax = 4f;
	public const float DefaultMusicVolume = 1f;
	public const float DefaultSfxVolume = 0.9f;

	public static readonly Vector2I[] OutputResolutions =
	{
		new(1920, 1080),
		new(1600, 900),
		new(1366, 768),
		new(1280, 720),
	};

	private const string SettingsPath = "user://display_settings.cfg";
	private const string Section = "display";
	private const string AudioSection = "audio";

	public DisplayWindowMode WindowMode { get; private set; } = DisplayWindowMode.BorderlessFullscreen;
	public Vector2I OutputResolution { get; private set; } = new(DesignWidth, DesignHeight);
	public float CameraDragSpeed { get; private set; } = DefaultDragSpeed;
	public float GameSpeed { get; private set; } = 1f;
	public float MusicVolume { get; private set; } = DefaultMusicVolume;
	public float SfxVolume { get; private set; } = DefaultSfxVolume;
	public bool IsMuted { get; private set; }
	private float _mutedMusicVolume = DefaultMusicVolume;
	private float _mutedSfxVolume = DefaultSfxVolume;
	public UiProfileKind UiProfile { get; private set; } = UiProfileKind.Detailed;

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

	public void SaveAndApply(DisplayWindowMode mode, Vector2I resolution, float dragSpeed, float musicVolume, float sfxVolume, UiProfileKind profile)
	{
		bool profileChanged = UiProfile != profile;
		WindowMode = mode;
		OutputResolution = resolution;
		CameraDragSpeed = Mathf.Clamp(dragSpeed, DragSpeedMin, DragSpeedMax);
		MusicVolume = musicVolume;
		SfxVolume = sfxVolume;
		UiProfile = profile;
		Save();
		Apply();
		if (profileChanged)
			UiProfileChanged?.Invoke(profile);
	}

	public void SetAudioVolumes(float musicVolume, float sfxVolume)
	{
		MusicVolume = musicVolume;
		SfxVolume = sfxVolume;
		Save();
	}

	public void SetGameSpeed(float speed)
	{
		GameSpeed = Mathf.Clamp(speed, GameSpeedMin, GameSpeedMax);
		Save();
	}

	public void ToggleMute()
	{
		if (IsMuted)
		{
			MusicVolume = _mutedMusicVolume;
			SfxVolume = _mutedSfxVolume;
			IsMuted = false;
		}
		else
		{
			_mutedMusicVolume = MusicVolume;
			_mutedSfxVolume = SfxVolume;
			MusicVolume = 0f;
			SfxVolume = 0f;
			IsMuted = true;
		}
		Save();
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
		DevModeEnabled = (bool)config.GetValue(Section, "dev_mode", false);
		ConfirmSellEnabled = (bool)config.GetValue(Section, "confirm_sell", false);
		AutoPauseNightEnabled = (bool)config.GetValue(Section, "auto_pause_night", false);
		AutoResumeAfterPlaceEnabled = (bool)config.GetValue(Section, "auto_resume_after_place", false);
		int legacyUiProfile = (int)config.GetValue(Section, "attribute_panel_profile", (int)UiProfile);
		UiProfile = (UiProfileKind)(int)config.GetValue(Section, "ui_profile", legacyUiProfile);
		CameraDragSpeed = Mathf.Clamp(
			(float)config.GetValue(Section, "camera_drag_speed", CameraDragSpeed),
			DragSpeedMin,
			DragSpeedMax);
		GameSpeed = Mathf.Clamp(
			(float)config.GetValue(Section, "game_speed", 1f),
			GameSpeedMin,
			GameSpeedMax);
		MusicVolume = Mathf.Clamp(
			(float)config.GetValue(AudioSection, "music_volume", MusicVolume),
			0f,
			1f);
		SfxVolume = Mathf.Clamp(
			(float)config.GetValue(AudioSection, "sfx_volume", SfxVolume),
			0f,
			1f);
	}

	private void Save()
	{
		ConfigFile config = new ConfigFile();
		config.SetValue(Section, "window_mode", (int)WindowMode);
		config.SetValue(Section, "width", OutputResolution.X);
		config.SetValue(Section, "height", OutputResolution.Y);
		config.SetValue(Section, "dev_mode", DevModeEnabled);
		config.SetValue(Section, "confirm_sell", ConfirmSellEnabled);
		config.SetValue(Section, "auto_pause_night", AutoPauseNightEnabled);
		config.SetValue(Section, "auto_resume_after_place", AutoResumeAfterPlaceEnabled);
		config.SetValue(Section, "ui_profile", (int)UiProfile);
		config.SetValue(Section, "camera_drag_speed", CameraDragSpeed);
		config.SetValue(Section, "game_speed", GameSpeed);
		config.SetValue(AudioSection, "music_volume", MusicVolume);
		config.SetValue(AudioSection, "sfx_volume", SfxVolume);
		config.Save(SettingsPath);
	}

	private void Apply()
	{
		Window window = GetTree().Root;
		window.ContentScaleSize = new Vector2I(DesignWidth, DesignHeight);
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
