// SFX 静态入口：供任意适配器代码快速播放命名音效事件。
public static class Sfx
{
	public static void Play(string eventName)
	{
		AudioManager.Instance?.PlaySfx(eventName);
	}
}