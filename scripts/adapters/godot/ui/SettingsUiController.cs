// 设置面板组件：管理显示、游戏界面风格、操作与音频选项（settings_panel.tscn 场景根）
using CasualCastle.Adapters.Godot;
using Godot;
using System;

public partial class SettingsUiController : Control
{
	private const float VolumeMinPercent = 0f;
	private const float VolumeMaxPercent = 100f;
	private const float VolumeStep = 1f;

	private OptionButton _windowModeOption;
	private OptionButton _resolutionOption;
	private OptionButton _uiProfileOption;
	private CheckBox _devModeCheck;
	private CheckBox _confirmSellCheck;
	private CheckBox _autoPauseNightCheck;
	private CheckBox _autoResumePlaceCheck;
	private HSlider _dragSpeedSlider;
	private Label _dragSpeedValueLabel;
	private HSlider _musicVolumeSlider;
	private Label _musicVolumeValueLabel;
	private HSlider _sfxVolumeSlider;
	private Label _sfxVolumeValueLabel;
	private Button _applyButton;
	private Button _backButton;

	public bool IsOpen { get; private set; }
	public event Action<bool> OpenChanged;
	private float _dragSpeed = DisplaySettingsManager.DefaultDragSpeed;
	private float _musicVolume = DisplaySettingsManager.DefaultMusicVolume;
	private float _sfxVolume = DisplaySettingsManager.DefaultSfxVolume;
	private bool _syncing;

	public override void _Ready()
	{
		_windowModeOption = GetNode<OptionButton>("Panel/WindowModeOption");
		_resolutionOption = GetNode<OptionButton>("Panel/ResolutionOption");
		_uiProfileOption = GetNode<OptionButton>("Panel/UiProfileOption");
		_devModeCheck = GetNode<CheckBox>("Panel/DevModeCheck");
		_confirmSellCheck = GetNode<CheckBox>("Panel/ConfirmSellCheck");
		_autoPauseNightCheck = GetNode<CheckBox>("Panel/AutoPauseNightCheck");
		_autoResumePlaceCheck = GetNode<CheckBox>("Panel/AutoResumePlaceCheck");
		_dragSpeedSlider = GetNode<HSlider>("Panel/DragSpeedSlider");
		_dragSpeedValueLabel = GetNode<Label>("Panel/DragSpeedValueLabel");
		_musicVolumeSlider = GetNode<HSlider>("Panel/MusicVolumeSlider");
		_musicVolumeValueLabel = GetNode<Label>("Panel/MusicVolumeValueLabel");
		_sfxVolumeSlider = GetNode<HSlider>("Panel/SfxVolumeSlider");
		_sfxVolumeValueLabel = GetNode<Label>("Panel/SfxVolumeValueLabel");
		_applyButton = GetNode<Button>("Panel/ApplyButton");
		_backButton = GetNode<Button>("Panel/BackButton");

		_windowModeOption.Clear();
		_windowModeOption.AddItem("窗口化", (int)DisplayWindowMode.Windowed);
		_windowModeOption.AddItem("无边框全屏", (int)DisplayWindowMode.BorderlessFullscreen);

		_resolutionOption.Clear();
		foreach (Vector2I resolution in DisplaySettingsManager.OutputResolutions)
			_resolutionOption.AddItem($"{resolution.X} × {resolution.Y}", _resolutionOption.ItemCount);

		_uiProfileOption.Clear();
		_uiProfileOption.AddItem("经典详细", (int)UiProfileKind.Detailed);
		_uiProfileOption.AddItem("粗描边动画", (int)UiProfileKind.Cartoon);

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

		Visible = false;
		AdapterRegistry.Register<SettingsUiController>(this);
	}

	public override void _ExitTree()
	{
		_windowModeOption.ItemSelected -= OnWindowModeSelected;
		_dragSpeedSlider.ValueChanged -= OnDragSpeedChanged;
		_musicVolumeSlider.ValueChanged -= OnMusicVolumeChanged;
		_musicVolumeSlider.DragEnded -= OnVolumeDragEnded;
		_sfxVolumeSlider.ValueChanged -= OnSfxVolumeChanged;
		_sfxVolumeSlider.DragEnded -= OnVolumeDragEnded;
		_applyButton.Pressed -= OnApplyPressed;
		_backButton.Pressed -= OnBackPressed;
		AdapterRegistry.Unregister<SettingsUiController>(this);
	}

	public void Open()
	{
		if (IsOpen)
			return;

		IsOpen = true;
		Visible = true;
		SyncFromSaved();
		OpenChanged?.Invoke(true);
	}

	public bool Close()
	{
		if (!IsOpen)
			return false;

		IsOpen = false;
		Visible = false;
		OpenChanged?.Invoke(false);
		return true;
	}

	private void SyncFromSaved()
	{
		_syncing = true;
		DisplaySettingsManager settings = AdapterRegistry.Resolve<DisplaySettingsManager>();
		_windowModeOption.Select(_windowModeOption.GetItemIndex((int)settings.WindowMode));
		_resolutionOption.Select(settings.FindResolutionIndex(settings.OutputResolution));
		_uiProfileOption.Select(_uiProfileOption.GetItemIndex((int)settings.UiProfile));
		_devModeCheck.ButtonPressed = DisplaySettingsManager.DevModeEnabled;
		_confirmSellCheck.ButtonPressed = DisplaySettingsManager.ConfirmSellEnabled;
		_autoPauseNightCheck.ButtonPressed = DisplaySettingsManager.AutoPauseNightEnabled;
		_autoResumePlaceCheck.ButtonPressed = DisplaySettingsManager.AutoResumeAfterPlaceEnabled;
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
		DisplaySettingsManager.AutoPauseNightEnabled = _autoPauseNightCheck.ButtonPressed;
		DisplaySettingsManager.AutoResumeAfterPlaceEnabled = _autoResumePlaceCheck.ButtonPressed;
		UiProfileKind profile = (UiProfileKind)_uiProfileOption.GetSelectedId();
		DisplaySettingsManager settings = AdapterRegistry.Resolve<DisplaySettingsManager>();
		settings.SaveAndApply(mode, resolution, _dragSpeed, _musicVolume, _sfxVolume, profile);
		AudioManager.Instance?.SetMusicVolume(_musicVolume);
		AudioManager.Instance?.SetSfxVolume(_sfxVolume);
	}

	private void OnBackPressed() => Close();
}