// 流场数据：从目标格反向扩散得到的代价与方向场
namespace CasualCastle.Domain.Battle;

public sealed class FlowField
{
	public readonly float[] Cost;
	public readonly float[] DirX;
	public readonly float[] DirY;
	public readonly bool[] Reachable;

	public FlowField(int cellCount)
	{
		Cost = new float[cellCount];
		DirX = new float[cellCount];
		DirY = new float[cellCount];
		Reachable = new bool[cellCount];
	}
}
