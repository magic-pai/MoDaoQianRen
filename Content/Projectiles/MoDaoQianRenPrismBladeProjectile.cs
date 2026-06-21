using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using 魔刀千刃.Content.Items.Weapons;
using 魔刀千刃.Content.Players;
using 魔刀千刃.Content.Systems;

namespace 魔刀千刃.Content.Projectiles;

public class MoDaoQianRenPrismBladeProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Item_0";

	public override void SetStaticDefaults()
	{
		Main.projPet[base.Type] = true;
		ProjectileID.Sets.MinionTargettingFeature[base.Type] = true;
		ProjectileID.Sets.MinionCannotBeFreed[base.Type] = true;
	}

	public override void SetDefaults()
	{
		base.Projectile.width = 24;
		base.Projectile.height = 24;
		base.Projectile.friendly = false;
		base.Projectile.hostile = false;
		base.Projectile.penetrate = -1;
		base.Projectile.tileCollide = false;
		base.Projectile.ignoreWater = true;
		base.Projectile.DamageType = DamageClass.Summon;
		base.Projectile.minion = true;
		base.Projectile.minionSlots = 0f;
		base.Projectile.netImportant = true;
		base.Projectile.noEnchantmentVisuals = true;
		base.Projectile.timeLeft = 2;
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
		Player player = Main.player[base.Projectile.owner];
		if (!IsOwnerValid(player))
		{
			KillOwnedShards();
			base.Projectile.Kill();
			return;
		}
		if (MoDaoQianRenWarmupSystem.ShouldBlockBladeUseForLights)
		{
			KillOwnedShards();
			base.Projectile.Kill();
			return;
		}
		base.Projectile.Center = player.Center;
		base.Projectile.velocity = Vector2.Zero;
		base.Projectile.scale = GetOwnerBladeScale(player);
		base.Projectile.timeLeft = 2;
		int growthStage = GetGrowthStage(player);
		int shardCapacity = GetShardCapacity(player, growthStage);
		if (base.Projectile.owner == Main.myPlayer)
		{
			SyncShardSettings(shardCapacity, growthStage);
			KillExcessShards(shardCapacity);
			SpawnMissingShards(player, shardCapacity, growthStage);
		}
		MoDaoQianRenWarmupSystem.AddLight(player.Center, 0.55f, 0.08f, 1.05f);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		return false;
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

	private void SpawnMissingShards(Player player, int shardCapacity, int growthStage)
	{
		int shardType = ModContent.ProjectileType<MoDaoQianRenPrismShardProjectile>();
		bool[] usedSlots = new bool[shardCapacity];
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile shard = Main.projectile[i];
			if (shard.active && shard.owner == base.Projectile.owner && shard.type == shardType)
			{
				int slot = (int)shard.ai[0];
				if (slot >= 0 && slot < shardCapacity)
				{
					usedSlots[slot] = true;
				}
			}
		}
		for (int j = 0; j < shardCapacity; j++)
		{
			if (!usedSlots[j])
			{
				Vector2 spawnPosition = MoDaoQianRenPrismShardProjectile.GetIdlePosition(player, j, shardCapacity, 0f);
				int shardDamage = ((base.Projectile.originalDamage > 0) ? base.Projectile.originalDamage : base.Projectile.damage);
				int shardIndex = Projectile.NewProjectile(base.Projectile.GetSource_FromThis(), spawnPosition, Vector2.Zero, shardType, shardDamage, base.Projectile.knockBack, base.Projectile.owner, j, shardCapacity, growthStage);
				if (shardIndex >= 0 && shardIndex < Main.maxProjectiles)
				{
					Projectile obj = Main.projectile[shardIndex];
					obj.originalDamage = shardDamage;
					obj.netUpdate = true;
				}
			}
		}
	}

	private void SyncShardSettings(int shardCapacity, int growthStage)
	{
		int shardType = ModContent.ProjectileType<MoDaoQianRenPrismShardProjectile>();
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile shard = Main.projectile[i];
			if (shard.active && shard.owner == base.Projectile.owner && shard.type == shardType && (shard.ai[1] != (float)shardCapacity || shard.ai[2] != (float)growthStage))
			{
				shard.ai[1] = shardCapacity;
				shard.ai[2] = growthStage;
				shard.netUpdate = true;
			}
		}
	}

	private void KillExcessShards(int shardCapacity)
	{
		int shardType = ModContent.ProjectileType<MoDaoQianRenPrismShardProjectile>();
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile shard = Main.projectile[i];
			if (shard.active && shard.owner == base.Projectile.owner && shard.type == shardType)
			{
				int slot = (int)shard.ai[0];
				if (slot < 0 || slot >= shardCapacity)
				{
					shard.Kill();
				}
			}
		}
	}

	private void KillOwnedShards()
	{
		int shardType = ModContent.ProjectileType<MoDaoQianRenPrismShardProjectile>();
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile shard = Main.projectile[i];
			if (shard.active && shard.owner == base.Projectile.owner && shard.type == shardType)
			{
				shard.Kill();
			}
		}
	}

	private NPC FindTarget(Player player)
	{
		NPC closestTarget = null;
		float closestDistanceSquared = 960400f;
		if (player.HasMinionAttackTargetNPC)
		{
			NPC markedTarget = Main.npc[player.MinionAttackTargetNPC];
			if (markedTarget.CanBeChasedBy(base.Projectile))
			{
				closestDistanceSquared = Vector2.DistanceSquared(player.Center, markedTarget.Center);
				closestTarget = markedTarget;
			}
		}
		for (int i = 0; i < Main.maxNPCs; i++)
		{
			NPC npc = Main.npc[i];
			if (npc.CanBeChasedBy(base.Projectile))
			{
				float distanceSquared = Vector2.DistanceSquared(player.Center, npc.Center);
				if (!(distanceSquared >= closestDistanceSquared))
				{
					closestDistanceSquared = distanceSquared;
					closestTarget = npc;
				}
			}
		}
		return closestTarget;
	}

	private static int GetGrowthStage(Player player)
	{
		if (!MoDaoQianRen.TryGetOwnedGrowthStage(player, out var growthStage))
		{
			return 0;
		}
		return growthStage;
	}

	private static int GetShardCapacity(Player player, int growthStage)
	{
		int bonusMinionSlots = ((player.maxMinions > 1) ? (player.maxMinions - 1) : 0);
		int baseShardCount = MoDaoQianRen.GetShardPrismBaseShardCount(growthStage);
		int capacity = Utils.Clamp(baseShardCount + bonusMinionSlots * MoDaoQianRen.GetShardPrismShardsPerBonusMinionSlot(growthStage), baseShardCount, MoDaoQianRen.GetShardPrismMaxShardCount(growthStage));
		float guardMultiplier = player.GetModPlayer<MoDaoQianRenPlayer>().GetGuardShardCapacityMultiplier();
		return Math.Max(1, (int)MathF.Ceiling((float)capacity * guardMultiplier));
	}

	private static float GetOwnerBladeScale(Player player)
	{
		float scale = ((player != null && player.active && player.HeldItem?.ModItem is MoDaoQianRen) ? player.GetAdjustedItemScale(player.HeldItem) : 1f);
		if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f)
		{
			scale = 1f;
		}
		return MathHelper.Clamp(scale, 0.25f, 3f);
	}
}
