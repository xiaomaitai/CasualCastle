using Godot;
using System;

public partial class Barracks : Node2D
{
    [Export]
    public float SpawnInterval = 5.0f;

    public bool IsPlayerBarracks { get; private set; }

    private Timer _spawnTimer;
    private bool _isActive = true;
    private Node2D _battlefield;

    public override void _Ready()
    {
        // 根据父节点名称判断阵营
        Node2D parent = GetParent() as Node2D;
        if (parent != null && parent.Name == "EnemySide")
        {
            IsPlayerBarracks = false;
        }
        else
        {
            IsPlayerBarracks = true;
        }

        // 创建产兵计时器
        _spawnTimer = new Timer();
        _spawnTimer.WaitTime = SpawnInterval;
        _spawnTimer.Autostart = true;
        _spawnTimer.OneShot = false;
        _spawnTimer.Connect("timeout", Callable.From(SpawnUnit));
        AddChild(_spawnTimer);

        // 获取战场引用
        _battlefield = GetParent() as Node2D;
        if (_battlefield == null)
        {
            _battlefield = GetTree().Root.GetNodeOrNull<Node2D>("MainGame/Battlefield");
        }
    }

    private void SpawnUnit()
    {
        if (!_isActive || _battlefield == null) return;

        // 士兵预制体将在阶段2.2创建，这里先检查是否存在
        PackedScene soldierScene = GD.Load<PackedScene>("res://prefabs/Soldier.tscn");
        if (soldierScene == null) return;

        // 根据阵营决定生成位置
        Vector2 spawnPosition = IsPlayerBarracks
            ? GlobalPosition + new Vector2(50, 0)
            : GlobalPosition + new Vector2(-50, 0);

        Soldier soldier = soldierScene.Instantiate<Soldier>();
        soldier.GlobalPosition = spawnPosition;
        soldier.IsPlayerUnit = IsPlayerBarracks;
        _battlefield.AddChild(soldier);
    }

    public void SetActive(bool active)
    {
        _isActive = active;
        if (_spawnTimer != null)
        {
            if (active)
            {
                _spawnTimer.Start();
            }
            else
            {
                _spawnTimer.Stop();
            }
        }
    }
}