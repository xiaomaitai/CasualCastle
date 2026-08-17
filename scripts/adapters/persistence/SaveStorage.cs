// 存档存储：Godot 侧入口，解析 user:// 目录后委托纯 C# 存档数据库
using CasualCastle.Domain.Shared;
using System.Collections.Generic;

using GodotProjectSettings = Godot.ProjectSettings;

namespace CasualCastle.Adapters.Persistence;

public class SaveStorage : ISaveRepository
{
	private readonly SaveDatabase _database;

	public SaveStorage()
	{
		_database = new SaveDatabase(GodotProjectSettings.GlobalizePath("user://saves"));
	}

	public void Save(SaveData data)
	{
		_database.Save(data);
	}

	public SaveData Load(int slotIndex)
	{
		return _database.Load(slotIndex);
	}

	public bool HasSave(int slotIndex)
	{
		return _database.HasSave(slotIndex);
	}

	public void DeleteSave(int slotIndex)
	{
		_database.DeleteSave(slotIndex);
	}

	public List<int> ListSlots()
	{
		return _database.ListSlots();
	}
}
