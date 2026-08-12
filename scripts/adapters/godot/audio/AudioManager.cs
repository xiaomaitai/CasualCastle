// 音频管理器：统一接管 BGM 曲目池轮播、SFX 事件播放与 Music/SFX 总线音量。
using CasualCastle.Adapters.Godot;
using CasualCastle.Adapters.Persistence;
using CasualCastle.Domain.Building;
using CasualCastle.Domain.Shared;
using Godot;
using System;
using System.Collections.Generic;

public partial class AudioManager : Node
{
	public static AudioManager Instance { get; private set; }

	private const string MusicBusName = "Music";
	private const string SfxBusName = "SFX";
	private const float BgmBaseVolumeDb = -8f;
	private const float SfxBaseVolumeDb = -5f;
	private const float MuteVolumeDb = -60f;
	private const float CrossfadeSeconds = 1.0f;
	private const double MinGapSeconds = 3.0;
	private const double MaxGapSeconds = 6.0;
	private const ulong SfxDedupeMs = 50;

	private readonly Dictionary<string, List<BgmDef>> _bgmPoolDefs = new();

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

	public BgmDef CurrentBgm { get; private set; }

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

		BuildBgmPools();
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
		PlayPool(BgmPoolIds.Title);
	}

	public void UpdateGameplayMusic()
	{
		if (_gameManager == null)
			return;

		bool isHorde = _gameManager.SelectedFaction == FactionDef.HordeId;
		bool isNightMusic = _gameManager.IsNightDisplayed;
		string poolKey = isHorde
			? isNightMusic ? BgmPoolIds.HordeNight : BgmPoolIds.HordeDay
			: isNightMusic ? BgmPoolIds.AllianceNight : BgmPoolIds.AllianceDay;
		PlayPool(poolKey);
	}

	public void PlayResultMusic()
	{
		PlayPool(BgmPoolIds.Result);
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
		List<BgmDef> tracks = SelectPoolTracks(poolKey);
		int count = tracks.Count;
		if (count == 0)
			return 0;

		if (!_shuffleEnabled)
			return (_nextTrackIndex + 1) % count;

		int candidate = Random.Shared.Next(count);
		if (count > 1 && candidate == _nextTrackIndex)
			candidate = (candidate + 1) % count;
		return candidate;
	}

	private AudioStream LoadBgmStream(string poolKey, int index)
	{
		List<BgmDef> tracks = SelectPoolTracks(poolKey);
		if (index < 0 || index >= tracks.Count)
			return null;

		BgmDef def = tracks[index];
		if (_bgmStreams.TryGetValue(def.Uid, out AudioStream cached))
		{
			CurrentBgm = def;
			return cached;
		}

		AudioStream stream = GD.Load<AudioStream>(def.Uid);
		if (stream == null)
			return null;

		_bgmStreams[def.Uid] = stream;
		CurrentBgm = def;
		return stream;
	}

	private List<BgmDef> SelectPoolTracks(string poolKey)
	{
		if (!_bgmPoolDefs.TryGetValue(poolKey, out List<BgmDef> poolDefs))
			return new List<BgmDef>();

		int nightIndex = _gameManager == null ? 0 : _gameManager.CurrentNightIndex;
		return BgmSelectionRules.SelectTracks(poolDefs, poolKey, nightIndex);
	}

	private void BuildBgmPools()
	{
		foreach (BgmDef def in GameDataLoader.BgmCatalog)
		{
			if (!_bgmPoolDefs.TryGetValue(def.Pool, out List<BgmDef> pool))
			{
				pool = new List<BgmDef>();
				_bgmPoolDefs[def.Pool] = pool;
			}

			pool.Add(def);
		}
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

	private void OnGamePhaseChanged(MatchPhase phase)
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
