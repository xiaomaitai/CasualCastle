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

        string side = IsPlayerBarracks ? "玩家" : "敌方";
        GD.Print($"[Barracks _Ready] {side}兵营初始化，全局位置: {GlobalPosition}, 父节点: {GetParent().Name}");

        // 创建产兵计时器
        _spawnTimer = new Timer();
        _spawnTimer.WaitTime = SpawnInterval;
        _spawnTimer.Autostart = true;
        _spawnTimer.OneShot = false;
        _spawnTimer.Connect("timeout", Callable.From(SpawnUnit));
        AddChild(_spawnTimer);

        // 获取战场引用（通过场景树路径）
        _battlefield = GetNodeOrNull<Node2D>("/root/MainGame/Battlefield");
        if (_battlefield == null)
        {
            _battlefield = GetTree().Root.GetNodeOrNull<Node2D>("MainGame/Battlefield");
        }

        GD.Print($"[Barracks _Ready] 战场节点: {(_battlefield != null ? _battlefield.Name : "null")}, 位置: {(_battlefield?.GlobalPosition ?? Vector2.Zero)}");
    }

    private void SpawnUnit()
    {
        GD.Print($"[Barracks SpawnUnit] 开始产兵，_isActive={_isActive}, _battlefield={(_battlefield != null ? _battlefield.Name : "null")}");

        if (!_isActive || _battlefield == null) return;

        // 加载士兵预制体
        PackedScene soldierScene = GD.Load<PackedScene>("res://prefabs/Soldier.tscn");
        GD.Print($"[Barracks SpawnUnit] 加载Soldier预制体: {(soldierScene != null ? "成功" : "失败 - 文件不存在")}");
        if (soldierScene == null) return;

        // 根据阵营决定生成位置
        Vector2 spawnPosition = IsPlayerBarracks
            ? GlobalPosition + new Vector2(50, 0)
            : GlobalPosition + new Vector2(-50, 0);

        string side = IsPlayerBarracks ? "玩家" : "敌方";
        GD.Print($"[Barracks SpawnUnit] 生成{side}士兵，兵营位置: {GlobalPosition}, 生成位置: {spawnPosition}");

        Soldier soldier = soldierScene.Instantiate<Soldier>();
        soldier.GlobalPosition = spawnPosition;
        soldier.IsPlayerUnit = IsPlayerBarracks;
        _battlefield.AddChild(soldier);
        GD.Print($"[Barracks SpawnUnit] 士兵添加完成，子节点数: {_battlefield.GetChildCount()}");
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