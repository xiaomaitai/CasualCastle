// 冒烟测试基类：看门狗、失败收集、安全节点访问与统一报告
using Godot;
using System;
using System.Collections.Generic;

public partial class SmokeTestBase : Node
{
	private readonly List<string> _failures = new();
	private bool _reported;

	public void StartWatchdog(float seconds)
	{
		GetTree().CreateTimer(seconds).Timeout += () =>
		{
			if (_reported)
				return;
			GD.Print("SMOKE: FAIL 看门狗超时，冒烟未在限定时间内退出");
			GetTree().Quit(1);
		};
	}

	public void Fail(string message)
	{
		_failures.Add(message);
	}

	public T RequireNode<T>(Node root, string path) where T : Node
	{
		T node = root.GetNodeOrNull<T>(path);
		if (node == null)
		{
			Fail($"节点缺失 {path}（节点迁移后请更新路径或改用公开入口）");
			ReportAndQuit();
			throw new Exception($"节点缺失 {path}");
		}
		return node;
	}

	public void RunGuarded(Action step)
	{
		try
		{
			step();
		}
		catch (Exception ex)
		{
			Fail($"步骤异常 {ex.GetType().Name}: {ex.Message}");
			ReportAndQuit();
		}
	}

	public void ReportAndQuit()
	{
		if (_reported)
			return;
		_reported = true;
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
