// 音频冒烟测试：无头验证 BGM/SFX 资源、音频总线与 AudioManager 运行路径。
using CasualCastle.Adapters.Godot;
using CasualCastle.Adapters.Persistence;
using CasualCastle.Domain.Shared;
using Godot;
using System.Collections.Generic;

public partial class AudioSmoke : Node
{
	private const string SfxRoot = "res://assets/audio/sfx/";
	private const string MusicBusName = "Music";
	private const string SfxBusName = "SFX";
	private const float CheckVolume = 0.5f;
	private const float VolumeTolerance = 0.01f;


	private static readonly string[] SfxEventNames =
	{
		SfxEvents.ButtonClick,
		SfxEvents.UiStart,
		SfxEvents.SliderChange,
		SfxEvents.UnitSelect,
		SfxEvents.BuildingSelect,
		SfxEvents.ShopOpen,
		SfxEvents.ShopClose,
		SfxEvents.Buy,
		SfxEvents.CoinChange,
		SfxEvents.Place,
		SfxEvents.PlaceCancel,
		SfxEvents.Repair,
		SfxEvents.Combine,
		SfxEvents.Upgrade,
		SfxEvents.MeleeAttack,
		SfxEvents.MeleeSlash,
		SfxEvents.MeleePierce,
		SfxEvents.ProjectileLaunch,
		SfxEvents.LaunchArrow,
		SfxEvents.LaunchMagic,
		SfxEvents.ProjectileHit,
		SfxEvents.Hit,
		SfxEvents.HitSlash,
		SfxEvents.HitPierce,
		SfxEvents.HitMagic,
		SfxEvents.UnitDeath,
		SfxEvents.BuildingHit,
		SfxEvents.BuildingDestroy,
		SfxEvents.PhaseChange,
		SfxEvents.Victory,
		SfxEvents.Defeat,
	};

	private readonly List<string> _failures = new();

	public override void _Ready()
	{
		CheckBuses();
		CheckResources();
		CheckAudioManager();
		ReportAndQuit();
	}

	private void CheckBuses()
	{
		if (AudioServer.GetBusIndex(MusicBusName) < 0)
			_failures.Add($"缺少音频总线 {MusicBusName}");
		if (AudioServer.GetBusIndex(SfxBusName) < 0)
			_failures.Add($"缺少音频总线 {SfxBusName}");
		GD.Print("SMOKE: buses ok");
	}

	private void CheckResources()
	{
		foreach (BgmDef def in GameDataLoader.BgmCatalog)
		{
			if (GD.Load<AudioStream>(def.Uid) == null)
				_failures.Add($"BGM 资源缺失 {def.Name}（{def.Uid}）");
		}

		foreach (string eventName in SfxEventNames)
		{
			if (GD.Load<AudioStream>($"{SfxRoot}sfx_{eventName}.ogg") == null)
				_failures.Add($"SFX 资源缺失 {eventName}");
		}

		GD.Print("SMOKE: resources ok");
	}

	private void CheckAudioManager()
	{
		AudioManager audio = AdapterRegistry.Resolve<AudioManager>();
		if (audio == null)
		{
			_failures.Add("AudioManager 未注册");
			return;
		}

		audio.PlayTitleMusic();
		audio.UpdateGameplayMusic();
		audio.PlayResultMusic();
		audio.PlaySfx(SfxEvents.Buy);
		audio.PlaySfx(SfxEvents.Victory);
		audio.PlaySfx(SfxEvents.ButtonClick);
		audio.SetMusicVolume(CheckVolume);
		audio.SetSfxVolume(CheckVolume);
		audio.SetShuffleEnabled(false);

		int musicBus = AudioServer.GetBusIndex(MusicBusName);
		float expectedDb = Mathf.LinearToDb(CheckVolume);
		if (Mathf.Abs(AudioServer.GetBusVolumeDb(musicBus) - expectedDb) > VolumeTolerance)
			_failures.Add("音乐总线音量写入未生效");
		GD.Print("SMOKE: audio manager ok");
	}

	private void ReportAndQuit()
	{
		if (_failures.Count == 0)
		{
			GD.Print("SMOKE: PASS");
			GetTree().Quit(0);
			return;
		}

		foreach (string failure in _failures)
			GD.Print($"SMOKE: FAIL {failure}");
		GetTree().Quit(1);
	}
}
