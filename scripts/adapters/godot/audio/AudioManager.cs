// 音频管理器：统一接管 BGM 曲目池轮播、SFX 事件播放与 Music/SFX 总线音量。
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Building;
using Godot;
using System;
using System.Collections.Generic;

public partial class AudioManager : Node
{
	public static AudioManager Instance { get; private set; }

	private const string MusicBusName = "Music";
	private const string SfxBusName = "SFX";
	private const string TitlePool = "title";
	private const string AllianceDayPool = "alliance_day";
	private const string AllianceNightPool = "alliance_night";
	private const string HordeDayPool = "horde_day";
	private const string HordeNightPool = "horde_night";
	private const string ResultPool = "result";
	private const float BgmBaseVolumeDb = -8f;
	private const float SfxBaseVolumeDb = -5f;
	private const float MuteVolumeDb = -60f;
	private const float CrossfadeSeconds = 1.0f;
	private const double MinGapSeconds = 3.0;
	private const double MaxGapSeconds = 6.0;
	private const ulong SfxDedupeMs = 50;

	private static readonly string[] TitleTracks =
	{
		"res://assets/audio/bgm/bgm_title_1.ogg",
		"res://assets/audio/bgm/bgm_title_2.ogg",
		"res://assets/audio/bgm/bgm_title_3.ogg",
	};

	private static readonly string[] AllianceDayTracks =
	{
		"res://assets/audio/bgm/bgm_alliance_day_1.ogg",
		"res://assets/audio/bgm/bgm_alliance_day_2.ogg",
		"res://assets/audio/bgm/bgm_alliance_day_3.ogg",
	};

	private static readonly string[] AllianceNightTracks =
	{
		"res://assets/audio/bgm/bgm_alliance_night_1.ogg",
		"res://assets/audio/bgm/bgm_alliance_night_2.ogg",
	};

	private static readonly string[] HordeDayTracks =
	{
		"res://assets/audio/bgm/bgm_horde_day_1.ogg",
		"res://assets/audio/bgm/bgm_horde_day_2.ogg",
		"res://assets/audio/bgm/bgm_horde_day_3.ogg",
	};

	private static readonly string[] HordeNightTracks =
	{
		"res://assets/audio/bgm/bgm_horde_night_1.ogg",
		"res://assets/audio/bgm/bgm_horde_night_2.ogg",
	};

	private static readonly string[] ResultTracks =
	{
		"res://assets/audio/bgm/bgm_result_1.ogg",
		"res://assets/audio/bgm/bgm_result_2.ogg",
	};

	private readonly Dictionary<string, string[]> _bgmPools = new()
	{
		[TitlePool] = TitleTracks,
		[AllianceDayPool] = AllianceDayTracks,
		[AllianceNightPool] = AllianceNightTracks,
		[HordeDayPool] = HordeDayTracks,
		[HordeNightPool] = HordeNightTracks,
		[ResultPool] = ResultTracks,
	};

	private readonly Dictionary<string, AudioStream> _bgmStreams = new();
	private readonly Dictionary<string, AudioStream> _sfxStreams = new();
	private readonly Dictionary<string, AudioStreamPlayer> _sfxPlayers = new();
	private readonly Dictionary<string, ulong> _lastSfxTick = new();

	private AudioStreamPlayer _musicA;
	private AudioStreamPlayer _musicB;
	private AudioStreamPlayer _musicActive;
	private Tween _musicTweenA;
	private Tween _musicTweenB;
	private Timer _gapTimer;
	private GameManager _gameManager;
	private string _currentPool = "";
	private int _nextTrackIndex;
	private bool _shuffleEnabled = true;

	public bool ShuffleEnabled
	{
		get => _shuffleEnabled;
		set => _shuffleEnabled = value;
	}

	public override void _Ready()
	{
		Instance = this;
		AdapterRegistry.Register<AudioManager>(this);
		_musicA = CreateMusicPlayer();
		_musicB = CreateMusicPlayer();
		_gapTimer = new Timer();
		_gapTimer.OneShot = true;
		AddChild(_gapTimer);
		_gapTimer.Timeout += OnGapTimeout;
		_gameManager = AdapterRegistry.Resolve<GameManager>();
		if (_gameManager != null)
		{
			_gameManager.PhaseChanged += OnGamePhaseChanged;
			_gameManager.GameStateChanged += OnGameStateChanged;
		}

		ApplyVolumesFromSettings();
	}

	public override void _ExitTree()
	{
		if (_gameManager != null)
		{
			_gameManager.PhaseChanged -= OnGamePhaseChanged;
			_gameManager.GameStateChanged -= OnGameStateChanged;
		}

		if (Instance == this)
		{
			AdapterRegistry.Unregister<AudioManager>(this);
			Instance = null;
		}
	}

	public void PlayTitleMusic()
	{
		PlayPool(TitlePool);
	}

	public void UpdateGameplayMusic()
	{
		if (_gameManager == null)
			return;

		bool isHorde = _gameManager.SelectedFaction == FactionDef.HordeId;
		bool isNight = _gameManager.CurrentPhase != GameManager.GamePhase.Day;
		string poolKey = isHorde
			? isNight ? HordeNightPool : HordeDayPool
			: isNight ? AllianceNightPool : AllianceDayPool;
		PlayPool(poolKey);
	}

	public void PlayResultMusic()
	{
		PlayPool(ResultPool);
	}

	public void SetShuffleEnabled(bool enabled)
	{
		_shuffleEnabled = enabled;
	}

	public void PlaySfx(string eventName)
	{
		if (string.IsNullOrEmpty(eventName))
			return;

		ulong now = Time.GetTicksMsec();
		if (_lastSfxTick.TryGetValue(eventName, out ulong last) && now - last < SfxDedupeMs)
			return;

		_lastSfxTick[eventName] = now;

		AudioStream stream = LoadSfxStream(eventName);
		if (stream == null)
			return;

		AudioStreamPlayer player = GetSfxPlayer(eventName);
		player.Stream = stream;
		player.Play();
	}

	public void SetMusicVolume(float volume)
	{
		SetBusVolume(MusicBusName, volume);
	}

	public void SetSfxVolume(float volume)
	{
		SetBusVolume(SfxBusName, volume);
	}

	private void PlayPool(string poolKey)
	{
		if (poolKey == _currentPool && _musicActive != null)
			return;

		_currentPool = poolKey;
		_gapTimer.Stop();
		_nextTrackIndex = PickNextTrackIndex(poolKey);
		CrossfadeToTrack(LoadBgmStream(poolKey, _nextTrackIndex));
	}

	private void CrossfadeToTrack(AudioStream stream)
	{
		AudioStreamPlayer previous = _musicActive;
		AudioStreamPlayer next = previous == _musicA ? _musicB : _musicA;

		KillTween(_musicTweenA);
		KillTween(_musicTweenB);
		_musicTweenA = null;
		_musicTweenB = null;

		if (stream == null)
		{
			_musicActive = null;
			return;
		}

		next.Stream = stream;
		next.VolumeDb = MuteVolumeDb;
		next.Play();

		if (previous != null && previous.Playing)
		{
			Tween fadeOut = CreateTween();
			fadeOut.TweenProperty(previous, "volume_db", MuteVolumeDb, CrossfadeSeconds);
			fadeOut.TweenCallback(Callable.From(previous.Stop));
			AssignMusicTween(previous, fadeOut);
		}

		Tween fadeIn = CreateTween();
		fadeIn.TweenProperty(next, "volume_db", BgmBaseVolumeDb, CrossfadeSeconds);
		AssignMusicTween(next, fadeIn);
		_musicActive = next;
	}

	private void AssignMusicTween(AudioStreamPlayer player, Tween tween)
	{
		if (player == _musicA)
			_musicTweenA = tween;
		else
			_musicTweenB = tween;
	}

	private void OnTrackFinished(AudioStreamPlayer player)
	{
		if (player != _musicActive)
			return;

		double gapSeconds = Random.Shared.NextDouble() * (MaxGapSeconds - MinGapSeconds) + MinGapSeconds;
		_gapTimer.WaitTime = gapSeconds;
		_gapTimer.Start();
	}

	private void OnGapTimeout()
	{
		if (_currentPool == "")
			return;

		_nextTrackIndex = PickNextTrackIndex(_currentPool);
		AudioStream stream = LoadBgmStream(_currentPool, _nextTrackIndex);
		if (stream == null || _musicActive == null)
			return;

		_musicActive.Stream = stream;
		_musicActive.VolumeDb = BgmBaseVolumeDb;
		_musicActive.Play();
	}

	private int PickNextTrackIndex(string poolKey)
	{
		int count = _bgmPools[poolKey].Length;
		if (!_shuffleEnabled)
			return (_nextTrackIndex + 1) % count;

		int candidate = Random.Shared.Next(count);
		if (count > 1 && candidate == _nextTrackIndex)
			candidate = (candidate + 1) % count;
		return candidate;
	}

	private AudioStream LoadBgmStream(string poolKey, int index)
	{
		string path = _bgmPools[poolKey][index];
		if (_bgmStreams.TryGetValue(path, out AudioStream cached))
			return cached;

		AudioStream stream = GD.Load<AudioStream>(path);
		_bgmStreams[path] = stream;
		return stream;
	}

	private AudioStream LoadSfxStream(string eventName)
	{
		if (_sfxStreams.TryGetValue(eventName, out AudioStream cached))
			return cached;

		AudioStream stream = GD.Load<AudioStream>($"res://assets/audio/sfx/sfx_{eventName}.ogg");
		_sfxStreams[eventName] = stream;
		return stream;
	}

	private AudioStreamPlayer GetSfxPlayer(string eventName)
	{
		if (_sfxPlayers.TryGetValue(eventName, out AudioStreamPlayer player))
			return player;

		AudioStreamPlayer newPlayer = new AudioStreamPlayer();
		newPlayer.Bus = SfxBusName;
		newPlayer.VolumeDb = SfxBaseVolumeDb;
		AddChild(newPlayer);
		_sfxPlayers[eventName] = newPlayer;
		return newPlayer;
	}

	private AudioStreamPlayer CreateMusicPlayer()
	{
		AudioStreamPlayer player = new AudioStreamPlayer();
		player.Bus = MusicBusName;
		player.VolumeDb = BgmBaseVolumeDb;
		AddChild(player);
		player.Finished += () => OnTrackFinished(player);
		return player;
	}

	private static void KillTween(Tween tween)
	{
		if (tween != null && tween.IsValid())
			tween.Kill();
	}

	private void OnGamePhaseChanged(GameManager.GamePhase phase)
	{
		if (_gameManager != null && _gameManager.CurrentState == GameManager.GameState.GameOver)
			return;

		UpdateGameplayMusic();
	}

	private void OnGameStateChanged(GameManager.GameState state)
	{
		if (state == GameManager.GameState.GameOver)
			PlayResultMusic();
	}

	private void ApplyVolumesFromSettings()
	{
		DisplaySettingsManager settings = AdapterRegistry.Resolve<DisplaySettingsManager>();
		if (settings == null)
			return;

		SetMusicVolume(settings.MusicVolume);
		SetSfxVolume(settings.SfxVolume);
	}

	private static void SetBusVolume(string busName, float volume)
	{
		int busIndex = AudioServer.GetBusIndex(busName);
		if (busIndex < 0)
			return;

		float clamped = Mathf.Clamp(volume, 0f, 1f);
		float db = clamped <= 0f ? MuteVolumeDb : Mathf.LinearToDb(clamped);
		AudioServer.SetBusVolumeDb(busIndex, db);
	}
}
