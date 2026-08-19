// 投射物表现器：生成攻击/治疗投射物并缓存贴图
using CasualCastle.Domain.Battle;
using Godot;
using System.Collections.Generic;

namespace CasualCastle.Adapters.Godot.Battle;

public sealed class ProjectilePresenter
{
	private static readonly Dictionary<string, Texture2D> _projectileTextures = new();

	public void SpawnAttack(Node root, Vector2 fromPixels, Vector2 toPixels, AttackType attackType, DamageType damageType, bool playHitSound)
	{
		if (root == null)
			return;
		Projectile projectile = new Projectile();
		root.AddChild(projectile);
		projectile.PlayHitSound = playHitSound;
		Texture2D texture = LoadProjectileTexture(GetProjectileTexturePath(attackType, damageType));
		if (texture == null)
			return;
		float speed = attackType == AttackType.Melee ? ProjectileRules.SpeedMelee : ProjectileRules.SpeedRanged;
		projectile.Launch(fromPixels, toPixels, speed, texture);
	}

	public void SpawnHeal(Node root, Vector2 fromPixels, Vector2 toPixels)
	{
		if (root == null)
			return;
		Projectile projectile = new Projectile();
		root.AddChild(projectile);
		projectile.SetModulate(new Color(0.5f, 1f, 0.6f));
		projectile.PlayHitSound = false;
		Texture2D texture = LoadProjectileTexture("res://assets/art/projectiles/magic_ball.png");
		if (texture == null)
			return;
		projectile.Launch(fromPixels, toPixels, ProjectileRules.SpeedHeal, texture);
	}

	private static string GetProjectileTexturePath(AttackType attackType, DamageType damageType)
	{
		if (attackType == AttackType.Ranged)
		{
			if (damageType == DamageType.Magic)
				return "res://assets/art/projectiles/magic_ball.png";
			return "res://assets/art/projectiles/arrow.png";
		}

		if (damageType == DamageType.Pierce)
			return "res://assets/art/projectiles/spear.png";
		return "res://assets/art/projectiles/sword.png";
	}

	private static Texture2D LoadProjectileTexture(string path)
	{
		if (_projectileTextures.TryGetValue(path, out Texture2D cached))
			return cached;

		Texture2D texture = GD.Load<Texture2D>(path);
		if (texture != null)
			_projectileTextures[path] = texture;
		return texture;
	}
}
