using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using 魔刀千刃.Content.Items.Weapons;
using 魔刀千刃.Content.Players;
using 魔刀千刃.Content.Systems;

namespace 魔刀千刃.Content.Projectiles;

public class MoDaoQianRenPrismShardProjectile : ModProjectile
{
	private const float TeleportRange = 4200f;

	public override string Texture => "Terraria/Images/Item_0";

	public float Time
	{
		get
		{
			return base.Projectile.localAI[1];
		}
		set
		{
			base.Projectile.localAI[1] = value;
		}
	}

	private int SlotIndex => Math.Max(0, (int)base.Projectile.ai[0]);

	private int ShardCapacity => Math.Max(1, (int)base.Projectile.ai[1]);

	private int GrowthStage
	{
		get
		{
			int growthStage = (int)base.Projectile.ai[2];
			if (growthStage >= 0)
			{
				return Utils.Clamp(growthStage, 0, 7);
			}
			if (!MoDaoQianRen.TryGetOwnedGrowthStage(Main.player[base.Projectile.owner], out growthStage))
			{
				return 0;
			}
			return growthStage;
		}
	}

	private bool North => SlotIndex % 2 == 0;

	private float AngularOffset => (float)Math.PI * 2f * (float)SlotIndex / (float)ShardCapacity;

	public override void SetStaticDefaults()
	{
		Main.projPet[base.Type] = true;
		ProjectileID.Sets.MinionShot[base.Type] = true;
		ProjectileID.Sets.TrailCacheLength[base.Type] = 10;
		ProjectileID.Sets.TrailingMode[base.Type] = 2;
		ProjectileID.Sets.MinionTargettingFeature[base.Type] = true;
		ProjectileID.Sets.MinionCannotBeFreed[base.Type] = true;
	}

	public override void SetDefaults()
	{
		base.Projectile.width = 14;
		base.Projectile.height = 14;
		base.Projectile.friendly = true;
		base.Projectile.hostile = false;
		base.Projectile.penetrate = -1;
		base.Projectile.tileCollide = false;
		base.Projectile.ignoreWater = true;
		base.Projectile.DamageType = DamageClass.Summon;
		base.Projectile.minion = true;
		base.Projectile.minionSlots = 0f;
		base.Projectile.usesLocalNPCImmunity = true;
		base.Projectile.localNPCHitCooldown = 28;
		base.Projectile.timeLeft = 2;
	}

	public override bool MinionContactDamage()
	{
		return true;
	}

	public override void AI()
	{
		Player player = Main.player[base.Projectile.owner];
		if (!IsOwnerValid(player))
		{
			base.Projectile.Kill();
			return;
		}
		if (MoDaoQianRenWarmupSystem.ShouldBlockBladeUseForLights)
		{
			base.Projectile.Kill();
			return;
		}
		int growthStage = GrowthStage;
		base.Projectile.timeLeft = 2;
		base.Projectile.localNPCHitCooldown = GetLocalHitCooldown(growthStage);
		base.Projectile.scale = MathHelper.Lerp(0.78f, 1.1f, GetOuterFactor()) * GetStageVisualScale(growthStage);
		if (!base.Projectile.WithinRange(player.Center, 4200f))
		{
			base.Projectile.Center = GetIdlePosition(player, SlotIndex, ShardCapacity, Time, growthStage);
			base.Projectile.velocity = Vector2.Zero;
		}
		NPC target = FindTarget(player, growthStage);
		if (target == null)
		{
			PlayerMovement(player, growthStage);
			RepelMovement(growthStage);
		}
		else
		{
			NPCMovement(target, growthStage);
			if ((int)(Time % (float)GetChargeTime(growthStage)) < GetLungeFrame(growthStage))
			{
				RepelMovement(growthStage);
			}
		}
		Time++;
		ProduceDust(target != null);
		MoDaoQianRenWarmupSystem.AddLight(base.Projectile.Center, (target == null) ? 0.24f : 0.48f, 0.05f, (target == null) ? 0.62f : 1.05f);
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		ProduceHitEffects(target);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		if (MoDaoQianRenWarmupSystem.ShouldSkipCustomDrawingForLights)
		{
			return false;
		}
		Texture2D texture = MoDaoQianRenShardVisuals.Texture;
		int seed = GetSeed();
		Rectangle source = MoDaoQianRenShardVisuals.GetFrame(seed);
		Vector2 origin = source.Size() * 0.5f;
		float flicker = MoDaoQianRenShardVisuals.Flicker(Time, seed + 1709, 0.055f, 0.68f);
		int growthStage = GrowthStage;
		int phase = (int)(Time % (float)GetChargeTime(growthStage));
		bool charging = phase >= GetChargeFrame(growthStage) && phase < GetLungeFrame(growthStage);
		bool lunging = phase >= GetLungeFrame(growthStage);
		float outerFactor = GetOuterFactor();
		float scale = MathHelper.Lerp(0.13f, 0.23f, outerFactor) * MathHelper.Lerp(0.9f, 1.18f, flicker) * (lunging ? 1.2f : (charging ? 1.08f : 1f));
		Color outline = Color.Lerp(North ? new Color(110, 52, 255) : new Color(215, 62, 255), Color.White, lunging ? 0.26f : (charging ? 0.16f : 0.04f)) * (0.38f + flicker * 0.22f);
		Color core = Color.Lerp(new Color(184, 92, 255), Color.White, 0.2f + flicker * 0.43f);
		Color flash = Color.Lerp(new Color(255, 166, 255), Color.White, flicker * 0.72f);
		for (int i = base.Projectile.oldPos.Length - 1; i >= 1; i--)
		{
			Vector2 oldCenter = base.Projectile.oldPos[i] + base.Projectile.Size * 0.5f;
			float progress = 1f - (float)i / (float)base.Projectile.oldPos.Length;
			Color trailColor = outline * (progress * (lunging ? 0.46f : 0.26f));
			Main.EntitySpriteDraw(texture, oldCenter - Main.screenPosition, source, trailColor, base.Projectile.rotation, origin, scale * (1.08f - progress * 0.22f), SpriteEffects.None);
		}
		MoDaoQianRenShardVisuals.DrawOutlinedShard(texture, base.Projectile.Center - Main.screenPosition, seed, outline, core, flash, base.Projectile.rotation, scale, flicker);
		return false;
	}

	public static Vector2 GetIdlePosition(Player player, int slotIndex, int shardCapacity, float timer)
	{
		return GetIdlePosition(player, slotIndex, shardCapacity, timer, 7);
	}

	public static Vector2 GetIdlePosition(Player player, int slotIndex, int shardCapacity, float timer, int growthStage)
	{
		shardCapacity = Math.Max(1, shardCapacity);
		bool north = slotIndex % 2 == 0;
		float angularOffset = (float)Math.PI * 2f * (float)slotIndex / (float)shardCapacity;
		return player.Center + Vector2.UnitY.RotatedBy(timer / 16f + angularOffset + (north ? 0f : ((float)Math.PI))) * GetPlayerOrbitRadius(growthStage);
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
			MoDaoQianRenPlayer bladePlayer = player.GetModPlayer<MoDaoQianRenPlayer>();
			return bladePlayer.IsShardPrismMode || bladePlayer.IsGuardMode;
		}
		return true;
	}

	private void PlayerMovement(Player player, int growthStage)
	{
		float idleSpeed = GetIdleSpeed(growthStage);
		Vector2 destination = player.Center + Vector2.UnitY.RotatedBy(Time / 16f + AngularOffset + (North ? 0f : ((float)Math.PI))) * GetPlayerOrbitRadius(growthStage);
		base.Projectile.velocity = (base.Projectile.velocity * 4f + (destination - base.Projectile.Center).SafeNormalize(Vector2.UnitY) * idleSpeed) / 5f;
		base.Projectile.velocity = base.Projectile.velocity.SafeNormalize(Vector2.UnitY) * idleSpeed;
		base.Projectile.rotation = base.Projectile.velocity.ToRotation();
	}

	private void NPCMovement(NPC npc, int growthStage)
	{
		int phase = (int)(Time % (float)GetChargeTime(growthStage));
		int chargeFrame = GetChargeFrame(growthStage);
		int lungeFrame = GetLungeFrame(growthStage);
		if (phase < chargeFrame)
		{
			float offsetAngle = AngularOffset * 0.5f + (North ? 0f : ((float)Math.PI));
			Vector2 destination = npc.Center + Vector2.UnitY.RotatedBy(offsetAngle) * GetTargetOrbitRadius(growthStage);
			base.Projectile.velocity = (base.Projectile.velocity * 4f + (destination - base.Projectile.Center).SafeNormalize(Vector2.UnitY) * 10f) / 5f;
			base.Projectile.velocity = base.Projectile.velocity.SafeNormalize(Vector2.UnitY) * GetTargetOrbitSpeed(growthStage);
			base.Projectile.rotation = base.Projectile.AngleTo(npc.Center);
		}
		else if (phase < lungeFrame)
		{
			base.Projectile.velocity *= 0.96f;
			base.Projectile.rotation += (North ? 0.05f : (-0.05f));
		}
		else if (phase == lungeFrame)
		{
			base.Projectile.velocity = (npc.Center - base.Projectile.Center).SafeNormalize(-Vector2.UnitY) * GetLungeSpeed(growthStage);
			base.Projectile.rotation = base.Projectile.AngleTo(npc.Center);
		}
	}

	private void RepelMovement(int growthStage)
	{
		int shardType = ModContent.ProjectileType<MoDaoQianRenPrismShardProjectile>();
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile other = Main.projectile[i];
			if (other.active && other.whoAmI != base.Projectile.whoAmI && other.owner == base.Projectile.owner && other.type == shardType && !(base.Projectile.Distance(other.Center) >= 40f) && other.ModProjectile is MoDaoQianRenPrismShardProjectile otherShard && otherShard.North != North)
			{
				float distance = base.Projectile.Distance(other.Center) + 1f;
				if (float.IsNaN(distance) || distance < 1f)
				{
					distance = 1f;
				}
				float repulsionSpeed = GetRepulsionSpeed(growthStage) * MathF.Pow(3f, (0f - distance) / 27f);
				base.Projectile.velocity -= (other.Center - base.Projectile.Center).SafeNormalize(Vector2.UnitY) * repulsionSpeed;
			}
		}
	}

	private NPC FindTarget(Player player, int growthStage)
	{
		NPC closestTarget = null;
		float closestDistanceSquared = GetSearchRange(growthStage) * GetSearchRange(growthStage);
		if (player.HasMinionAttackTargetNPC)
		{
			NPC markedTarget = Main.npc[player.MinionAttackTargetNPC];
			if (markedTarget.CanBeChasedBy(base.Projectile))
			{
				float markedDistanceSquared = Vector2.DistanceSquared(base.Projectile.Center, markedTarget.Center);
				if (markedDistanceSquared < closestDistanceSquared)
				{
					closestDistanceSquared = markedDistanceSquared;
					closestTarget = markedTarget;
				}
			}
		}
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

	private static float GetSearchRange(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).ShardPrismSearchRange;
	}

	private static float GetPlayerOrbitRadius(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).ShardPrismPlayerOrbitRadius;
	}

	private static float GetTargetOrbitRadius(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).ShardPrismTargetOrbitRadius;
	}

	private static int GetChargeTime(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).ShardPrismChargeTime;
	}

	private static int GetChargeFrame(int growthStage)
	{
		return Math.Max(12, (int)MathF.Round((float)GetChargeTime(growthStage) * 0.44f));
	}

	private static int GetLungeFrame(int growthStage)
	{
		return Math.Max(GetChargeFrame(growthStage) + 6, (int)MathF.Round((float)GetChargeTime(growthStage) * 0.78f));
	}

	private static float GetIdleSpeed(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).ShardPrismIdleSpeed;
	}

	private static float GetTargetOrbitSpeed(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).ShardPrismTargetOrbitSpeed;
	}

	private static float GetLungeSpeed(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).ShardPrismLungeSpeed;
	}

	private static float GetRepulsionSpeed(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).ShardPrismRepulsionSpeed;
	}

	private static int GetLocalHitCooldown(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).ShardPrismLocalHitCooldown;
	}

	private static float GetStageVisualScale(int growthStage)
	{
		if (growthStage >= 5)
		{
			if (growthStage < 7)
			{
				if (growthStage >= 6)
				{
					return 0.96f;
				}
				return 0.91f;
			}
			return 1f;
		}
		if (growthStage >= 4)
		{
			return 0.86f;
		}
		return 0.8f;
	}

	private void ProduceDust(bool hasTarget)
	{
		if (!Main.dedServ && Main.rand.NextBool(hasTarget ? 2 : 5))
		{
			Dust dust = Dust.NewDustDirect(base.Projectile.position, base.Projectile.width, base.Projectile.height, hasTarget ? 62 : 242, (0f - base.Projectile.velocity.X) * 0.08f, (0f - base.Projectile.velocity.Y) * 0.08f, 70, new Color(212, 118, 255), Main.rand.NextFloat(0.82f, hasTarget ? 1.42f : 1.15f));
			dust.noGravity = true;
			dust.fadeIn = 0.35f;
		}
	}

	private void ProduceHitEffects(NPC target)
	{
		if (!Main.dedServ)
		{
			SoundStyle style = SoundID.Item10 with
			{
				Volume = 0.28f,
				Pitch = 0.08f
			};
			SoundEngine.PlaySound(in style, target.Center);
			int dustCount = ((target.life <= 0) ? 12 : 5);
			for (int i = 0; i < dustCount; i++)
			{
				Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular((float)target.width * 0.34f, (float)target.height * 0.34f), Main.rand.NextBool(4) ? 27 : 62, Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.6f, (target.life <= 0) ? 5.8f : 3.2f), 68, Color.Lerp(new Color(196, 92, 255), Color.White, Main.rand.NextFloat(0.12f, 0.48f)), Main.rand.NextFloat(0.82f, 1.45f)).noGravity = true;
			}
			MoDaoQianRenWarmupSystem.AddLight(target.Center, 0.55f, 0.08f, 1.05f);
		}
	}

	private int GetSeed()
	{
		return SlotIndex * 37 + base.Projectile.identity * 11 + 1701;
	}

	private float GetOuterFactor()
	{
		return MathHelper.Clamp(((float)SlotIndex + 0.5f) / (float)ShardCapacity, 0f, 1f);
	}
}
