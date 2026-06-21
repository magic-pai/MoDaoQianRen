using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using 魔刀千刃.Content.Items.Weapons;
using 魔刀千刃.Content.Players;
using 魔刀千刃.Content.Systems;

namespace 魔刀千刃.Content.Projectiles;

public class MoDaoQianRenShardProjectile : ModProjectile
{
	public const float BurstHomingMode = 1f;

	public const float StreamGuidedMode = 2f;

	public const float FinalBladeAssistMode = 3f;

	public override string Texture => "Terraria/Images/Item_0";

	private bool IsBurstHomingShard => base.Projectile.ai[2] == 1f;

	private bool IsStreamGuidedShard => base.Projectile.ai[2] == 2f;

	private bool IsFinalBladeAssistShard => base.Projectile.ai[2] == 3f;

	private bool UsesBurstHoming
	{
		get
		{
			if (!IsBurstHomingShard)
			{
				return IsFinalBladeAssistShard;
			}
			return true;
		}
	}

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.TrailCacheLength[base.Type] = 8;
		ProjectileID.Sets.TrailingMode[base.Type] = 2;
	}

	public override void SetDefaults()
	{
		base.Projectile.width = 10;
		base.Projectile.height = 10;
		base.Projectile.friendly = true;
		base.Projectile.hostile = false;
		base.Projectile.penetrate = 3;
		base.Projectile.tileCollide = false;
		base.Projectile.ignoreWater = true;
		base.Projectile.DamageType = DamageClass.Melee;
		base.Projectile.timeLeft = 52;
		base.Projectile.extraUpdates = 1;
		base.Projectile.usesLocalNPCImmunity = true;
		base.Projectile.localNPCHitCooldown = 10;
	}

	public override void AI()
	{
		if (MoDaoQianRenWarmupSystem.ShouldBlockBladeUseForLights)
		{
			base.Projectile.Kill();
			return;
		}
		if (UsesBurstHoming && base.Projectile.localAI[0] == 0f)
		{
			base.Projectile.localAI[0] = 1f;
			base.Projectile.penetrate = (IsFinalBladeAssistShard ? 4 : 5);
			base.Projectile.timeLeft = (IsFinalBladeAssistShard ? 82 : 76);
			base.Projectile.localNPCHitCooldown = 6;
		}
		else if (IsStreamGuidedShard && base.Projectile.localAI[0] == 0f)
		{
			base.Projectile.localAI[0] = 1f;
			base.Projectile.penetrate = 4;
			base.Projectile.timeLeft = 64;
			base.Projectile.localNPCHitCooldown = 8;
		}
		base.Projectile.rotation = base.Projectile.velocity.ToRotation();
		base.Projectile.velocity *= (UsesBurstHoming ? 1.006f : (IsStreamGuidedShard ? 1.005f : 1.004f));
		if (UsesBurstHoming)
		{
			UpdateBurstHoming();
		}
		else if (IsStreamGuidedShard)
		{
			UpdateStreamGuidance();
		}
		MoDaoQianRenWarmupSystem.AddLight(base.Projectile.Center, UsesBurstHoming ? 0.62f : (IsStreamGuidedShard ? 0.5f : 0.42f), 0.08f, IsFinalBladeAssistShard ? 1.28f : (UsesBurstHoming ? 1.18f : (IsStreamGuidedShard ? 1.02f : 0.9f)));
		if (Main.rand.NextBool(2))
		{
			Dust dust = Dust.NewDustDirect(base.Projectile.position, base.Projectile.width, base.Projectile.height, 62, 0f, 0f, 80, new Color(210, 120, 255), Main.rand.NextFloat(1.05f, UsesBurstHoming ? 1.85f : (IsStreamGuidedShard ? 1.68f : 1.55f)));
			dust.noGravity = true;
			dust.fadeIn = 0.4f;
			dust.velocity = -base.Projectile.velocity * Main.rand.NextFloat(0.05f, 0.12f);
		}
		if (Main.rand.NextBool(4))
		{
			Dust.NewDustPerfect(base.Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 242, -base.Projectile.velocity * Main.rand.NextFloat(0.04f, 0.09f), 60, Color.White, Main.rand.NextFloat(0.65f, 1.05f)).noGravity = true;
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (IsFinalBladeAssistShard && base.Projectile.owner == Main.myPlayer && base.Projectile.owner >= 0 && base.Projectile.owner < 255)
		{
			Player player = Main.player[base.Projectile.owner];
			if (player.active && MoDaoQianRen.TryGetGrowthStage(player.HeldItem, out var growthStage) && growthStage >= 7)
			{
				player.GetModPlayer<MoDaoQianRenPlayer>().AddShardCharge(1);
			}
		}
	}

	private void UpdateBurstHoming()
	{
		base.Projectile.localAI[1] += 1f;
		if (!(base.Projectile.localAI[1] < 8f))
		{
			NPC target = FindTarget(760f);
			if (target != null)
			{
				float speed = MathHelper.Clamp(base.Projectile.velocity.Length() * 1.01f, 14f, 24f);
				Vector2 desiredVelocity = (target.Center - base.Projectile.Center).SafeNormalize(base.Projectile.velocity.SafeNormalize(Vector2.UnitX)) * speed;
				base.Projectile.velocity = Vector2.Lerp(base.Projectile.velocity, desiredVelocity, 0.105f);
			}
		}
	}

	private void UpdateStreamGuidance()
	{
		base.Projectile.localAI[1] += 1f;
		if (!(base.Projectile.localAI[1] < 6f))
		{
			NPC target = FindTarget(560f);
			if (target != null)
			{
				float speed = MathHelper.Clamp(base.Projectile.velocity.Length() * 1.005f, 12f, 22f);
				Vector2 desiredVelocity = (target.Center - base.Projectile.Center).SafeNormalize(base.Projectile.velocity.SafeNormalize(Vector2.UnitX)) * speed;
				base.Projectile.velocity = Vector2.Lerp(base.Projectile.velocity, desiredVelocity, 0.058f);
			}
		}
	}

	private NPC FindTarget(float maxDistance)
	{
		NPC closestTarget = null;
		float closestDistanceSquared = maxDistance * maxDistance;
		for (int i = 0; i < Main.maxNPCs; i++)
		{
			NPC npc = Main.npc[i];
			if (npc.CanBeChasedBy(base.Projectile))
			{
				float distanceSquared = Vector2.DistanceSquared(base.Projectile.Center, npc.Center);
				if (!(distanceSquared >= closestDistanceSquared))
				{
					closestDistanceSquared = distanceSquared;
					closestTarget = npc;
				}
			}
		}
		return closestTarget;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		if (MoDaoQianRenWarmupSystem.ShouldSkipCustomDrawingForLights)
		{
			return false;
		}
		Texture2D texture = MoDaoQianRenShardVisuals.Texture;
		int seed = (int)base.Projectile.ai[0];
		Rectangle source = MoDaoQianRenShardVisuals.GetFrame(seed);
		Vector2 origin = source.Size() * 0.5f;
		float flicker = MoDaoQianRenShardVisuals.Flicker(Main.GameUpdateCount, seed + 503, 0.07f, IsFinalBladeAssistShard ? 0.78f : 0.58f);
		float scale = 0.16f * base.Projectile.ai[1] * (UsesBurstHoming ? 1.18f : (IsStreamGuidedShard ? 1.08f : 1f));
		Color outline = (IsFinalBladeAssistShard ? new Color(230, 82, 255) : new Color(145, 60, 255)) * (0.42f + flicker * 0.24f) * base.Projectile.Opacity;
		Color color = Color.Lerp(new Color(184, 104, 255), Color.White, 0.22f + flicker * 0.42f) * base.Projectile.Opacity;
		Color flash = Color.Lerp(new Color(255, 165, 255), Color.White, flicker * 0.7f) * base.Projectile.Opacity;
		for (int i = base.Projectile.oldPos.Length - 1; i >= 1; i--)
		{
			Vector2 oldCenter = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f;
			float trailProgress = 1f - (float)i / (float)base.Projectile.oldPos.Length;
			Color trailColor = outline * (trailProgress * 0.3f);
			Main.EntitySpriteDraw(texture, oldCenter - Main.screenPosition, source, trailColor, base.Projectile.rotation, origin, scale * (1.04f - trailProgress * 0.18f), SpriteEffects.None);
		}
		MoDaoQianRenShardVisuals.DrawOutlinedShard(texture, base.Projectile.Center - Main.screenPosition, seed, outline, color, flash, base.Projectile.rotation, scale * MathHelper.Lerp(0.94f, 1.12f, flicker), flicker);
		return false;
	}
}
