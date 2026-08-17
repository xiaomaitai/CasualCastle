// UI 输入路由：按优先级注册式分发输入与 Esc 关闭，注册顺序即优先级（高→低）
using Godot;
using System;
using System.Collections.Generic;

public sealed class UiInputRouter
{
	private readonly List<(int Priority, Func<bool> TryClose)> _escHandlers = new();
	private readonly List<(int Priority, Func<InputEvent, bool> Handler)> _inputHandlers = new();

	public void RegisterEsc(int priority, Func<bool> tryClose)
	{
		_escHandlers.Add((priority, tryClose));
	}

	public void RegisterInput(int priority, Func<InputEvent, bool> handler)
	{
		_inputHandlers.Add((priority, handler));
	}

	public bool HandleEscape()
	{
		foreach ((int Priority, Func<bool> TryClose) entry in _escHandlers)
		{
			if (entry.TryClose())
				return true;
		}
		return false;
	}

	public bool HandleInput(InputEvent @event)
	{
		foreach ((int Priority, Func<InputEvent, bool> Handler) entry in _inputHandlers)
		{
			if (entry.Handler(@event))
				return true;
		}
		return false;
	}
}
