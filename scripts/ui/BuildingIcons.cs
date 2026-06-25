using Godot;
using System.Collections.Generic;

public static class BuildingIcons
{
	private const string PausePath = "res://assets/art/ui/building_pause_icon.png";
	private const string RepairPath = "res://assets/art/ui/building_repair_icon.png";
	private const string ProhibitPath = "res://assets/art/ui/building_prohibit_icon.png";

	private static readonly Dictionary<string, Texture2D> Cache = new();

	public static Texture2D Pause => Load(PausePath);
	public static Texture2D Repair => Load(RepairPath);
	public static Texture2D Prohibit => Load(ProhibitPath);

	private static Texture2D Load(string path)
	{
		if (Cache.TryGetValue(path, out Texture2D cached))
			return cached;

		Texture2D texture = GD.Load<Texture2D>(path);
		Cache[path] = texture;
		return texture;
	}
}
