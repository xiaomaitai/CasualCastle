// 魔王已习得技能集：记录升级选择的技能与习得等级
using System.Collections.Generic;

namespace CasualCastle.Domain.Battle;

public sealed class KingSkillSet
{
	private readonly List<string> _skillIds = new();
	private readonly Dictionary<string, int> _learnLevels = new();

	public int Count => _skillIds.Count;

	public bool Contains(string skillId)
	{
		return _skillIds.Contains(skillId);
	}

	public void Add(string skillId)
	{
		Add(skillId, 1);
	}

	public void Add(string skillId, int level)
	{
		if (string.IsNullOrEmpty(skillId) || _skillIds.Contains(skillId))
			return;
		_skillIds.Add(skillId);
		_learnLevels[skillId] = level;
	}

	public int GetLearnLevel(string skillId)
	{
		if (_learnLevels.TryGetValue(skillId, out int level))
			return level;
		return 1;
	}

	public IReadOnlyList<string> GetAll()
	{
		return _skillIds.ToArray();
	}

	public void Reset()
	{
		_skillIds.Clear();
		_learnLevels.Clear();
	}
}