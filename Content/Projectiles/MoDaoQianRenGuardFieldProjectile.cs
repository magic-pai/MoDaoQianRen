using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using 魔刀千刃.Content.Items.Weapons;
using 魔刀千刃.Content.Players;
using 魔刀千刃.Content.Systems;

namespace 魔刀千刃.Content.Projectiles;

public class MoDaoQianRenGuardFieldProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Item_0";

	private ref float Timer => ref Projectile.localAI[0];

	public override void SetStaticDefaults()
	{
		Main.projPet[Type] = true;
		ProjectileID.Sets.MinionCannotBeFreed[Type] = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 240;
		Projectile.height = 240;
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.timeLeft = 2;
		Projectile.netImportant = true;
		Projectile.noEnchantmentVisuals = true;
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Player player = Main.player[Projectile.owner];
		if (!IsOwnerValid(player) || MoDaoQianRenWarmupSystem.ShouldBlockBladeUseForLights)
		{
			Projectile.Kill();
			return;
		}
		Timer++;
		Projectile.Center = player.Center;
		Projectile.velocity = Vector2.Zero;
		Projectile.timeLeft = 2;
		int guardForm = GetVisualGuardForm(player);
		if (Projectile.owner == Main.myPlayer && Projectile.ai[0] != guardForm)
		{
			Projectile.ai[0] = guardForm;
			Projectile.netUpdate = true;
		}
		float lightScale = guardForm switch
		{
			2 => 1.4f,
			1 => 0.95f,
			_ => 0.58f,
		};
		MoDaoQianRenWarmupSystem.AddLight(player.Center, 0.42f * lightScale, 0.08f * lightScale, 0.95f * lightScale);
		ProduceGuardDust(player, guardForm);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		if (MoDaoQianRenWarmupSystem.ShouldSkipCustomDrawingForLights)
		{
			return false;
		}
		Player player = Main.player[Projectile.owner];
		if (!IsOwnerValid(player))
		{
			return false;
		}
		DrawGuardShell(MoDaoQianRenShardVisuals.Texture, player, GetVisualGuardForm(player));
		return false;
	}

	private bool IsOwnerValid(Player player)
	{
		if (!player.active || player.dead)
		{
			return false;
		}
		if (!MoDaoQianRen.TryGetOwnedGrowthStage(player, out var growthStage) || !MoDaoQianRen.IsGuardModeUnlocked(growthStage))
		{
			return false;
		}
		return Projectile.owner != Main.myPlayer || player.GetModPlayer<MoDaoQianRenPlayer>().IsGuardMode;
	}

	private int GetVisualGuardForm(Player player)
	{
		int form = Projectile.owner == Main.myPlayer ? player.GetModPlayer<MoDaoQianRenPlayer>().GuardForm : (int)Projectile.ai[0];
		return Utils.Clamp(form, 0, 2);
	}

	private void DrawGuardShell(Texture2D shardTexture, Player player, int guardForm)
	{
		int shardCount = guardForm switch
		{
			2 => 160,
			1 => 104,
			_ => 58,
		};
		int rings = guardForm switch
		{
			2 => 4,
			1 => 3,
			_ => 2,
		};
		float baseRadius = guardForm switch
		{
			2 => 130f,
			1 => 118f,
			_ => 104f,
		};
		float densityPower = (float)guardForm / 2f;
		Vector2 drawCenter = player.Center - Main.screenPosition;
		for (int i = 0; i < shardCount; i++)
		{
			int ring = i % rings;
			float ringProgress = rings <= 1 ? 0f : (float)ring / (rings - 1);
			float progress = ((float)i + 0.5f) / shardCount;
			float angle = MathHelper.TwoPi * progress * (ring + 1.8f) + Timer * MathHelper.Lerp(0.018f, 0.045f, densityPower) * (ring % 2 == 0 ? 1f : -1f) + MoDaoQianRenShardVisuals.Random01(i * 31 + ring * 7) * MathHelper.TwoPi;
			Vector2 direction = angle.ToRotationVector2();
			Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
			float radius = baseRadius + MathHelper.Lerp(-28f, 34f, ringProgress) + MathF.Sin(Timer * 0.09f + i * 0.47f) * MathHelper.Lerp(5f, 14f, densityPower);
			float sideOffset = MathF.Cos(Timer * 0.16f + i * 1.13f) * MathHelper.Lerp(3f, 11f, ringProgress);
			Vector2 drawPosition = drawCenter + direction * radius + normal * sideOffset;
			float flicker = MoDaoQianRenShardVisuals.Flicker(Timer, i + 6200, 0.045f, guardForm == 2 ? 0.8f : 0.52f);
			float scale = MathHelper.Lerp(0.09f, 0.19f, densityPower) * MathHelper.Lerp(0.86f, 1.18f, flicker) * MathHelper.Lerp(0.9f, 1.1f, MoDaoQianRenShardVisuals.Random01(i * 47 + 13));
			float rotation = angle + MathHelper.PiOver2 + MathF.Sin(Timer * 0.14f + i) * MathHelper.Lerp(0.42f, 0.82f, densityPower);
			Color outline = Color.Lerp(new Color(115, 44, 255), new Color(230, 170, 255), densityPower) * MathHelper.Lerp(0.28f, 0.58f, densityPower) * (0.74f + flicker * 0.28f);
			Color core = Color.Lerp(new Color(174, 94, 255), Color.White, MathHelper.Lerp(0.18f, 0.42f, densityPower) + flicker * 0.24f) * MathHelper.Lerp(0.52f, 0.92f, densityPower);
			Color flash = Color.Lerp(new Color(250, 150, 255), Color.White, flicker * 0.72f);
			MoDaoQianRenShardVisuals.DrawOutlinedShard(shardTexture, drawPosition, i + 6200, outline, core, flash, rotation, scale, flicker);
		}
		DrawGuardRim(shardTexture, drawCenter, guardForm, baseRadius, densityPower);
	}

	private void DrawGuardRim(Texture2D shardTexture, Vector2 drawCenter, int guardForm, float baseRadius, float densityPower)
	{
		int shardCount = guardForm switch
		{
			2 => 72,
			1 => 42,
			_ => 0,
		};
		if (shardCount <= 0)
		{
			return;
		}
		float radius = baseRadius + (guardForm == 2 ? 54f : 38f);
		for (int i = 0; i < shardCount; i++)
		{
			float progress = (float)i / shardCount;
			float angle = MathHelper.TwoPi * progress - Timer * MathHelper.Lerp(0.035f, 0.062f, densityPower);
			Vector2 drawPosition = drawCenter + angle.ToRotationVector2() * (radius + MathF.Sin(Timer * 0.21f + i) * 4f);
			float flicker = MoDaoQianRenShardVisuals.Flicker(Timer, i + 7600, 0.075f, 0.84f);
			float scale = MathHelper.Lerp(0.08f, 0.15f, densityPower) * MathHelper.Lerp(0.9f, 1.2f, flicker);
			Color outline = new Color(190, 88, 255) * MathHelper.Lerp(0.34f, 0.62f, densityPower);
			Color core = Color.Lerp(new Color(200, 115, 255), Color.White, 0.34f + flicker * 0.34f) * MathHelper.Lerp(0.46f, 0.82f, densityPower);
			Color flash = Color.Lerp(new Color(255, 190, 255), Color.White, flicker * 0.72f);
			MoDaoQianRenShardVisuals.DrawOutlinedShard(shardTexture, drawPosition, i + 7600, outline, core, flash, angle + MathHelper.PiOver2, scale, flicker);
		}
	}

	private void ProduceGuardDust(Player player, int guardForm)
	{
		if (Main.dedServ || Timer % (guardForm == 2 ? 3f : 5f) != 0f)
		{
			return;
		}
		int dustCount = guardForm switch
		{
			2 => 3,
			1 => 2,
			_ => 1,
		};
		float radius = guardForm switch
		{
			2 => 172f,
			1 => 148f,
			_ => 118f,
		};
		for (int i = 0; i < dustCount; i++)
		{
			Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius);
			Vector2 velocity = offset.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.7f);
			Dust dust = Dust.NewDustPerfect(player.Center + offset, Main.rand.NextBool(3) ? DustID.PinkTorch : DustID.PurpleTorch, velocity, 60, Color.Lerp(new Color(180, 82, 255), Color.White, guardForm * 0.16f), Main.rand.NextFloat(0.85f, 1.55f));
			dust.noGravity = true;
			dust.fadeIn = Main.rand.NextFloat(0.35f, 0.75f);
		}
	}
}
