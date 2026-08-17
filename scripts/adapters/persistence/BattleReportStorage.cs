// 战报持久化：JSON 读写用户目录战报
using CasualCastle.Domain.History;
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CasualCastle.Adapters.Persistence;

public class BattleReportStorage : IBattleReportRepository
{
	private const string ReportDir = "user://battle_reports";
	private const string ReportFile = "user://battle_reports/reports.json";

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
		Converters = { new JsonStringEnumConverter() },
	};

	public List<BattleReport> LoadAll()
	{
		EnsureDirectory();
		if (!FileAccess.FileExists(ReportFile))
			return new List<BattleReport>();

		using FileAccess file = FileAccess.Open(ReportFile, FileAccess.ModeFlags.Read);
		if (file == null)
			return new List<BattleReport>();

		string json = file.GetAsText();
		if (string.IsNullOrWhiteSpace(json))
			return new List<BattleReport>();

		return JsonSerializer.Deserialize<List<BattleReport>>(json, JsonOptions) ?? new List<BattleReport>();
	}

	public void SaveAll(IReadOnlyList<BattleReport> reports)
	{
		EnsureDirectory();
		string json = JsonSerializer.Serialize(reports, JsonOptions);

		using FileAccess file = FileAccess.Open(ReportFile, FileAccess.ModeFlags.Write);
		file.StoreString(json);
	}

	private static void EnsureDirectory()
	{
		DirAccess.MakeDirRecursiveAbsolute(ReportDir);
	}
}
