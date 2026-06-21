using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using 魔刀千刃.Content.Players;
using 魔刀千刃.Content.Projectiles;
using 魔刀千刃.Content.Systems;

namespace 魔刀千刃.Content.Items.Weapons;

public class MoDaoQianRen : ModItem
{
	public const int DormantStage = 0;

	public const int AwakenedStage = 1;

	public const int ThousandBladesStage = 2;

	public const int UnboundStage = 3;

	public const int BladeOrbStage = 4;

	public const int BladeFieldStage = 5;

	public const int MoonlitStage = 6;

	public const int FinalStage = 7;

	public const int MaxImplementedStage = 7;

	private int appliedGrowthStage = -1;

	private int appliedPrefix = -1;

	private int prefixedStageUseTime = 25;

	private int prefixedStageUseAnimation = 25;

	private int prefixedStageCrit = 4;

	private float prefixedUseTimeMultiplier = 1f;

	private float prefixedUseAnimationMultiplier = 1f;

	private int growthStageBeforeReforge = -1;

	private bool appliedCalamityBalance;

	private int appliedStageParameterRevision = -1;

	private const int InventoryIconPulseFrameCount = 8;

	private const int InventoryIconPulseFrameTicks = 5;

	private const float InventoryIconTargetSize = 44f;

	private const string CalamityModName = "CalamityMod";

	public override string Texture => ModLoader.HasMod("WeaponOut")
		? MoDaoQianRenMod.WeaponOutTexture
		: MoDaoQianRenMod.WeaponOutAnchorTexture;

	protected override bool CloneNewInstances => true;

	public int GrowthStage { get; private set; }

	protected virtual int InitialGrowthStage => 0;

	protected virtual bool FixedGrowthStage => false;

	public static bool UsesCalamityBalance => ModLoader.HasMod("CalamityMod");

	public override void SetDefaults()
	{
		GrowthStage = (FixedGrowthStage ? Utils.Clamp(InitialGrowthStage, 0, 7) : Utils.Clamp(GrowthStage, 0, 7));
		base.Item.damage = 7;
		base.Item.DamageType = DamageClass.Melee;
		base.Item.width = 80;
		base.Item.height = 28;
		base.Item.useTime = 25;
		base.Item.useAnimation = 25;
		base.Item.useStyle = 5;
		base.Item.noMelee = true;
		base.Item.noUseGraphic = true;
		base.Item.channel = true;
		base.Item.autoReuse = true;
		base.Item.attackSpeedOnlyAffectsWeaponAnimation = true;
		base.Item.knockBack = 2f;
		base.Item.rare = 0;
		base.Item.value = 0;
		base.Item.maxStack = 1;
		base.Item.UseSound = null;
		base.Item.shoot = ModContent.ProjectileType<MoDaoQianRenHeldProjectile>();
		base.Item.shootSpeed = 1f;
		ApplyGrowthStats();
		CachePrefixedUseStats();
	}

	public override bool AltFunctionUse(Player player)
	{
		MoDaoQianRenPlayer bladePlayer = player.GetModPlayer<MoDaoQianRenPlayer>();
		return !bladePlayer.IsGreatswordMode && !bladePlayer.IsGuardMode;
	}

	public override bool CanUseItem(Player player)
	{
		if (MoDaoQianRenWarmupSystem.ShouldBlockBladeUseForLights)
		{
			return false;
		}
		EnsureGrowthStatsCurrent();
		ApplyModeStats(player);
		MoDaoQianRenPlayer bladePlayer = player.GetModPlayer<MoDaoQianRenPlayer>();
		if (bladePlayer.IsShardPrismMode || bladePlayer.IsGuardMode)
		{
			return false;
		}
		if (bladePlayer.IsGreatswordDevilsMode)
		{
			return false;
		}
		bool greatswordMode = bladePlayer.IsGreatswordMode;
		bool channelingShards = player.altFunctionUse == 2;
		if (greatswordMode && channelingShards)
		{
			return false;
		}
		if (channelingShards && !CanUseShardStream(player))
		{
			return false;
		}
		base.Item.channel = true;
		base.Item.UseSound = null;
		if (channelingShards)
		{
			base.Item.useTime = ApplyUseTime(player, 12, 2);
			base.Item.useAnimation = ApplyUseAnimation(player, 12, 2);
		}
		else if (greatswordMode)
		{
			int attackTime = GetGreatswordUseTime(player);
			base.Item.useTime = ApplyUseTime(player, attackTime, 28);
			base.Item.useAnimation = ApplyUseAnimation(player, attackTime, 28);
		}
		else
		{
			float distanceFactor = GetDistanceFactor(player);
			int attackTime2 = (int)MathHelper.Lerp(GetStageUseTime(), (float)GetStageUseTime() + 8f, distanceFactor);
			base.Item.useTime = ApplyUseTime(player, attackTime2, 5);
			base.Item.useAnimation = ApplyUseAnimation(player, attackTime2, 5);
		}
		return player.ownedProjectileCounts[base.Item.shoot] == 0;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (MoDaoQianRenWarmupSystem.ShouldBlockBladeUseForLights)
		{
			return false;
		}
		MoDaoQianRenPlayer bladePlayer = player.GetModPlayer<MoDaoQianRenPlayer>();
		if (bladePlayer.IsShardPrismMode || bladePlayer.IsGuardMode)
		{
			return false;
		}
		Vector2 handPosition = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
		Vector2 aim = Main.MouseWorld - handPosition;
		if (aim == Vector2.Zero || aim.HasNaNs())
		{
			aim = Vector2.UnitX * player.direction;
		}
		bool greatswordMode = bladePlayer.IsGreatswordMode;
		float targetLength = (greatswordMode ? MathHelper.Clamp(GetScaledStageBladeLength(player), 120f, 880f) : MathHelper.Clamp(aim.Length(), 120f, 880f));
		if (!greatswordMode)
		{
			targetLength = MathHelper.Min(targetLength, GetScaledStageBladeLength(player));
		}
		Vector2 direction = aim.SafeNormalize(Vector2.UnitX * player.direction);
		bool channelingShards = player.altFunctionUse == 2 && !greatswordMode;
		int attackMode = channelingShards ? 1 : (greatswordMode ? bladePlayer.GetNextGreatswordAttackMode(GrowthStage) : bladePlayer.GetNextBladeAttackMode(GrowthStage));
		int finalDamage = damage;
		if (greatswordMode)
		{
			finalDamage = Math.Max(1, (int)MathF.Round((float)damage * GetGreatswordDamageMultiplier(GrowthStage, attackMode)));
			knockback *= GetGreatswordKnockbackMultiplier(GrowthStage, attackMode);
		}
		else if (channelingShards)
		{
			finalDamage = Math.Max(1, (int)MathF.Round((float)damage * GetShardStreamDamageMultiplier(GrowthStage)));
		}
		else
		{
			finalDamage = (int)((float)damage * MathHelper.Lerp(1f, GetBladeDistanceDamageMultiplierMax(GrowthStage), MoDaoQianRenHeldProjectile.GetDistanceFactor(targetLength)));
			switch (attackMode)
			{
			case 3:
				finalDamage = Math.Max(1, (int)MathF.Round((float)finalDamage * GetBladeHeavyDamageMultiplier(GrowthStage)));
				break;
			case 4:
				finalDamage = Math.Max(1, (int)MathF.Round((float)finalDamage * GetBladeSpinDamageMultiplier(GrowthStage)));
				break;
			}
		}
		Projectile.NewProjectile(source, handPosition, direction, type, finalDamage, knockback, player.whoAmI, attackMode, targetLength, direction.ToRotation());
		return false;
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		EnsureGrowthStatsCurrent();
		ReplacePrefixStatTooltips(tooltips);
		tooltips.Add(new TooltipLine(base.Mod, "GrowthStage", GetStageTooltip()));
		tooltips.Add(new TooltipLine(base.Mod, "GrowthStats", GetStatsTooltip()));
		tooltips.Add(new TooltipLine(base.Mod, "GrowthMechanic", GetMechanicTooltip()));
		AddTooltipLines(tooltips, "BladeModeControls", Text(GetModeControlsTooltipKey()));
		AddTooltipLines(tooltips, "BladeModeGuide", GetModeGameplayTooltip());
		if (GrowthStage < 7)
		{
			tooltips.Add(new TooltipLine(base.Mod, "GrowthNext", GetNextUpgradeTooltip(Main.LocalPlayer)));
		}
	}

	public override bool CanRightClick()
	{
		return GrowthStage < 7;
	}

	public override void RightClick(Player player)
	{
		if (!TryUpgrade(player, out var message))
		{
			Main.NewText(message, Color.OrangeRed);
			return;
		}
		FinalizeGrowthStageChange();
		PlayUpgradeEffects(player);
		Main.NewText(message, new Color(190, 95, 255));
	}

	public bool TryAutoUpgradeAfterBoss(Player player, out string message)
	{
		message = string.Empty;
		if (GrowthStage >= 7 || !IsBossGateMetForNextStage())
		{
			return false;
		}
		GrowthStage++;
		message = Text("Upgrade" + GrowthStage);
		FinalizeGrowthStageChange();
		PlayUpgradeEffects(player);
		return true;
	}

	public override bool ConsumeItem(Player player)
	{
		return false;
	}

	public override bool CanReforge()
	{
		return true;
	}

	public override void PreReforge()
	{
		growthStageBeforeReforge = GrowthStage;
	}

	public override void PostReforge()
	{
		if (!FixedGrowthStage && growthStageBeforeReforge >= 0)
		{
			GrowthStage = Utils.Clamp(growthStageBeforeReforge, 0, 7);
		}
		EnsureFixedGrowthStage();
		ApplyGrowthStatsPreservingPrefix();
		EnsureItemTypeMatchesGrowthStage();
		growthStageBeforeReforge = -1;
	}

	public override bool CanResearch()
	{
		return false;
	}

	public override bool CanStack(Item source)
	{
		return false;
	}

	public override bool CanStackInWorld(Item source)
	{
		return false;
	}

	public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		if (MoDaoQianRenWarmupSystem.ShouldSkipCustomDrawingForLights)
		{
			return false;
		}
		Texture2D texture = ModContent.Request<Texture2D>(MoDaoQianRenMod.InventoryIconPulseTexture).Value;
		int frameWidth = texture.Width / 8;
		int frameIndex = (int)(Main.GameUpdateCount / 5 % 8);
		Rectangle iconFrame = new Rectangle(frameWidth * frameIndex, 0, frameWidth, texture.Height);
		Vector2 iconOrigin = iconFrame.Size() * 0.5f;
		float inventoryDrawScale = MathF.Max(scale, 0.92f);
		float drawScale = 44f * inventoryDrawScale / MathF.Max((float)iconFrame.Width, (float)iconFrame.Height);
		spriteBatch.Draw(texture, position, iconFrame, drawColor, 0f, iconOrigin, drawScale, SpriteEffects.None, 0f);
		return false;
	}

	public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
	{
		if (MoDaoQianRenWarmupSystem.ShouldSkipCustomDrawingForLights)
		{
			return false;
		}
		Texture2D texture = ModContent.Request<Texture2D>("魔刀千刃/Content/Items/Weapons/MoDaoQianRen_weaponout_pulse").Value;
		Rectangle frame = MoDaoQianRenHeldProjectile.GetWeaponOutPulseFrame(texture);
		Vector2 drawPosition = base.Item.Center - Main.screenPosition;
		Vector2 origin = frame.Size() * 0.5f;
		spriteBatch.Draw(texture, drawPosition, frame, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
		return false;
	}

	public override void SaveData(TagCompound tag)
	{
		if (!FixedGrowthStage)
		{
			tag["GrowthStage"] = GrowthStage;
		}
	}

	public override void LoadData(TagCompound tag)
	{
		GrowthStage = (FixedGrowthStage ? Utils.Clamp(InitialGrowthStage, 0, 7) : Utils.Clamp(tag.GetInt("GrowthStage"), 0, 7));
		ApplyGrowthStatsPreservingPrefix();
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write((byte)GrowthStage);
	}

	public override void NetReceive(BinaryReader reader)
	{
		GrowthStage = Utils.Clamp(reader.ReadByte(), 0, 7);
		ApplyGrowthStatsPreservingPrefix();
	}

	public override void UpdateInventory(Player player)
	{
		EnsureFixedGrowthStage();
		EnsureGrowthStatsCurrent();
		if (!EnsureItemTypeMatchesGrowthStage())
		{
			RestorePrefixedUseStats();
			ApplyModeStats(player);
		}
	}

	public override void HoldItem(Player player)
	{
		EnsureFixedGrowthStage();
		EnsureGrowthStatsCurrent();
		if (!EnsureItemTypeMatchesGrowthStage())
		{
			RestorePrefixedUseStats();
			ApplyModeStats(player);
		}
	}

	public static bool TryGetGrowthStage(Item item, out int growthStage)
	{
		if (item?.ModItem is MoDaoQianRen blade)
		{
			growthStage = blade.GrowthStage;
			return true;
		}
		growthStage = 0;
		return false;
	}

	public static bool TryGetOwnedGrowthStage(Player player, out int growthStage)
	{
		growthStage = 0;
		if (player == null)
		{
			return false;
		}
		bool foundBlade = false;
		if (TryGetGrowthStage(player.HeldItem, out var heldStage))
		{
			growthStage = heldStage;
			foundBlade = true;
		}
		for (int i = 0; i < player.inventory.Length; i++)
		{
			if (TryGetGrowthStage(player.inventory[i], out var inventoryStage))
			{
				growthStage = Math.Max(growthStage, inventoryStage);
				foundBlade = true;
			}
		}
		return foundBlade;
	}

	public void ApplyModeStats(Player player)
	{
		MoDaoQianRenPlayer bladePlayer = player?.GetModPlayer<MoDaoQianRenPlayer>();
		if (bladePlayer != null && (bladePlayer.IsShardPrismMode || bladePlayer.IsGuardMode))
		{
			base.Item.DamageType = DamageClass.Summon;
			base.Item.useStyle = 5;
			base.Item.noMelee = true;
			base.Item.noUseGraphic = true;
			base.Item.channel = false;
			base.Item.autoReuse = false;
			base.Item.attackSpeedOnlyAffectsWeaponAnimation = false;
			base.Item.crit = prefixedStageCrit;
			base.Item.UseSound = null;
			base.Item.shoot = ModContent.ProjectileType<MoDaoQianRenPrismBladeProjectile>();
			base.Item.shootSpeed = 0f;
			base.Item.useTime = ApplyUseTimePrefix(GetShardPrismSetupUseTime(GrowthStage));
			base.Item.useAnimation = base.Item.useTime;
		}
		else
		{
			base.Item.DamageType = DamageClass.Melee;
			base.Item.useStyle = 5;
			base.Item.noMelee = true;
			base.Item.noUseGraphic = true;
			base.Item.channel = true;
			base.Item.autoReuse = true;
			base.Item.attackSpeedOnlyAffectsWeaponAnimation = true;
			base.Item.crit = ((bladePlayer != null && bladePlayer.IsGreatswordMode) ? GetGreatswordCritChance(GrowthStage) : prefixedStageCrit);
			base.Item.UseSound = null;
			base.Item.shoot = ModContent.ProjectileType<MoDaoQianRenHeldProjectile>();
			base.Item.shootSpeed = 1f;
		}
	}

	private static float GetDistanceFactor(Player player)
	{
		Vector2 handPosition = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
		return MoDaoQianRenHeldProjectile.GetDistanceFactor(MathHelper.Clamp((Main.MouseWorld - handPosition).Length(), 120f, 880f));
	}

	private int GetGreatswordUseTime(Player player)
	{
		float distanceFactor = MoDaoQianRenHeldProjectile.GetDistanceFactor(MathHelper.Clamp(GetScaledStageBladeLength(player), 120f, 880f));
		if (UsesCalamityBalance)
		{
			int growthStage = GrowthStage;
			if (growthStage >= 6)
			{
				if (growthStage >= 7)
				{
					return (int)MathHelper.Lerp(40f, 52f, distanceFactor);
				}
				return (int)MathHelper.Lerp(42f, 54f, distanceFactor);
			}
			if (growthStage >= 5)
			{
				return (int)MathHelper.Lerp(44f, 56f, distanceFactor);
			}
			return (int)MathHelper.Lerp(46f, 58f, distanceFactor);
		}
		int growthStage2 = GrowthStage;
		if (growthStage2 >= 6)
		{
			if (growthStage2 >= 7)
			{
				return (int)MathHelper.Lerp(42f, 54f, distanceFactor);
			}
			return (int)MathHelper.Lerp(44f, 56f, distanceFactor);
		}
		if (growthStage2 >= 5)
		{
			return (int)MathHelper.Lerp(46f, 58f, distanceFactor);
		}
		return (int)MathHelper.Lerp(48f, 60f, distanceFactor);
	}

	public static bool IsShardPrismModeUnlocked(int growthStage)
	{
		return growthStage >= 4;
	}

	public static bool IsGreatswordModeUnlocked(int growthStage)
	{
		return growthStage >= 5;
	}

	public static bool IsGuardModeUnlocked(int growthStage)
	{
		return IsShardPrismModeUnlocked(growthStage);
	}

	public static bool IsGreatswordDevilsModeUnlocked(int growthStage)
	{
		return IsGreatswordModeUnlocked(growthStage);
	}

	public static int GetShardPrismSetupUseTime(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).ShardPrismSetupUseTime;
	}

	public static int GetShardPrismBaseShardCount(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).ShardPrismBaseShardCount;
	}

	public static int GetShardPrismShardsPerBonusMinionSlot(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).ShardPrismShardsPerBonusMinionSlot;
	}

	public static int GetShardPrismMaxShardCount(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).ShardPrismMaxShardCount;
	}

	public static float GetShardPrismDamageMultiplier(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).ShardPrismDamageMultiplier;
	}

	public static int GetGreatswordCritChance(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).GreatswordCritChance;
	}

	public static float GetGreatswordDamageMultiplier(int growthStage, int attackMode)
	{
		bool num = attackMode == MoDaoQianRenHeldProjectile.GreatswordHeavySlashAttackMode;
		MoDaoQianRenStageStats stats = MoDaoQianRenStageParameterSystem.GetStageStats(growthStage);
		if (!num)
		{
			return stats.GreatswordDamageMultiplier;
		}
		return stats.GreatswordHeavyDamageMultiplier;
	}

	public static float GetGreatswordKnockbackMultiplier(int growthStage, int attackMode)
	{
		bool num = attackMode == MoDaoQianRenHeldProjectile.GreatswordHeavySlashAttackMode;
		MoDaoQianRenStageStats stats = MoDaoQianRenStageParameterSystem.GetStageStats(growthStage);
		if (!num)
		{
			return stats.GreatswordKnockbackMultiplier;
		}
		return stats.GreatswordHeavyKnockbackMultiplier;
	}

	public static int GetGreatswordAttackDuration(int growthStage, float bladeLength, int attackMode)
	{
		float distanceFactor = MoDaoQianRenHeldProjectile.GetDistanceFactor(bladeLength);
		bool heavySlash = attackMode == 7;
		bool reverseSlash = attackMode == 6;
		if (UsesCalamityBalance)
		{
			if (growthStage >= 7)
			{
				if (!heavySlash)
				{
					if (!reverseSlash)
					{
						return (int)MathHelper.Lerp(40f, 52f, distanceFactor);
					}
					return (int)MathHelper.Lerp(42f, 54f, distanceFactor);
				}
				return (int)MathHelper.Lerp(64f, 78f, distanceFactor);
			}
			if (growthStage >= 6)
			{
				if (!heavySlash)
				{
					if (!reverseSlash)
					{
						return (int)MathHelper.Lerp(42f, 54f, distanceFactor);
					}
					return (int)MathHelper.Lerp(44f, 56f, distanceFactor);
				}
				return (int)MathHelper.Lerp(68f, 82f, distanceFactor);
			}
			if (!heavySlash)
			{
				if (!reverseSlash)
				{
					return (int)MathHelper.Lerp(44f, 56f, distanceFactor);
				}
				return (int)MathHelper.Lerp(46f, 58f, distanceFactor);
			}
			return (int)MathHelper.Lerp(72f, 86f, distanceFactor);
		}
		if (growthStage >= 7)
		{
			if (!heavySlash)
			{
				if (!reverseSlash)
				{
					return (int)MathHelper.Lerp(42f, 54f, distanceFactor);
				}
				return (int)MathHelper.Lerp(44f, 56f, distanceFactor);
			}
			return (int)MathHelper.Lerp(68f, 82f, distanceFactor);
		}
		if (growthStage >= 6)
		{
			if (!heavySlash)
			{
				if (!reverseSlash)
				{
					return (int)MathHelper.Lerp(44f, 56f, distanceFactor);
				}
				return (int)MathHelper.Lerp(46f, 58f, distanceFactor);
			}
			return (int)MathHelper.Lerp(72f, 86f, distanceFactor);
		}
		if (!heavySlash)
		{
			if (!reverseSlash)
			{
				return (int)MathHelper.Lerp(46f, 58f, distanceFactor);
			}
			return (int)MathHelper.Lerp(48f, 60f, distanceFactor);
		}
		return (int)MathHelper.Lerp(76f, 90f, distanceFactor);
	}

	public static int GetGreatswordMinimumAttackDuration(int growthStage, int attackMode)
	{
		bool heavySlash = attackMode == 7;
		if (UsesCalamityBalance)
		{
			if (growthStage >= 7)
			{
				if (!heavySlash)
				{
					return 30;
				}
				return 48;
			}
			if (growthStage >= 6)
			{
				if (!heavySlash)
				{
					return 32;
				}
				return 50;
			}
			if (!heavySlash)
			{
				return 34;
			}
			return 52;
		}
		if (growthStage >= 7)
		{
			if (!heavySlash)
			{
				return 32;
			}
			return 50;
		}
		if (growthStage >= 6)
		{
			if (!heavySlash)
			{
				return 34;
			}
			return 52;
		}
		if (!heavySlash)
		{
			return 36;
		}
		return 54;
	}

	public static int GetShardStreamConsumeRate(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).ShardStreamConsumeRate;
	}

	public static int GetShardStreamShardCount(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).ShardStreamShardCount;
	}

	public static float GetShardStreamDamageMultiplier(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).ShardStreamDamageMultiplier;
	}

	public static float GetComboShardDamageMultiplier(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).ComboShardDamageMultiplier;
	}

	public static float GetBladeDistanceDamageMultiplierMax(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).BladeDistanceDamageMultiplierMax;
	}

	public static float GetBladeHeavyDamageMultiplier(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).BladeHeavyDamageMultiplier;
	}

	public static float GetBladeSpinDamageMultiplier(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).BladeSpinDamageMultiplier;
	}

	public static float GetGreatswordAssistShardDamageMultiplier(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).GreatswordAssistShardDamageMultiplier;
	}

	public static float GetFinalAssistShardDamageMultiplier(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).FinalAssistShardDamageMultiplier;
	}

	public static float GetFieldBurstDamageMultiplier(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).FieldBurstDamageMultiplier;
	}

	public static float GetFinalConvergenceShardDamageMultiplier(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).FinalConvergenceShardDamageMultiplier;
	}

	public static MoDaoQianRenStageStats GetRuntimeStats(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage);
	}

	private bool CanUseShardStream(Player player)
	{
		return player.GetModPlayer<MoDaoQianRenPlayer>().ShardCharge > 0;
	}

	private void EnsureFixedGrowthStage()
	{
		if (FixedGrowthStage)
		{
			int fixedStage = Utils.Clamp(InitialGrowthStage, 0, 7);
			if (GrowthStage != fixedStage)
			{
				GrowthStage = fixedStage;
				ApplyGrowthStatsPreservingPrefix();
			}
		}
	}

	private void EnsureGrowthStatsCurrent()
	{
		if (appliedGrowthStage != GrowthStage || appliedPrefix != base.Item.prefix || appliedCalamityBalance != UsesCalamityBalance || appliedStageParameterRevision != MoDaoQianRenStageParameterSystem.Revision)
		{
			ApplyGrowthStatsPreservingPrefix();
		}
	}

	public void RefreshGrowthStatsFromCustomParameters()
	{
		ApplyGrowthStatsPreservingPrefix();
	}

	private void ApplyGrowthStatsPreservingPrefix()
	{
		int prefix = base.Item.prefix;
		ApplyGrowthStats();
		if (prefix > 0)
		{
			base.Item.Prefix(prefix);
		}
		CachePrefixedUseStats();
	}

	private void FinalizeGrowthStageChange()
	{
		if (!EnsureItemTypeMatchesGrowthStage())
		{
			ApplyGrowthStatsPreservingPrefix();
		}
	}

	private bool EnsureItemTypeMatchesGrowthStage()
	{
		int targetType = GetItemTypeForGrowthStage(GrowthStage);
		if (base.Item.type == targetType)
		{
			return false;
		}
		int stage = GrowthStage;
		int prefix = base.Item.prefix;
		base.Item.ChangeItemType(targetType);
		if (base.Item.ModItem is MoDaoQianRen blade)
		{
			blade.GrowthStage = Utils.Clamp(stage, 0, 7);
			blade.ApplyGrowthStats();
			if (prefix > 0)
			{
				base.Item.Prefix(prefix);
			}
			blade.CachePrefixedUseStats();
		}
		return true;
	}

	private static int GetItemTypeForGrowthStage(int growthStage)
	{
		return growthStage switch
		{
			7 => ModContent.ItemType<MoDaoQianRenStage7>(), 
			6 => ModContent.ItemType<MoDaoQianRenStage6>(), 
			5 => ModContent.ItemType<MoDaoQianRenStage5>(), 
			4 => ModContent.ItemType<MoDaoQianRenStage4>(), 
			3 => ModContent.ItemType<MoDaoQianRenStage3>(), 
			2 => ModContent.ItemType<MoDaoQianRenStage2>(), 
			1 => ModContent.ItemType<MoDaoQianRenStage1>(), 
			_ => ModContent.ItemType<MoDaoQianRen>(), 
		};
	}

	private void ApplyGrowthStats()
	{
		MoDaoQianRenStageStats stageStats = MoDaoQianRenStageParameterSystem.GetStageStats(GrowthStage);
		base.Item.DamageType = DamageClass.Melee;
		base.Item.useStyle = 5;
		base.Item.noMelee = true;
		base.Item.noUseGraphic = true;
		base.Item.channel = true;
		base.Item.autoReuse = true;
		base.Item.attackSpeedOnlyAffectsWeaponAnimation = true;
		base.Item.damage = stageStats.Damage;
		base.Item.knockBack = stageStats.KnockBack;
		base.Item.crit = stageStats.CritChance;
		base.Item.scale = 1f;
		base.Item.shootSpeed = 1f;
		Item item = base.Item;
		item.rare = GrowthStage switch
		{
			7 => 11, 
			6 => 8, 
			5 => 7, 
			4 => 4, 
			3 => 3, 
			1 => 1, 
			2 => 2, 
			_ => 0, 
		};
		base.Item.value = GetStageItemValue();
		base.Item.useTime = stageStats.UseTime;
		base.Item.useAnimation = base.Item.useTime;
	}

	private int GetStageDamage()
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(GrowthStage).Damage;
	}

	private float GetStageKnockBack()
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(GrowthStage).KnockBack;
	}

	private int GetStageItemValue()
	{
		return GrowthStage switch
		{
			7 => Item.sellPrice(0, 25), 
			6 => Item.sellPrice(0, 15), 
			5 => Item.sellPrice(0, 8), 
			4 => Item.sellPrice(0, 5), 
			3 => Item.sellPrice(0, 2), 
			2 => Item.sellPrice(0, 0, 80), 
			1 => Item.sellPrice(0, 0, 25), 
			_ => Item.sellPrice(0, 0, 10), 
		};
	}

	private void CachePrefixedUseStats()
	{
		int stageUseTime = GetStageUseTime();
		prefixedStageUseTime = base.Item.useTime;
		prefixedStageUseAnimation = base.Item.useAnimation;
		prefixedStageCrit = base.Item.crit;
		prefixedUseTimeMultiplier = ((stageUseTime > 0) ? ((float)prefixedStageUseTime / (float)stageUseTime) : 1f);
		prefixedUseAnimationMultiplier = ((stageUseTime > 0) ? ((float)prefixedStageUseAnimation / (float)stageUseTime) : prefixedUseTimeMultiplier);
		appliedGrowthStage = GrowthStage;
		appliedPrefix = base.Item.prefix;
		appliedCalamityBalance = UsesCalamityBalance;
		appliedStageParameterRevision = MoDaoQianRenStageParameterSystem.Revision;
	}

	private void RestorePrefixedUseStats()
	{
		base.Item.useTime = prefixedStageUseTime;
		base.Item.useAnimation = prefixedStageUseAnimation;
	}

	public int ApplyUseTimePrefix(int baseUseTime)
	{
		return Utils.Clamp((int)MathF.Round((float)baseUseTime * prefixedUseTimeMultiplier), 1, 999);
	}

	public int ApplyUseTime(Player player, int baseUseTime, int minimumFrames = 1)
	{
		return ApplyMeleeAttackSpeed(player, ApplyUseTimePrefix(baseUseTime), minimumFrames);
	}

	private int ApplyUseAnimationPrefix(int baseUseAnimation)
	{
		return Utils.Clamp((int)MathF.Round((float)baseUseAnimation * prefixedUseAnimationMultiplier), 1, 999);
	}

	private int ApplyUseAnimation(Player player, int baseUseAnimation, int minimumFrames = 1)
	{
		return ApplyMeleeAttackSpeed(player, ApplyUseAnimationPrefix(baseUseAnimation), minimumFrames);
	}

	public static int ApplyMeleeAttackSpeed(Player player, int frames, int minimumFrames = 1)
	{
		float attackSpeed = ((player != null) ? player.GetAttackSpeed(DamageClass.Melee) : 1f);
		if (float.IsNaN(attackSpeed) || float.IsInfinity(attackSpeed) || attackSpeed <= 0f)
		{
			attackSpeed = 1f;
		}
		return Utils.Clamp((int)MathF.Round((float)frames / attackSpeed), minimumFrames, 999);
	}

	private int GetStageUseTime()
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(GrowthStage).UseTime;
	}

	private float GetStageBladeLength()
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(GrowthStage).BladeLength;
	}

	public float GetScaledStageBladeLength(Player player)
	{
		return GetStageBladeLength() * GetAdjustedBladeScale(player);
	}

	public float GetWeaponOutStageBladeLength()
	{
		return GetStageBladeLength() * GetWeaponOnlyScale();
	}

	private float GetAdjustedBladeScale(Player player)
	{
		float scale = ((player != null && player.active) ? player.GetAdjustedItemScale(base.Item) : base.Item.scale);
		if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f)
		{
			scale = 1f;
		}
		return MathHelper.Clamp(scale, 0.25f, 3f);
	}

	private float GetWeaponOnlyScale()
	{
		float scale = base.Item.scale;
		if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f)
		{
			scale = 1f;
		}
		return MathHelper.Clamp(scale, 0.25f, 3f);
	}

	private string GetStageTooltip()
	{
		return GrowthStage switch
		{
			7 => Text("Stage7"), 
			6 => Text("Stage6"), 
			5 => Text("Stage5"), 
			4 => Text("Stage4"), 
			3 => Text("Stage3"), 
			1 => Text("Stage1"), 
			2 => Text("Stage2"), 
			_ => Text("Stage0"), 
		};
	}

	private string GetMechanicTooltip()
	{
		return GrowthStage switch
		{
			7 => Text("Mechanic7"), 
			6 => Text("Mechanic6"), 
			5 => Text("Mechanic5"), 
			4 => Text("Mechanic4"), 
			3 => Text("Mechanic3"), 
			1 => Text("Mechanic1"), 
			2 => Text("Mechanic2"), 
			_ => Text("Mechanic0"), 
		};
	}

	private string GetModeControlsTooltipKey()
	{
		if (IsGreatswordModeUnlocked(GrowthStage))
		{
			return "ModeControlsUnlocked";
		}
		if (!IsShardPrismModeUnlocked(GrowthStage))
		{
			return "ModeControlsLocked";
		}
		return "ModeControlsShardUnlocked";
	}

	private string GetModeGameplayTooltip()
	{
		int currentMode = 1;
		if (Main.LocalPlayer != null)
		{
			currentMode = Main.LocalPlayer.GetModPlayer<MoDaoQianRenPlayer>().CurrentBladeMode;
		}
		return Text(GetModeGameplayTooltipKey(currentMode));
	}

	private string GetModeGameplayTooltipKey(int mode)
	{
		return mode switch
		{
			5 => IsGreatswordDevilsModeUnlocked(GrowthStage) ? "ModeGameplay5" : "ModeGameplay5Locked",
			4 => IsGuardModeUnlocked(GrowthStage) ? "ModeGameplay4" : "ModeGameplay4Locked",
			3 => IsGreatswordModeUnlocked(GrowthStage) ? "ModeGameplay3" : "ModeGameplay3Locked",
			2 => IsShardPrismModeUnlocked(GrowthStage) ? "ModeGameplay2" : "ModeGameplay2Locked",
			_ => "ModeGameplay1",
		};
	}

	private string GetStatsTooltip()
	{
		return Language.GetTextValue(GetKey("Stats"), base.Item.damage, GetScaledStageBladeLength(Main.LocalPlayer), ApplyUseTime(Main.LocalPlayer, GetStageUseTime()));
	}

	private void ReplacePrefixStatTooltips(List<TooltipLine> tooltips)
	{
		for (int i = 0; i < tooltips.Count; i++)
		{
			TooltipLine line = tooltips[i];
			if (line.Mod == "Terraria" && (line.Name == "PrefixDamage" || line.Name == "PrefixSpeed" || line.Name == "PrefixCritChance" || line.Name == "PrefixKnockback" || line.Name == "NoSpeedScaling"))
			{
				line.Hide();
			}
		}
		if (base.Item.prefix > 0)
		{
			AddPercentPrefixTooltip(tooltips, "PrefixDamage", GetRelativePercent(base.Item.damage, GetStageDamage()), "PrefixDamageBonus", "PrefixDamagePenalty");
			AddPercentPrefixTooltip(tooltips, "PrefixSpeed", GetUseSpeedPercent(), "PrefixSpeedBonus", "PrefixSpeedPenalty");
			AddFlatPrefixTooltip(tooltips, "PrefixCritChance", prefixedStageCrit - GetCurrentStageCritChance(), "PrefixCritBonus", "PrefixCritPenalty");
			AddPercentPrefixTooltip(tooltips, "PrefixKnockback", GetRelativePercent(base.Item.knockBack, GetStageKnockBack()), "PrefixKnockbackBonus", "PrefixKnockbackPenalty");
		}
	}

	private void AddPercentPrefixTooltip(List<TooltipLine> tooltips, string name, int percent, string bonusKey, string penaltyKey)
	{
		if (percent != 0)
		{
			AddPrefixTooltip(tooltips, name, percent, (percent > 0) ? bonusKey : penaltyKey);
		}
	}

	private void AddFlatPrefixTooltip(List<TooltipLine> tooltips, string name, int value, string bonusKey, string penaltyKey)
	{
		if (value != 0)
		{
			AddPrefixTooltip(tooltips, name, value, (value > 0) ? bonusKey : penaltyKey);
		}
	}

	private void AddPrefixTooltip(List<TooltipLine> tooltips, string name, int value, string key)
	{
		TooltipLine line = new TooltipLine(base.Mod, name, Language.GetTextValue(GetKey(key), Math.Abs(value)))
		{
			IsModifier = true,
			IsModifierBad = (value < 0)
		};
		tooltips.Add(line);
	}

	private void AddTooltipLines(List<TooltipLine> tooltips, string name, string text)
	{
		string[] lines = text.Split(new string[] { "\r\n", "\n", "\\n" }, StringSplitOptions.None);
		for (int i = 0; i < lines.Length; i++)
		{
			if (!string.IsNullOrWhiteSpace(lines[i]))
			{
				tooltips.Add(new TooltipLine(base.Mod, name + i, lines[i]));
			}
		}
	}

	private int GetRelativePercent(float currentValue, float baseValue)
	{
		if (baseValue <= 0f)
		{
			return 0;
		}
		return (int)MathF.Round((currentValue / baseValue - 1f) * 100f);
	}

	private int GetUseSpeedPercent()
	{
		int stageUseTime = GetStageUseTime();
		if (stageUseTime <= 0)
		{
			return 0;
		}
		return (int)MathF.Round((1f - (float)prefixedStageUseTime / (float)stageUseTime) * 100f);
	}

	private static int GetStageCritChance()
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(0).CritChance;
	}

	private int GetCurrentStageCritChance()
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(GrowthStage).CritChance;
	}

	private string GetNextUpgradeTooltip(Player player)
	{
		string status = ((GrowthStage switch
		{
			0 => IsFirstBossDowned() && HasItem(player, 75, 3) && HasItem(player, 38, 4), 
			1 => IsSkeletronOrQueenBeeDowned() && HasEitherItem(player, 154, 30, 2431, 12) && HasEitherItem(player, 57, 10, 1257, 10), 
			2 => Main.hardMode && HasItem(player, 175, 15) && HasItem(player, 502, 12) && HasEitherItem(player, 520, 8, 521, 8), 
			3 => IsAnyMechBossDowned() && HasItem(player, 1225, 12) && HasAnyMechSoul(player, 8), 
			4 => IsPlanteraDowned() && HasItem(player, 1006, 18) && HasItem(player, 1508, 10) && HasItem(player, 1291, 1) && HasEitherItem(player, 86, 25, 1329, 25), 
			5 => IsGolemDowned() && HasItem(player, 2218, 12) && HasItem(player, 2767, 1) && HasItem(player, 1508, 12), 
			6 => IsMoonLordDowned() && HasItem(player, 3467, 12) && HasItem(player, 3458, 8) && HasItem(player, 3456, 8) && HasItem(player, 3457, 8) && HasItem(player, 3459, 8), 
			_ => false, 
		}) ? Text("Ready") : Text("Missing"));
		return GrowthStage switch
		{
			0 => Language.GetTextValue(GetKey("Next1"), status), 
			1 => Language.GetTextValue(GetKey("Next2"), status), 
			2 => Language.GetTextValue(GetKey("Next3"), status), 
			3 => Language.GetTextValue(GetKey("Next4"), status), 
			4 => Language.GetTextValue(GetKey("Next5"), status), 
			5 => Language.GetTextValue(GetKey("Next6"), status), 
			6 => Language.GetTextValue(GetKey("Next7"), status), 
			_ => string.Empty, 
		};
	}

	private bool TryUpgrade(Player player, out string message)
	{
		if (GrowthStage == 0)
		{
			if (!IsFirstBossDowned())
			{
				message = Text("NeedFirstBoss");
				return false;
			}
			if (!HasItem(player, 75, 3) || !HasItem(player, 38, 4))
			{
				message = Text("NeedStage1Items");
				return false;
			}
			ConsumeItems(player, 75, 3);
			ConsumeItems(player, 38, 4);
			GrowthStage = 1;
			message = Text("Upgrade1");
			return true;
		}
		if (GrowthStage == 1)
		{
			if (!IsSkeletronOrQueenBeeDowned())
			{
				message = Text("NeedSkeletronOrQueenBee");
				return false;
			}
			if (!HasEitherItem(player, 154, 30, 2431, 12) || !HasEitherItem(player, 57, 10, 1257, 10))
			{
				message = Text("NeedStage2Items");
				return false;
			}
			ConsumeEitherItems(player, 154, 30, 2431, 12);
			ConsumeEitherItems(player, 57, 10, 1257, 10);
			GrowthStage = 2;
			message = Text("Upgrade2");
			return true;
		}
		if (GrowthStage == 2)
		{
			if (!Main.hardMode)
			{
				message = Text("NeedHardmode");
				return false;
			}
			if (!HasItem(player, 175, 15) || !HasItem(player, 502, 12) || !HasEitherItem(player, 520, 8, 521, 8))
			{
				message = Text("NeedStage3Items");
				return false;
			}
			ConsumeItems(player, 175, 15);
			ConsumeItems(player, 502, 12);
			ConsumeEitherItems(player, 520, 8, 521, 8);
			GrowthStage = 3;
			message = Text("Upgrade3");
			return true;
		}
		if (GrowthStage == 3)
		{
			if (!IsAnyMechBossDowned())
			{
				message = Text("NeedMechBoss");
				return false;
			}
			if (!HasItem(player, 1225, 12) || !HasAnyMechSoul(player, 8))
			{
				message = Text("NeedStage4Items");
				return false;
			}
			ConsumeItems(player, 1225, 12);
			ConsumeAnyMechSoul(player, 8);
			GrowthStage = 4;
			message = Text("Upgrade4");
			return true;
		}
		if (GrowthStage == 4)
		{
			if (!IsPlanteraDowned())
			{
				message = Text("NeedPlantera");
				return false;
			}
			if (!HasItem(player, 1006, 18) || !HasItem(player, 1508, 10) || !HasItem(player, 1291, 1) || !HasEitherItem(player, 86, 25, 1329, 25))
			{
				message = Text("NeedStage5Items");
				return false;
			}
			ConsumeItems(player, 1006, 18);
			ConsumeItems(player, 1508, 10);
			ConsumeItems(player, 1291, 1);
			ConsumeEitherItems(player, 86, 25, 1329, 25);
			GrowthStage = 5;
			message = Text("Upgrade5");
			return true;
		}
		if (GrowthStage == 5)
		{
			if (!IsGolemDowned())
			{
				message = Text("NeedGolem");
				return false;
			}
			if (!HasItem(player, 2218, 12) || !HasItem(player, 2767, 1) || !HasItem(player, 1508, 12))
			{
				message = Text("NeedStage6Items");
				return false;
			}
			ConsumeItems(player, 2218, 12);
			ConsumeItems(player, 2767, 1);
			ConsumeItems(player, 1508, 12);
			GrowthStage = 6;
			message = Text("Upgrade6");
			return true;
		}
		if (GrowthStage == 6)
		{
			if (!IsMoonLordDowned())
			{
				message = Text("NeedMoonLord");
				return false;
			}
			if (!HasItem(player, 3467, 12) || !HasItem(player, 3458, 8) || !HasItem(player, 3456, 8) || !HasItem(player, 3457, 8) || !HasItem(player, 3459, 8))
			{
				message = Text("NeedStage7Items");
				return false;
			}
			ConsumeItems(player, 3467, 12);
			ConsumeItems(player, 3458, 8);
			ConsumeItems(player, 3456, 8);
			ConsumeItems(player, 3457, 8);
			ConsumeItems(player, 3459, 8);
			GrowthStage = 7;
			message = Text("Upgrade7");
			return true;
		}
		message = Text("CurrentLimit");
		return false;
	}

	private bool IsBossGateMetForNextStage()
	{
		return GrowthStage switch
		{
			0 => IsFirstBossDowned(), 
			1 => IsSkeletronOrQueenBeeDowned(), 
			2 => Main.hardMode, 
			3 => IsAnyMechBossDowned(), 
			4 => IsPlanteraDowned(), 
			5 => IsGolemDowned(), 
			6 => IsMoonLordDowned(), 
			_ => false, 
		};
	}

	private void PlayUpgradeEffects(Player player)
	{
		SoundStyle style = SoundID.Item4 with
		{
			Volume = 0.9f,
			Pitch = 0.15f
		};
		SoundEngine.PlaySound(in style, player.Center);
		style = SoundID.Item29 with
		{
			Volume = 0.55f,
			Pitch = -0.25f
		};
		SoundEngine.PlaySound(in style, player.Center);
		if (!Main.dedServ)
		{
			Color textColor = ((GrowthStage >= 2) ? new Color(225, 160, 255) : new Color(190, 95, 255));
			CombatText.NewText(player.Hitbox, textColor, Text("AwakenedCombatText"), dramatic: true);
			int dustCount = ((GrowthStage >= 7) ? 240 : ((GrowthStage >= 6) ? 190 : ((GrowthStage >= 5) ? 156 : ((GrowthStage >= 4) ? 124 : ((GrowthStage >= 3) ? 96 : ((GrowthStage >= 2) ? 72 : 52))))));
			float radius = ((GrowthStage >= 7) ? 220f : ((GrowthStage >= 6) ? 178f : ((GrowthStage >= 5) ? 150f : ((GrowthStage >= 4) ? 128f : ((GrowthStage >= 3) ? 108f : ((GrowthStage >= 2) ? 86f : 64f))))));
			for (int i = 0; i < dustCount; i++)
			{
				float progress = (float)i / (float)dustCount;
				Vector2 direction = ((float)Math.PI * 2f * progress).ToRotationVector2();
				Dust dust = Dust.NewDustPerfect(player.Center + direction * Main.rand.NextFloat(radius * 0.35f, radius), Main.rand.NextBool(3) ? 242 : 62, direction * Main.rand.NextFloat(2.2f, 5.2f) - Vector2.UnitY * Main.rand.NextFloat(0.4f, 1.8f), 60, new Color(205, 105, 255), Main.rand.NextFloat(1.15f, 2.05f));
				dust.noGravity = true;
				dust.fadeIn = Main.rand.NextFloat(0.35f, 0.9f);
			}
			for (int j = 0; j < 18; j++)
			{
				Vector2 position = player.Center + Main.rand.NextVector2Circular(34f, 44f);
				Vector2 velocity = (player.Center - position).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(2.6f, 5.8f);
				Dust.NewDustPerfect(position, 27, velocity, 80, Color.White, Main.rand.NextFloat(0.75f, 1.25f)).noGravity = true;
			}
			MoDaoQianRenWarmupSystem.AddLight(player.Center, 1.1f, 0.25f, 1.7f);
		}
	}

	private string Text(string suffix)
	{
		return Language.GetTextValue(GetKey(suffix));
	}

	private string GetKey(string suffix)
	{
		return "Mods." + base.Mod.Name + ".Items.MoDaoQianRen." + suffix;
	}

	private static bool IsFirstBossDowned()
	{
		if (!NPC.downedSlimeKing && !NPC.downedBoss1 && !NPC.downedBoss2 && !NPC.downedQueenBee)
		{
			return NPC.downedBoss3;
		}
		return true;
	}

	private static bool IsSkeletronOrQueenBeeDowned()
	{
		if (!NPC.downedBoss3)
		{
			return NPC.downedQueenBee;
		}
		return true;
	}

	private static bool IsAnyMechBossDowned()
	{
		if (!NPC.downedMechBoss1 && !NPC.downedMechBoss2)
		{
			return NPC.downedMechBoss3;
		}
		return true;
	}

	private static bool IsPlanteraDowned()
	{
		return NPC.downedPlantBoss;
	}

	private static bool IsGolemDowned()
	{
		return NPC.downedGolemBoss;
	}

	private static bool IsMoonLordDowned()
	{
		return NPC.downedMoonlord;
	}

	private static bool HasItem(Player player, int itemType, int stack)
	{
		return player.CountItem(itemType) >= stack;
	}

	private static bool HasEitherItem(Player player, int firstItemType, int firstStack, int secondItemType, int secondStack)
	{
		if (!HasItem(player, firstItemType, firstStack))
		{
			return HasItem(player, secondItemType, secondStack);
		}
		return true;
	}

	private static bool HasAnyMechSoul(Player player, int stack)
	{
		if (!HasItem(player, 548, stack) && !HasItem(player, 549, stack))
		{
			return HasItem(player, 547, stack);
		}
		return true;
	}

	private static void ConsumeItems(Player player, int itemType, int stack)
	{
		for (int i = 0; i < stack; i++)
		{
			player.ConsumeItem(itemType);
		}
	}

	private static void ConsumeEitherItems(Player player, int firstItemType, int firstStack, int secondItemType, int secondStack)
	{
		if (HasItem(player, firstItemType, firstStack))
		{
			ConsumeItems(player, firstItemType, firstStack);
		}
		else
		{
			ConsumeItems(player, secondItemType, secondStack);
		}
	}

	private static void ConsumeAnyMechSoul(Player player, int stack)
	{
		if (HasItem(player, 548, stack))
		{
			ConsumeItems(player, 548, stack);
		}
		else if (HasItem(player, 549, stack))
		{
			ConsumeItems(player, 549, stack);
		}
		else
		{
			ConsumeItems(player, 547, stack);
		}
	}
}
