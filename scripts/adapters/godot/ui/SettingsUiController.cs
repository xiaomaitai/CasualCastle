// 设置面板控制器：显示/分辨率/开发者模式/拖拽速度与音乐音效音量。
using CasualCastle.Adapters.Godot;
using Godot;
using System;

public sealed class SettingsUiController
{
	private const float VolumeMinPercent = 0f;
	private const float VolumeMaxPercent = 100f;
	private const float VolumeStep = 1f;

	private readonly Control _root;
	private readonly OptionButton _windowModeOption;
	private readonly OptionButton _resolutionOption;
	private readonly CheckBox _devModeCheck;
	private readonly CheckBox _confirmSellCheck;
	private readonly HSlider _dragSpeedSlider;
	private readonly Label _dragSpeedValueLabel;
	private readonly HSlider _musicVolumeSlider;
	private readonly Label _musicVolumeValueLabel;
	private readonly HSlider _sfxVolumeSlider;
	private readonly Label _sfxVolumeValueLabel;
	private readonly Button _applyButton;
	private readonly Button _backButton;

	public bool IsOpen { get; private set; }
	public event Action<bool> OpenChanged;
	private float _dragSpeed = DisplaySettingsManager.DefaultDragSpeed;
	private float _musicVolume = DisplaySettingsManager.DefaultMusicVolume;
	private float _sfxVolume = DisplaySettingsManager.DefaultSfxVolume;
	private bool _syncing;

	public SettingsUiController(Control root)
	{
		_root = root;
		_windowModeOption = root.GetNode<OptionButton>("Panel/WindowModeOption");
		_resolutionOption = root.GetNode<OptionButton>("Panel/ResolutionOption");
		_devModeCheck = root.GetNode<CheckBox>("Panel/DevModeCheck");
		_confirmSellCheck = root.GetNode<CheckBox>("Panel/ConfirmSellCheck");
		_dragSpeedSlider = root.GetNode<HSlider>("Panel/DragSpeedSlider");
		_dragSpeedValueLabel = root.GetNode<Label>("Panel/DragSpeedValueLabel");
		_musicVolumeSlider = root.GetNode<HSlider>("Panel/MusicVolumeSlider");
		_musicVolumeValueLabel = root.GetNode<Label>("Panel/MusicVolumeValueLabel");
		_sfxVolumeSlider = root.GetNode<HSlider>("Panel/SfxVolumeSlider");
		_sfxVolumeValueLabel = root.GetNode<Label>("Panel/SfxVolumeValueLabel");
		_applyButton = root.GetNode<Button>("Panel/ApplyButton");
		_backButton = root.GetNode<Button>("Panel/BackButton");

		_windowModeOption.Clear();
		_windowModeOption.AddItem("窗口化", (int)DisplayWindowMode.Windowed);
		_windowModeOption.AddItem("无边框全屏", (int)DisplayWindowMode.BorderlessFullscreen);

		_resolutionOption.Clear();
		foreach (Vector2I resolution in DisplaySettingsManager.OutputResolutions)
			_resolutionOption.AddItem($"{resolution.X} × {resolution.Y}", _resolutionOption.ItemCount);

		_windowModeOption.ItemSelected += OnWindowModeSelected;
		_dragSpeedSlider.MinValue = DisplaySettingsManager.DragSpeedMin;
		_dragSpeedSlider.MaxValue = DisplaySettingsManager.DragSpeedMax;
		_dragSpeedSlider.Step = DisplaySettingsManager.DragSpeedStep;
		_dragSpeedSlider.ValueChanged += OnDragSpeedChanged;
		_musicVolumeSlider.MinValue = VolumeMinPercent;
		_musicVolumeSlider.MaxValue = VolumeMaxPercent;
		_musicVolumeSlider.Step = VolumeStep;
		_musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
		_musicVolumeSlider.DragEnded += OnVolumeDragEnded;
		_sfxVolumeSlider.MinValue = VolumeMinPercent;
		_sfxVolumeSlider.MaxValue = VolumeMaxPercent;
		_sfxVolumeSlider.Step = VolumeStep;
		_sfxVolumeSlider.ValueChanged += OnSfxVolumeChanged;
		_sfxVolumeSlider.DragEnded += OnVolumeDragEnded;
		_applyButton.Pressed += OnApplyPressed;
		_backButton.Pressed += OnBackPressed;

		_root.Visible = false;
	}

	public void Dispose()
	{
		_windowModeOption.ItemSelected -= OnWindowModeSelected;
		_dragSpeedSlider.ValueChanged -= OnDragSpeedChanged;
		_musicVolumeSlider.ValueChanged -= OnMusicVolumeChanged;
		_musicVolumeSlider.DragEnded -= OnVolumeDragEnded;
		_sfxVolumeSlider.ValueChanged -= OnSfxVolumeChanged;
		_sfxVolumeSlider.DragEnded -= OnVolumeDragEnded;
		_applyButton.Pressed -= OnApplyPressed;
		_backButton.Pressed -= OnBackPressed;
	}

	public void Open()
	{
		if (IsOpen)
			return;

		IsOpen = true;
		_root.Visible = true;
		SyncFromSaved();
		OpenChanged?.Invoke(true);
	}

	public bool Close()
	{
		if (!IsOpen)
			return false;

		IsOpen = false;
		_root.Visible = false;
		OpenChanged?.Invoke(false);
		return true;
	}

	private void SyncFromSaved()
	{
		_syncing = true;
		DisplaySettingsManager settings = AdapterRegistry.Resolve<DisplaySettingsManager>();
		_windowModeOption.Select(_windowModeOption.GetItemIndex((int)settings.WindowMode));
		_resolutionOption.Select(settings.FindResolutionIndex(settings.OutputResolution));
		_devModeCheck.ButtonPressed = DisplaySettingsManager.DevModeEnabled;
		_confirmSellCheck.ButtonPressed = DisplaySettingsManager.ConfirmSellEnabled;
		_dragSpeed = settings.CameraDragSpeed;
		_dragSpeedSlider.Value = _dragSpeed;
		_musicVolume = settings.MusicVolume;
		_sfxVolume = settings.SfxVolume;
		_musicVolumeSlider.Value = _musicVolume * VolumeMaxPercent;
		_sfxVolumeSlider.Value = _sfxVolume * VolumeMaxPercent;
		UpdateDragSpeedLabel();
		UpdateVolumeLabels();
		UpdateResolutionEnabled();
		_syncing = false;
	}

	private void OnWindowModeSelected(long _)
	{
		UpdateResolutionEnabled();
	}

	private void UpdateResolutionEnabled()
	{
		bool windowed = (DisplayWindowMode)_windowModeOption.GetSelectedId() == DisplayWindowMode.Windowed;
		_resolutionOption.Disabled = !windowed;
	}

	private void OnDragSpeedChanged(double value)
	{
		_dragSpeed = (float)value;
		UpdateDragSpeedLabel();
		Sfx.Play(SfxEvents.SliderChange);
	}

	private void OnMusicVolumeChanged(double value)
	{
		_musicVolume = (float)value / VolumeMaxPercent;
		UpdateVolumeLabels();
		if (_syncing)
			return;

		AudioManager.Instance?.SetMusicVolume(_musicVolume);
		Sfx.Play(SfxEvents.SliderChange);
	}

	private void OnSfxVolumeChanged(double value)
	{
		_sfxVolume = (float)value / VolumeMaxPercent;
		UpdateVolumeLabels();
		if (_syncing)
			return;

		AudioManager.Instance?.SetSfxVolume(_sfxVolume);
		Sfx.Play(SfxEvents.SliderChange);
	}

	private void OnVolumeDragEnded(bool valueChanged)
	{
		if (!valueChanged)
			return;

		DisplaySettingsManager settings = AdapterRegistry.Resolve<DisplaySettingsManager>();
		settings.SetAudioVolumes(_musicVolume, _sfxVolume);
	}

	private void UpdateDragSpeedLabel()
	{
		_dragSpeedValueLabel.Text = $"{_dragSpeed:0.0}×";
	}

	private void UpdateVolumeLabels()
	{
		_musicVolumeValueLabel.Text = $"{Mathf.RoundToInt(_musicVolume * VolumeMaxPercent)}%";
		_sfxVolumeValueLabel.Text = $"{Mathf.RoundToInt(_sfxVolume * VolumeMaxPercent)}%";
	}

	private void OnApplyPressed()
	{
		DisplayWindowMode mode = (DisplayWindowMode)_windowModeOption.GetSelectedId();
		Vector2I resolution = DisplaySettingsManager.OutputResolutions[_resolutionOption.Selected];
		DisplaySettingsManager.DevModeEnabled = _devModeCheck.ButtonPressed;
		DisplaySettingsManager.ConfirmSellEnabled = _confirmSellCheck.ButtonPressed;
		DisplaySettingsManager settings = AdapterRegistry.Resolve<DisplaySettingsManager>();
		settings.SaveAndApply(mode, resolution, _dragSpeed, _musicVolume, _sfxVolume);
		AudioManager.Instance?.SetMusicVolume(_musicVolume);
		AudioManager.Instance?.SetSfxVolume(_sfxVolume);
	}

	private void OnBackPressed() => Close();
}