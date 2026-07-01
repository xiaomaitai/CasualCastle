using CasualCastle.Domain.History;
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public class BattleReportStorage : IBattleReportRepository
{
	private const string ReportDir = "user://battle_reports";
	private const string ReportFile = "user://battle_reports/reports.json";

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
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

	public string BuildDefaultName(DateTimeOffset now)
	{
		return $"战报 {now:yyyy-MM-dd HH:mm:ss}";
	}

	private static void EnsureDirectory()
	{
		DirAccess.MakeDirRecursiveAbsolute(ReportDir);
	}
}
