// BGM 定义：编号、曲名、风格、曲目池与 Godot 资源 uid
namespace CasualCastle.Domain.Shared;

public sealed class BgmDef
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public BgmStyle Style { get; set; }
	public string Pool { get; set; } = "";
	public string Uid { get; set; } = "";
}