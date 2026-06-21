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

public class MoDaoQianRenShearsTyphoonProjectile : ModProjectile
{
	private const int GrowTicks = 90;

	private const float StartDiameter = 82f;

	private const float MaxDiameter = 560f;

	private const float FollowAcceleration = 0.13f;

	private const float MaxFollowSpeed = 42f;

	private const float SlowSpinSpeed = 0.08f;

	private const float FastSpinSpeed = 0.74f;

	private const int TyphoonFrameCount = 8;

	private const int TyphoonFrameTicks = 2;

	public override string Texture => MoDaoQianRenMod.ShearsTexture;

	private static string TyphoonVortexTexture => ModContent.GetInstance<MoDaoQianRenMod>().Name + "/Content/Projectiles/Generated/MoDaoQianRenShearsTyphoonVortex";

	private ref float Timer => ref base.Projectile.localAI[0];

	private ref float VisualRotation => ref base.Projectile.localAI[1];

	private float ChargeProgress => SmoothStep(MathHelper.Clamp(Timer / 90f, 0f, 1f));

	private float CurrentDiameter => MathHelper.Lerp(82f, 560f, ChargeProgress);

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[base.Type] = 18;
		ProjectileID.Sets.TrailingMode[base.Type] = 2;
	}

	public override void SetDefaults()
	{
		base.Projectile.width = 82;
		base.Projectile.height = 82;
		base.Projectile.friendly = true;
		base.Projectile.hostile = false;
		base.Projectile.penetrate = -1;
		base.Projectile.tileCollide = false;
		base.Projectile.ignoreWater = true;
		base.Projectile.DamageType = DamageClass.Summon;
		base.Projectile.minion = true;
		base.Projectile.minionSlots = 0f;
		base.Projectile.netImportant = true;
		base.Projectile.noEnchantmentVisuals = true;
		base.Projectile.usesLocalNPCImmunity = true;
		base.Projectile.localNPCHitCooldown = 8;
		base.Projectile.timeLeft = 2;
		base.Projectile.alpha = 0;
	}

	public override bool? CanDamage()
	{
		if (!(ChargeProgress > 0.12f))
		{
			return false;
		}
		return null;
	}

	public override void AI()
	{
		Player player = Main.player[base.Projectile.owner];
		if (!IsOwnerValid(player))
		{
			base.Projectile.Kill();
			return;
		}
		base.Projectile.localNPCHitCooldown = MoDaoQianRen.GetRuntimeStats(GetOwnerGrowthStage(player)).ShearsTyphoonLocalHitCooldown;
		base.Projectile.timeLeft = 2;
		Timer += 1f;
		UpdateMouseTarget(player);
		MoveWithInertia();
		UpdateSizeAndSpin();
		UpdateTyphoonAnimation();
		ProduceEffects();
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		float radius = CurrentDiameter * 0.47f;
		float closestX = MathHelper.Clamp(base.Projectile.Center.X, targetHitbox.Left, targetHitbox.Right);
		float closestY = MathHelper.Clamp(base.Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom);
		return Vector2.DistanceSquared(base.Projectile.Center, new Vector2(closestX, closestY)) <= radius * radius;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D shearsTexture = ModContent.Request<Texture2D>(Texture).Value;
		Rectangle shearsSource = shearsTexture.Frame();
		Vector2 shearsOrigin = shearsSource.Size() * 0.5f;
		Vector2 drawCenter = base.Projectile.Center - Main.screenPosition;
		float shearsScale = CurrentDiameter / (float)shearsSource.Height;
		float progress = ChargeProgress;
		float vortexOpacity = SmoothStep(Utils.GetLerpValue(0.18f, 0.9f, progress, clamped: true));
		float shearsOpacity = 1f - SmoothStep(Utils.GetLerpValue(0.38f, 0.96f, progress, clamped: true));
		DrawTyphoonAnimation(drawCenter, vortexOpacity);
		if (shearsOpacity > 0.01f)
		{
			int afterimageCount = 3 + (int)(progress * 5f);
			for (int i = afterimageCount; i >= 1; i--)
			{
				float fade = 1f - (float)i / ((float)afterimageCount + 1f);
				float rotation = VisualRotation - (float)i * MathHelper.Lerp(0.1f, 0.34f, progress);
				Color trailColor = new Color(105, 205, 255) * ((0.08f + fade * 0.12f) * shearsOpacity);
				Main.EntitySpriteDraw(shearsTexture, drawCenter, shearsSource, trailColor, rotation, shearsOrigin, shearsScale * (1f + (float)i * 0.012f), SpriteEffects.None);
			}
			Color bodyColor = Color.Lerp(lightColor, Color.White, 0.74f) * shearsOpacity;
			bodyColor.A = (byte)(byte.MaxValue * shearsOpacity);
			Main.EntitySpriteDraw(shearsTexture, drawCenter, shearsSource, bodyColor, VisualRotation, shearsOrigin, shearsScale, SpriteEffects.None);
			Main.EntitySpriteDraw(shearsTexture, drawCenter, shearsSource, new Color(120, 210, 255) * ((0.18f + progress * 0.16f) * shearsOpacity), VisualRotation, shearsOrigin, shearsScale * 1.035f, SpriteEffects.None);
		}
		return false;
	}

	private bool IsOwnerValid(Player player)
	{
		if (!player.active || player.dead || MoDaoQianRenWarmupSystem.ShouldBlockBladeUseForLights)
		{
			return false;
		}
		if (!MoDaoQianRen.TryGetOwnedGrowthStage(player, out var growthStage) || !MoDaoQianRen.IsShardPrismModeUnlocked(growthStage))
		{
			return false;
		}
		if (base.Projectile.owner != Main.myPlayer)
		{
			return true;
		}
		if (Main.mouseRight && !player.noItems && !player.CCed && player.HeldItem?.ModItem is MoDaoQianRen)
		{
			return player.GetModPlayer<MoDaoQianRenPlayer>().IsShardPrismShearsMode;
		}
		return false;
	}

	private static int GetOwnerGrowthStage(Player player)
	{
		if (!MoDaoQianRen.TryGetOwnedGrowthStage(player, out var growthStage))
		{
			return 0;
		}
		return growthStage;
	}

	private void UpdateMouseTarget(Player player)
	{
		if (base.Projectile.owner == Main.myPlayer)
		{
			base.Projectile.ai[0] = Main.MouseWorld.X;
			base.Projectile.ai[1] = Main.MouseWorld.Y;
			if (Timer % 6f == 0f)
			{
				base.Projectile.netUpdate = true;
			}
		}
	}

	private void MoveWithInertia()
	{
		Vector2 target = new Vector2(base.Projectile.ai[0], base.Projectile.ai[1]);
		if (target == Vector2.Zero || target.HasNaNs())
		{
			target = base.Projectile.Center;
		}
		Vector2 toTarget = target - base.Projectile.Center;
		float desiredSpeed = MathHelper.Clamp(toTarget.Length() * 0.16f, 0f, 42f);
		Vector2 desiredVelocity = toTarget.SafeNormalize(Vector2.Zero) * desiredSpeed;
		base.Projectile.velocity = Vector2.Lerp(base.Projectile.velocity, desiredVelocity, 0.13f);
		if (base.Projectile.velocity.LengthSquared() > 1764f)
		{
			base.Projectile.velocity = base.Projectile.velocity.SafeNormalize(Vector2.Zero) * 42f;
		}
	}

	private void UpdateSizeAndSpin()
	{
		int size = Math.Max(8, (int)MathF.Round(CurrentDiameter));
		base.Projectile.Resize(size, size);
		base.Projectile.rotation = VisualRotation;
		VisualRotation = MathHelper.WrapAngle(VisualRotation + MathHelper.Lerp(0.08f, 0.74f, ChargeProgress));
	}

	private void ProduceEffects()
	{
		float progress = ChargeProgress;
		MoDaoQianRenWarmupSystem.AddLight(base.Projectile.Center, 0.12f + progress * 0.22f, 0.34f + progress * 0.28f, 0.58f + progress * 0.55f);
		if (Main.dedServ)
		{
			return;
		}
		int dustCount = ((progress > 0.65f) ? 3 : ((!(progress > 0.3f)) ? 1 : 2));
		for (int i = 0; i < dustCount; i++)
		{
			if (!(Main.rand.NextFloat() > 0.42f + progress * 0.38f))
			{
				float f = Main.rand.NextFloat((float)Math.PI * 2f);
				float radius = CurrentDiameter * Main.rand.NextFloat(0.32f, 0.5f);
				Vector2 radial = f.ToRotationVector2();
				Vector2 position = base.Projectile.Center + radial * radius;
				Vector2 velocity = radial.RotatedBy(1.5707963705062866) * Main.rand.NextFloat(1.2f, 3.8f) - base.Projectile.velocity * 0.08f;
				Dust dust = Dust.NewDustPerfect(position, 59, velocity, 110, new Color(120, 218, 255), Main.rand.NextFloat(0.78f, 1.32f));
				dust.noGravity = true;
				dust.noLight = true;
				dust.fadeIn = Main.rand.NextFloat(0.22f, 0.55f);
			}
		}
	}

	private void UpdateTyphoonAnimation()
	{
		base.Projectile.frameCounter++;
		if (base.Projectile.frameCounter >= TyphoonFrameTicks)
		{
			base.Projectile.frameCounter = 0;
			base.Projectile.frame = (base.Projectile.frame + 1) % TyphoonFrameCount;
		}
	}

	private void DrawTyphoonAnimation(Vector2 drawCenter, float opacity)
	{
		if (!(opacity <= 0f))
		{
			Texture2D texture = ModContent.Request<Texture2D>(TyphoonVortexTexture).Value;
			int frame = Math.Clamp(base.Projectile.frame, 0, TyphoonFrameCount - 1);
			Rectangle source = texture.Frame(TyphoonFrameCount, 1, frame, 0);
			Vector2 origin = source.Size() * 0.5f;
			float typhoonBaseSize = MathF.Max((float)source.Width, (float)source.Height);
			Main.EntitySpriteDraw(scale: CurrentDiameter / typhoonBaseSize * 1.08f, color: Color.White * opacity, texture: texture, position: drawCenter, sourceRectangle: source, rotation: (0f - VisualRotation) * 0.58f, origin: origin, effects: SpriteEffects.None);
		}
	}

	private static float SmoothStep(float value)
	{
		value = MathHelper.Clamp(value, 0f, 1f);
		return value * value * (3f - 2f * value);
	}
}
