using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using 魔刀千刃.Content.Items.Weapons;
using 魔刀千刃.Content.Players;
using 魔刀千刃.Content.Systems;

namespace 魔刀千刃.Content.Projectiles;

public class MoDaoQianRenShearsProjectile : ModProjectile
{
	private const float DrawScale = 1f;

	private const float IdleRotation = 0f;

	private const float AttackRotationOffset = (float)Math.PI / 2f;

	private const float FastPlayerSpeed = 7.2f;

	private const float FastProjectileSpeed = 16f;

	private const float PersonalSearchRange = 520f;

	private const float FormationAcceleration = 0.18f;

	private const float FormationMaxSpeed = 22f;

	private const float FormationSnapDistance = 2600f;

	private const float IdleRingRotationSpeed = 0.026f;

	private const float ClockwiseSpinSpeed = 0.68f;

	private const float IdleGlowDustChance = 0.18f;

	private const float AttackGlowDustChance = 0.42f;

	private float visualRotation;

	private bool usingSpinVisual;

	private bool usingAttackVisual;

	public override string Texture => MoDaoQianRenMod.ShearsTexture;

	public override void SetStaticDefaults()
	{
		Main.projPet[base.Type] = true;
		ProjectileID.Sets.MinionTargettingFeature[base.Type] = true;
		ProjectileID.Sets.MinionCannotBeFreed[base.Type] = true;
		ProjectileID.Sets.TrailCacheLength[base.Type] = 12;
		ProjectileID.Sets.TrailingMode[base.Type] = 2;
	}

	public override void SetDefaults()
	{
		base.Projectile.aiStyle = 156;
		base.AIType = 946;
		base.Projectile.width = 34;
		base.Projectile.height = 75;
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
		base.Projectile.localNPCHitCooldown = 24;
		base.Projectile.alpha = 0;
		base.Projectile.hide = false;
		base.Projectile.scale = 1f;
		base.Projectile.timeLeft = 2;
	}

	public override void OnSpawn(IEntitySource source)
	{
		visualRotation = 0f;
	}

	public override bool PreAI()
	{
		Player player = Main.player[base.Projectile.owner];
		if (!IsOwnerValid(player) || MoDaoQianRenWarmupSystem.ShouldBlockBladeUseForLights)
		{
			base.Projectile.Kill();
			return false;
		}
		base.Projectile.localNPCHitCooldown = MoDaoQianRen.GetRuntimeStats(GetOwnerGrowthStage(player)).ShearsLocalHitCooldown;
		base.Projectile.timeLeft = 2;
		KeepTextureVisible();
		UpdateVisualState(player);
		if (usingAttackVisual)
		{
			return true;
		}
		ApplyIdleFormation(player);
		ProduceGlowEffects();
		return false;
	}

	public override void PostAI()
	{
		if (usingAttackVisual)
		{
			ProduceGlowEffects();
		}
		KeepTextureVisible();
	}

	public override bool ShouldUpdatePosition()
	{
		return usingAttackVisual;
	}

	public override bool MinionContactDamage()
	{
		return true;
	}

	public override bool? CanDamage()
	{
		if (!usingAttackVisual)
		{
			return false;
		}
		return null;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
		Rectangle source = texture.Frame();
		Vector2 origin = source.Size() * 0.5f;
		Vector2 floatOffset = Vector2.Zero;
		Vector2 drawPosition = base.Projectile.Center - Main.screenPosition + floatOffset;
		float drawRotation = (usingAttackVisual ? GetAttackDrawRotation() : visualRotation);
		Vector2 slotDrawOffset = Vector2.Zero;
		float slotRotationOffset = 0f;
		drawPosition += slotDrawOffset;
		drawRotation += slotRotationOffset;
		if (usingSpinVisual || usingAttackVisual)
		{
			for (int i = base.Projectile.oldPos.Length - 1; i >= 1; i--)
			{
				Vector2 oldCenter = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f - Main.screenPosition + floatOffset + slotDrawOffset;
				float progress = 1f - (float)i / (float)base.Projectile.oldPos.Length;
				float trailRotation = (usingSpinVisual ? (visualRotation - (float)i * 0.46f + slotRotationOffset) : drawRotation);
				Color trailColor = new Color(155, 190, 210) * (progress * (usingSpinVisual ? 0.34f : 0.24f));
				Main.EntitySpriteDraw(texture, oldCenter, source, trailColor, trailRotation, origin, 1f, SpriteEffects.None);
			}
		}
		Color bodyColor = Color.Lerp(lightColor, Color.White, 0.82f);
		bodyColor.A = byte.MaxValue;
		Main.EntitySpriteDraw(texture, drawPosition, source, bodyColor, drawRotation, origin, 1f, SpriteEffects.None);
		return false;
	}

	private void KeepTextureVisible()
	{
		base.Projectile.alpha = 0;
		base.Projectile.hide = false;
		base.Projectile.scale = 1f;
	}

	private void ApplyIdleFormation(Player player)
	{
		if (!player.active)
		{
			return;
		}
		int shearsCount = CountOwnedShears();
		int slot = Utils.Clamp((int)base.Projectile.ai[2], 0, shearsCount - 1);
		Vector2 targetCenter = GetIdleFormationCenter(player, slot, shearsCount);
		Vector2 toFormation = targetCenter - base.Projectile.Center;
		float distance = toFormation.Length();
		if (distance > 2600f)
		{
			base.Projectile.Center = targetCenter;
			base.Projectile.velocity = Vector2.Zero;
			ResetOldPositions();
			return;
		}
		if (distance <= 2.5f)
		{
			base.Projectile.Center = targetCenter;
			base.Projectile.velocity *= 0.55f;
			return;
		}
		float maxSpeed = 22f + player.velocity.Length() * 0.45f;
		float desiredSpeed = MathHelper.Clamp(distance * 0.105f, 1.8f, maxSpeed);
		Vector2 desiredVelocity = toFormation.SafeNormalize(Vector2.Zero) * desiredSpeed + player.velocity * 0.14f;
		base.Projectile.velocity = Vector2.Lerp(base.Projectile.velocity, desiredVelocity, 0.18f);
		if (base.Projectile.velocity.LengthSquared() > maxSpeed * maxSpeed)
		{
			base.Projectile.velocity = base.Projectile.velocity.SafeNormalize(Vector2.Zero) * maxSpeed;
		}
		base.Projectile.Center += base.Projectile.velocity;
	}

	private void ResetOldPositions()
	{
		for (int i = 0; i < base.Projectile.oldPos.Length; i++)
		{
			base.Projectile.oldPos[i] = base.Projectile.position;
		}
	}

	private void ProduceGlowEffects()
	{
		float lightPower = (usingAttackVisual ? 0.72f : 0.42f);
		MoDaoQianRenWarmupSystem.AddLight(base.Projectile.Center, 0.06f * lightPower, 0.22f * lightPower, 0.34f * lightPower);
		if (!Main.dedServ)
		{
			float dustChance = (usingAttackVisual ? 0.42f : 0.18f);
			if (!(Main.rand.NextFloat() >= dustChance))
			{
				Vector2 edgeOffset = Main.rand.NextVector2Circular((float)base.Projectile.width * 0.42f, (float)base.Projectile.height * 0.42f);
				Vector2 spinVelocity = edgeOffset.SafeNormalize(Vector2.UnitY).RotatedBy(1.5707963705062866) * Main.rand.NextFloat(0.25f, 0.95f);
				Vector2 driftVelocity = (usingAttackVisual ? (-base.Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.2f, 0.9f)) : Vector2.Zero);
				Dust dust = Dust.NewDustPerfect(base.Projectile.Center + edgeOffset, 59, spinVelocity + driftVelocity, 120, new Color(145, 215, 255), Main.rand.NextFloat(usingAttackVisual ? 0.72f : 0.52f, usingAttackVisual ? 1.08f : 0.82f));
				dust.noGravity = true;
				dust.fadeIn = Main.rand.NextFloat(0.18f, 0.42f);
				dust.noLight = true;
			}
		}
	}

	private int CountOwnedShears()
	{
		int count = 0;
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile projectile = Main.projectile[i];
			if (projectile.active && projectile.owner == base.Projectile.owner && projectile.type == base.Projectile.type)
			{
				count++;
			}
		}
		return Math.Max(1, count);
	}

	private int GetSlot(int shearsCount)
	{
		return Utils.Clamp((int)base.Projectile.ai[2], 0, shearsCount - 1);
	}

	private static Vector2 GetIdleFormationCenter(Player player, int slot, int shearsCount)
	{
		float radius = MathHelper.Clamp(78f + (float)shearsCount * 2.8f, 82f, 132f);
		float angle = (float)Math.PI * 2f * (float)slot / (float)shearsCount - (float)Math.PI / 2f + (float)Main.GameUpdateCount * 0.026f;
		return player.Center + angle.ToRotationVector2() * radius;
	}

	private bool IsOwnerValid(Player player)
	{
		if (!player.active || player.dead)
		{
			return false;
		}
		if (!MoDaoQianRen.TryGetOwnedGrowthStage(player, out var growthStage) || !MoDaoQianRen.IsShardPrismModeUnlocked(growthStage))
		{
			return false;
		}
		if (base.Projectile.owner == Main.myPlayer)
		{
			return player.GetModPlayer<MoDaoQianRenPlayer>().IsShardPrismShearsMode;
		}
		return true;
	}

	private static int GetOwnerGrowthStage(Player player)
	{
		if (!MoDaoQianRen.TryGetOwnedGrowthStage(player, out var growthStage))
		{
			return 0;
		}
		return growthStage;
	}

	private void UpdateVisualState(Player player)
	{
		int shearsCount = CountOwnedShears();
		int slot = GetSlot(shearsCount);
		Vector2 formationCenter = GetIdleFormationCenter(player, slot, shearsCount);
		Vector2 searchCenter = (usingAttackVisual ? base.Projectile.Center : formationCenter);
		NPC target = FindTarget(player, searchCenter, 520f);
		usingAttackVisual = target != null;
		bool fastMovement = player.velocity.LengthSquared() >= 51.839996f || base.Projectile.velocity.LengthSquared() >= 256f;
		usingSpinVisual = !usingAttackVisual;
		if (usingAttackVisual)
		{
			visualRotation = GetAttackDrawRotation(target);
			return;
		}
		float speedBoost = (fastMovement ? MathHelper.Clamp(player.velocity.Length() * 0.018f, 0f, 0.22f) : 0f);
		visualRotation = MathHelper.WrapAngle(visualRotation + 0.68f + speedBoost);
	}

	private float GetAttackDrawRotation(NPC target = null)
	{
		return ((base.Projectile.velocity.LengthSquared() > 1f) ? base.Projectile.velocity.SafeNormalize(Vector2.UnitX) : ((target != null) ? (target.Center - base.Projectile.Center).SafeNormalize(Vector2.UnitX) : base.Projectile.rotation.ToRotationVector2())).ToRotation() + (float)Math.PI / 2f;
	}

	private NPC FindTarget(Player player, Vector2 searchCenter, float searchRange)
	{
		NPC closestTarget = null;
		float closestDistanceSquared = searchRange * searchRange;
		if (player.HasMinionAttackTargetNPC)
		{
			NPC markedTarget = Main.npc[player.MinionAttackTargetNPC];
			if (markedTarget.CanBeChasedBy(base.Projectile) && Vector2.DistanceSquared(searchCenter, markedTarget.Center) < closestDistanceSquared)
			{
				closestDistanceSquared = Vector2.DistanceSquared(searchCenter, markedTarget.Center);
				closestTarget = markedTarget;
			}
		}
		for (int i = 0; i < Main.maxNPCs; i++)
		{
			NPC npc = Main.npc[i];
			if (npc.CanBeChasedBy(base.Projectile))
			{
				float distanceSquared = Vector2.DistanceSquared(searchCenter, npc.Center);
				if (!(distanceSquared >= closestDistanceSquared))
				{
					closestDistanceSquared = distanceSquared;
					closestTarget = npc;
				}
			}
		}
		return closestTarget;
	}
}
