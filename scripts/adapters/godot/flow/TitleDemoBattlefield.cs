// 标题演示战场：两侧自动出兵在中间交战，摄像机缓慢偏移
using CasualCastle.Adapters.Godot;
using CasualCastle.Domain.Battle;
using Godot;
using System;

public partial class TitleDemoBattlefield : Node2D
{
	private const string CastlePrefabPath = "res://prefabs/Castle.tscn";
	private const float SpawnIntervalSeconds = 2.5f;
	private const int UnitsPerWave = 2;
	private const float BattleLineY = 1610f;
	private const float PlayerSpawnX = 260f;
	private const float EnemySpawnX = 3340f;
	private const float PlayerPatrolX = 1470f;
	private const float EnemyPatrolX = 2430f;
	private const float PatrolRadius = 130f;
	private const float UnitOffsetY = 70f;
	private const float SpawnGap = 45f;
	private const float CameraMinX = 500f;
	private const float CameraMaxX = 3500f;
	private const float CameraStartX = 1920f;
	private const float CameraPanSpeed = 120f;
	private const float PlayerCastleX = 170f;
	private const float EnemyCastleX = 2870f;
	private const float CastleY = 1210f;

	private static readonly string[] PlayerUnits = { "OrcWarrior", "Archer" };

	private static readonly string[] EnemyUnits = { "GoblinSlinger", "Skeleton" };

	private UnitSpawnService _spawnService;
	private Camera2D _camera;
	private float _spawnTimer;
	private float _cameraX;
	private float _cameraDirection = 1f;
	private bool _demoReady;

	public bool IsReady => _demoReady;

	public override void _Ready()
	{
		try
		{
			SetupDemo();
		}
		catch (Exception)
		{
			_demoReady = false;
		}
	}

	public override void _Process(double delta)
	{
		if (!_demoReady)
			return;

		float step = (float)delta;
		_spawnTimer += step;
		if (_spawnTimer >= SpawnIntervalSeconds)
		{
			_spawnTimer = 0f;
			SpawnWave();
		}

		UpdateCamera(step);
	}

	private void SetupDemo()
	{
		AddChild(new BuildingSystem());
		_spawnService = new UnitSpawnService();
		_cameraX = CameraStartX;
		_camera = new Camera2D
		{
			Position = new Vector2(_cameraX, BattleLineY),
		};
		_camera.MakeCurrent();
		AddChild(_camera);
		AddCastle(PlayerCastleX, true);
		AddCastle(EnemyCastleX, false);
		_demoReady = true;
		GD.Print("标题演示战场已就绪");
	}

	private void AddCastle(float x, bool isPlayer)
	{
		PackedScene castleScene = GD.Load<PackedScene>(CastlePrefabPath);
		Castle castle = castleScene.Instantiate<Castle>();
		castle.IsPlayerCastle = isPlayer;
		AddChild(castle);
		castle.GlobalPosition = new Vector2(x, CastleY);
	}

	private void SpawnWave()
	{
		for (int i = 0; i < UnitsPerWave; i++)
		{
			SpawnDemoUnit(true, PlayerUnits[i % PlayerUnits.Length], PlayerSpawnX + i * SpawnGap, PlayerPatrolX);
			SpawnDemoUnit(false, EnemyUnits[i % EnemyUnits.Length], EnemySpawnX - i * SpawnGap, EnemyPatrolX);
		}
	}

	private void SpawnDemoUnit(bool isPlayer, string unitTypeId, float x, float patrolX)
	{
		_spawnService.SpawnUnitAt(this, unitTypeId, new Vector2(x, BattleLineY + UnitOffsetY), isPlayer);
		ConfigureNewestDefender(patrolX);
	}

	private void ConfigureNewestDefender(float patrolX)
	{
		for (int i = GetChildCount() - 1; i >= 0; i--)
		{
			if (GetChild(i) is not Unit shell)
				continue;
			UnitLogic logic = shell.GetNodeOrNull<UnitLogic>("Logic");
			logic?.ConfigureDefender(patrolX, BattleLineY, PatrolRadius);
			return;
		}
	}

	private void UpdateCamera(float step)
	{
		_cameraX += step * CameraPanSpeed * _cameraDirection;
		if (_cameraX >= CameraMaxX)
		{
			_cameraX = CameraMaxX;
			_cameraDirection = -1f;
		}
		else if (_cameraX <= CameraMinX)
		{
			_cameraX = CameraMinX;
			_cameraDirection = 1f;
		}

		_camera.Position = new Vector2(_cameraX, BattleLineY);
	}
}
