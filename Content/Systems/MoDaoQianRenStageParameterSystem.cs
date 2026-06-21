using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using 魔刀千刃.Content.Items.Weapons;

namespace 魔刀千刃.Content.Systems;

public class MoDaoQianRenStageParameterSystem : ModSystem
{
	private sealed class StageParameterSaveData
	{
		public bool AutoUpgradeAfterBoss { get; set; }

		public List<StageParameterRecord> Stages { get; set; } = new List<StageParameterRecord>();
	}

	private sealed class StageParameterRecord
	{
		public int Stage { get; set; }

		public bool Customized { get; set; }

		public MoDaoQianRenStageStats Stats { get; set; }
	}

	private const int StageCount = 8;

	private static readonly MoDaoQianRenStageStats[] StageStats = new MoDaoQianRenStageStats[8];

	private static readonly bool[] CustomizedStages = new bool[8];

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true
	};

	public static int Revision { get; private set; }

	public static bool AutoUpgradeAfterBoss { get; private set; }

	private static string SavePath => Path.Combine(Main.SavePath, "ModConfigs", "MoDaoQianRenStageParameters.json");

	public override void Load()
	{
		AutoUpgradeAfterBoss = false;
		ResetToDefaults();
		LoadSavedParameters();
	}

	public override void Unload()
	{
		for (int i = 0; i < StageStats.Length; i++)
		{
			StageStats[i] = null;
			CustomizedStages[i] = false;
		}
		Revision = 0;
		AutoUpgradeAfterBoss = false;
	}

	public static MoDaoQianRenStageStats GetStageStats(int growthStage)
	{
		EnsureLoaded();
		return StageStats[ClampStage(growthStage)];
	}

	public static MoDaoQianRenStageStats GetEditableStageStats(int growthStage)
	{
		return GetStageStats(growthStage).Clone();
	}

	public static bool HasCustomStats(int growthStage)
	{
		EnsureLoaded();
		return CustomizedStages[ClampStage(growthStage)];
	}

	public static void SaveStageStats(int growthStage, MoDaoQianRenStageStats stats)
	{
		EnsureLoaded();
		int stage = ClampStage(growthStage);
		StageStats[stage] = Sanitize(stats, GetDefaultStats(stage));
		CustomizedStages[stage] = true;
		Revision++;
		SaveParameters();
	}

	public static void ResetStageStats(int growthStage)
	{
		EnsureLoaded();
		int stage = ClampStage(growthStage);
		StageStats[stage] = GetDefaultStats(stage);
		CustomizedStages[stage] = false;
		Revision++;
		SaveParameters();
	}

	public static void SetAutoUpgradeAfterBoss(bool enabled)
	{
		EnsureLoaded();
		if (AutoUpgradeAfterBoss == enabled)
		{
			return;
		}
		AutoUpgradeAfterBoss = enabled;
		SaveParameters();
	}

	public static MoDaoQianRenStageStats GetDefaultStats(int growthStage)
	{
		int stage = ClampStage(growthStage);
		bool calamity = MoDaoQianRen.UsesCalamityBalance;
		return new MoDaoQianRenStageStats
		{
			Damage = GetDefaultDamage(stage, calamity),
			KnockBack = GetDefaultKnockBack(stage, calamity),
			CritChance = 4,
			UseTime = GetDefaultUseTime(stage, calamity),
			BladeLength = GetDefaultBladeLength(stage),
			MaxShardCharge = GetDefaultMaxShardCharge(stage, calamity),
			ShardStreamConsumeRate = GetDefaultShardStreamConsumeRate(stage, calamity),
			ShardStreamShardCount = GetDefaultShardStreamShardCount(stage, calamity),
			ShardStreamDamageMultiplier = GetDefaultShardStreamDamageMultiplier(stage),
			ShardPrismSetupUseTime = GetDefaultShardPrismSetupUseTime(stage, calamity),
			ShardPrismBaseShardCount = GetDefaultShardPrismBaseShardCount(stage, calamity),
			ShardPrismShardsPerBonusMinionSlot = GetDefaultShardPrismShardsPerBonusMinionSlot(stage, calamity),
			ShardPrismMaxShardCount = GetDefaultShardPrismMaxShardCount(stage, calamity),
			ShardPrismDamageMultiplier = GetDefaultShardPrismDamageMultiplier(stage, calamity),
			GreatswordCritChance = GetDefaultGreatswordCritChance(stage),
			GreatswordDamageMultiplier = GetDefaultGreatswordDamageMultiplier(stage, calamity, heavySlash: false),
			GreatswordHeavyDamageMultiplier = GetDefaultGreatswordDamageMultiplier(stage, calamity, heavySlash: true),
			GreatswordKnockbackMultiplier = GetDefaultGreatswordKnockbackMultiplier(stage, calamity, heavySlash: false),
			GreatswordHeavyKnockbackMultiplier = GetDefaultGreatswordKnockbackMultiplier(stage, calamity, heavySlash: true),
			ComboShardDamageMultiplier = ((!calamity) ? ((stage >= 5) ? 0.5f : ((stage >= 2) ? 0.46f : 0.38f)) : ((stage >= 5) ? 0.56f : ((stage >= 2) ? 0.5f : 0.42f))),
			GreatswordAssistShardDamageMultiplier = ((!calamity) ? ((stage >= 6) ? 0.28f : 0.22f) : ((stage >= 6) ? 0.32f : 0.25f)),
			FieldBurstDamageMultiplier = ((!calamity) ? ((stage >= 7) ? 0.9f : ((stage >= 6) ? 0.82f : 0.66f)) : ((stage >= 7) ? 1f : ((stage >= 6) ? 0.92f : 0.74f))),
			BladeDistanceDamageMultiplierMax = 1.35f,
			BladeHeavyDamageMultiplier = 2f,
			BladeSpinDamageMultiplier = 1.85f,
			BladeHitShardChargeGain = 1,
			BladeFinisherShardChargeBonus = 1,
			TipHitShardChargeBonus = 1,
			ShardPrismSearchRange = GetDefaultShardPrismSearchRange(stage),
			ShardPrismPlayerOrbitRadius = GetDefaultShardPrismPlayerOrbitRadius(stage),
			ShardPrismTargetOrbitRadius = GetDefaultShardPrismTargetOrbitRadius(stage),
			ShardPrismChargeTime = GetDefaultShardPrismChargeTime(stage),
			ShardPrismIdleSpeed = GetDefaultShardPrismIdleSpeed(stage),
			ShardPrismTargetOrbitSpeed = GetDefaultShardPrismTargetOrbitSpeed(stage),
			ShardPrismLungeSpeed = GetDefaultShardPrismLungeSpeed(stage),
			ShardPrismRepulsionSpeed = GetDefaultShardPrismRepulsionSpeed(stage),
			ShardPrismLocalHitCooldown = GetDefaultShardPrismLocalHitCooldown(stage),
			ShearsSummonDamageMultiplier = 5f,
			ShearsTyphoonDamageMultiplier = 100f,
			ShearsLocalHitCooldown = 24,
			ShearsTyphoonLocalHitCooldown = 8,
			GreatswordBurstDamageMultiplier = 100f,
			GreatswordBurstLengthMultiplier = 1.62f,
			GreatswordBurstMaximumBladeLength = 1540f,
			GreatswordBurstChargeConsumeInterval = 2,
			GreatswordBurstChargePerHit = 8,
			GreatswordBurstMaximumHitCount = 24,
			GreatswordComboDashShardCost = 5,
			GreatswordComboDashLengthMultiplier = 1.55f,
			GreatswordComboRingSlashFrames = 34,
			GreatswordComboRingSlashConsumeInterval = 6,
			GreatswordComboRingSlashDamageMultiplier = 1.8f,
			GreatswordHitShardChargeGain = 2,
			GreatswordFinisherShardChargeBonus = 2,
			GuardNormalDamageMultiplier = 0.5f,
			GuardWallDamageMultiplier = 0.1f,
			GuardSuperDamageMultiplier = 0f,
			GuardNormalShardCapacityMultiplier = 0.75f,
			GuardWallShardCapacityMultiplier = 0.45f,
			GuardSuperShardCapacityMultiplier = 0.2f,
			FieldBurstFullCharge = GetDefaultFieldBurstFullCharge(stage, calamity),
			FieldBurstMinimumCharge = GetDefaultFieldBurstMinimumCharge(stage, calamity),
			FieldBurstShardCount = GetDefaultFieldBurstShardCount(stage, calamity),
			FieldMinimumRadius = GetDefaultFieldMinimumRadius(stage),
			FieldMaximumRadius = GetDefaultFieldMaximumRadius(stage),
			FinalAssistShardDamageMultiplier = calamity ? 0.42f : 0.36f,
			FinalConvergenceShardDamageMultiplier = calamity ? 0.54f : 0.48f,
			FinalConvergenceDuration = 72,
			FinalConvergencePulseRate = 12,
			CrimsonRiftDamageMultiplier = 50f,
			CrimsonRiftBladeLengthMultiplier = 1.2f,
			CrimsonRiftShardChargeGain = 2,
			CrimsonRiftWindupFrames = 7,
			CrimsonRiftSlashFrames = 16,
			CrimsonRiftRecoveryFrames = 8,
			CrimsonRiftLocalHitCooldown = 6
		};
	}

	private static void EnsureLoaded()
	{
		if (StageStats[0] == null)
		{
			ResetToDefaults();
			LoadSavedParameters();
		}
	}

	private static void ResetToDefaults()
	{
		for (int stage = 0; stage < 8; stage++)
		{
			StageStats[stage] = GetDefaultStats(stage);
			CustomizedStages[stage] = false;
		}
		Revision++;
	}

	private static void LoadSavedParameters()
	{
		try
		{
			if (!File.Exists(SavePath))
			{
				return;
			}
			StageParameterSaveData saveData = JsonSerializer.Deserialize<StageParameterSaveData>(File.ReadAllText(SavePath), JsonOptions);
			if (saveData == null)
			{
				return;
			}
			AutoUpgradeAfterBoss = saveData.AutoUpgradeAfterBoss;
			if (saveData.Stages == null)
			{
				return;
			}
			foreach (StageParameterRecord record in saveData.Stages)
			{
				if (record?.Stats != null)
				{
					int stage = ClampStage(record.Stage);
					StageStats[stage] = Sanitize(record.Stats, GetDefaultStats(stage));
					CustomizedStages[stage] = record.Customized;
				}
			}
			Revision++;
		}
		catch (Exception ex)
		{
			ModContent.GetInstance<MoDaoQianRenMod>().Logger.Warn((object)$"Failed to load MoDaoQianRen stage parameters: {ex}");
		}
	}

	private static void SaveParameters()
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
			StageParameterSaveData saveData = new StageParameterSaveData
			{
				AutoUpgradeAfterBoss = AutoUpgradeAfterBoss
			};
			for (int stage = 0; stage < 8; stage++)
			{
				if (CustomizedStages[stage])
				{
					saveData.Stages.Add(new StageParameterRecord
					{
						Stage = stage,
						Customized = true,
						Stats = StageStats[stage].Clone()
					});
				}
			}
			File.WriteAllText(SavePath, JsonSerializer.Serialize<StageParameterSaveData>(saveData, JsonOptions));
		}
		catch (Exception ex)
		{
			ModContent.GetInstance<MoDaoQianRenMod>().Logger.Warn((object)$"Failed to save MoDaoQianRen stage parameters: {ex}");
		}
	}

	private static MoDaoQianRenStageStats Sanitize(MoDaoQianRenStageStats stats, MoDaoQianRenStageStats defaults)
	{
		if (stats == null)
		{
			stats = defaults;
		}
		if (stats.CrimsonRiftDamageMultiplier == 0f && stats.GreatswordBurstDamageMultiplier == 0f && stats.ShardPrismSearchRange == 0f)
		{
			stats = MergeRuntimeDefaults(stats, defaults);
		}
		return new MoDaoQianRenStageStats
		{
			Damage = Utils.Clamp(stats.Damage, 1, 9999),
			KnockBack = ClampFinite(stats.KnockBack, 0f, 50f, defaults.KnockBack),
			CritChance = Utils.Clamp(stats.CritChance, 0, 100),
			UseTime = Utils.Clamp(stats.UseTime, 1, 999),
			BladeLength = ClampFinite(stats.BladeLength, 40f, 3000f, defaults.BladeLength),
			MaxShardCharge = Utils.Clamp(stats.MaxShardCharge, 0, 9999),
			ShardStreamConsumeRate = Utils.Clamp(stats.ShardStreamConsumeRate, 1, 999),
			ShardStreamShardCount = Utils.Clamp(stats.ShardStreamShardCount, 0, 100),
			ShardStreamDamageMultiplier = ClampFinite((stats.ShardStreamDamageMultiplier > 0f) ? stats.ShardStreamDamageMultiplier : defaults.ShardStreamDamageMultiplier, 0.01f, 50f, defaults.ShardStreamDamageMultiplier),
			ShardPrismSetupUseTime = Utils.Clamp(stats.ShardPrismSetupUseTime, 1, 999),
			ShardPrismBaseShardCount = Utils.Clamp(stats.ShardPrismBaseShardCount, 0, 300),
			ShardPrismShardsPerBonusMinionSlot = Utils.Clamp(stats.ShardPrismShardsPerBonusMinionSlot, 0, 100),
			ShardPrismMaxShardCount = Utils.Clamp(stats.ShardPrismMaxShardCount, 0, 400),
			ShardPrismDamageMultiplier = ClampFinite(stats.ShardPrismDamageMultiplier, 0f, 50f, defaults.ShardPrismDamageMultiplier),
			GreatswordCritChance = Utils.Clamp(stats.GreatswordCritChance, 0, 100),
			GreatswordDamageMultiplier = ClampFinite(stats.GreatswordDamageMultiplier, 0f, 50f, defaults.GreatswordDamageMultiplier),
			GreatswordHeavyDamageMultiplier = ClampFinite(stats.GreatswordHeavyDamageMultiplier, 0f, 50f, defaults.GreatswordHeavyDamageMultiplier),
			GreatswordKnockbackMultiplier = ClampFinite(stats.GreatswordKnockbackMultiplier, 0f, 50f, defaults.GreatswordKnockbackMultiplier),
			GreatswordHeavyKnockbackMultiplier = ClampFinite(stats.GreatswordHeavyKnockbackMultiplier, 0f, 50f, defaults.GreatswordHeavyKnockbackMultiplier),
			ComboShardDamageMultiplier = ClampFinite(stats.ComboShardDamageMultiplier, 0f, 50f, defaults.ComboShardDamageMultiplier),
			GreatswordAssistShardDamageMultiplier = ClampFinite(stats.GreatswordAssistShardDamageMultiplier, 0f, 50f, defaults.GreatswordAssistShardDamageMultiplier),
			FieldBurstDamageMultiplier = ClampFinite(stats.FieldBurstDamageMultiplier, 0f, 50f, defaults.FieldBurstDamageMultiplier),
			BladeDistanceDamageMultiplierMax = ClampFinite(stats.BladeDistanceDamageMultiplierMax, 0f, 50f, defaults.BladeDistanceDamageMultiplierMax),
			BladeHeavyDamageMultiplier = ClampFinite(stats.BladeHeavyDamageMultiplier, 0f, 50f, defaults.BladeHeavyDamageMultiplier),
			BladeSpinDamageMultiplier = ClampFinite(stats.BladeSpinDamageMultiplier, 0f, 50f, defaults.BladeSpinDamageMultiplier),
			BladeHitShardChargeGain = Utils.Clamp(stats.BladeHitShardChargeGain, 0, 999),
			BladeFinisherShardChargeBonus = Utils.Clamp(stats.BladeFinisherShardChargeBonus, 0, 999),
			TipHitShardChargeBonus = Utils.Clamp(stats.TipHitShardChargeBonus, 0, 999),
			ShardPrismSearchRange = ClampFinite(stats.ShardPrismSearchRange, 0f, 6000f, defaults.ShardPrismSearchRange),
			ShardPrismPlayerOrbitRadius = ClampFinite(stats.ShardPrismPlayerOrbitRadius, 0f, 2000f, defaults.ShardPrismPlayerOrbitRadius),
			ShardPrismTargetOrbitRadius = ClampFinite(stats.ShardPrismTargetOrbitRadius, 0f, 2000f, defaults.ShardPrismTargetOrbitRadius),
			ShardPrismChargeTime = Utils.Clamp(stats.ShardPrismChargeTime, 1, 999),
			ShardPrismIdleSpeed = ClampFinite(stats.ShardPrismIdleSpeed, 0f, 200f, defaults.ShardPrismIdleSpeed),
			ShardPrismTargetOrbitSpeed = ClampFinite(stats.ShardPrismTargetOrbitSpeed, 0f, 200f, defaults.ShardPrismTargetOrbitSpeed),
			ShardPrismLungeSpeed = ClampFinite(stats.ShardPrismLungeSpeed, 0f, 200f, defaults.ShardPrismLungeSpeed),
			ShardPrismRepulsionSpeed = ClampFinite(stats.ShardPrismRepulsionSpeed, 0f, 200f, defaults.ShardPrismRepulsionSpeed),
			ShardPrismLocalHitCooldown = Utils.Clamp(stats.ShardPrismLocalHitCooldown, 1, 999),
			ShearsSummonDamageMultiplier = ClampFinite(stats.ShearsSummonDamageMultiplier, 0f, 500f, defaults.ShearsSummonDamageMultiplier),
			ShearsTyphoonDamageMultiplier = ClampFinite(stats.ShearsTyphoonDamageMultiplier, 0f, 500f, defaults.ShearsTyphoonDamageMultiplier),
			ShearsLocalHitCooldown = Utils.Clamp(stats.ShearsLocalHitCooldown, 1, 999),
			ShearsTyphoonLocalHitCooldown = Utils.Clamp(stats.ShearsTyphoonLocalHitCooldown, 1, 999),
			GreatswordBurstDamageMultiplier = ClampFinite(stats.GreatswordBurstDamageMultiplier, 0f, 500f, defaults.GreatswordBurstDamageMultiplier),
			GreatswordBurstLengthMultiplier = ClampFinite(stats.GreatswordBurstLengthMultiplier, 0f, 20f, defaults.GreatswordBurstLengthMultiplier),
			GreatswordBurstMaximumBladeLength = ClampFinite(stats.GreatswordBurstMaximumBladeLength, 120f, 4000f, defaults.GreatswordBurstMaximumBladeLength),
			GreatswordBurstChargeConsumeInterval = Utils.Clamp(stats.GreatswordBurstChargeConsumeInterval, 1, 999),
			GreatswordBurstChargePerHit = Utils.Clamp(stats.GreatswordBurstChargePerHit, 1, 999),
			GreatswordBurstMaximumHitCount = Utils.Clamp(stats.GreatswordBurstMaximumHitCount, 1, 999),
			GreatswordComboDashShardCost = Utils.Clamp(stats.GreatswordComboDashShardCost, 0, 999),
			GreatswordComboDashLengthMultiplier = ClampFinite(stats.GreatswordComboDashLengthMultiplier, 0f, 20f, defaults.GreatswordComboDashLengthMultiplier),
			GreatswordComboRingSlashFrames = Utils.Clamp(stats.GreatswordComboRingSlashFrames, 1, 999),
			GreatswordComboRingSlashConsumeInterval = Utils.Clamp(stats.GreatswordComboRingSlashConsumeInterval, 1, 999),
			GreatswordComboRingSlashDamageMultiplier = ClampFinite(stats.GreatswordComboRingSlashDamageMultiplier, 0f, 50f, defaults.GreatswordComboRingSlashDamageMultiplier),
			GreatswordHitShardChargeGain = Utils.Clamp(stats.GreatswordHitShardChargeGain, 0, 999),
			GreatswordFinisherShardChargeBonus = Utils.Clamp(stats.GreatswordFinisherShardChargeBonus, 0, 999),
			GuardNormalDamageMultiplier = ClampFinite(stats.GuardNormalDamageMultiplier, 0f, 10f, defaults.GuardNormalDamageMultiplier),
			GuardWallDamageMultiplier = ClampFinite(stats.GuardWallDamageMultiplier, 0f, 10f, defaults.GuardWallDamageMultiplier),
			GuardSuperDamageMultiplier = ClampFinite(stats.GuardSuperDamageMultiplier, 0f, 10f, defaults.GuardSuperDamageMultiplier),
			GuardNormalShardCapacityMultiplier = ClampFinite(stats.GuardNormalShardCapacityMultiplier, 0f, 10f, defaults.GuardNormalShardCapacityMultiplier),
			GuardWallShardCapacityMultiplier = ClampFinite(stats.GuardWallShardCapacityMultiplier, 0f, 10f, defaults.GuardWallShardCapacityMultiplier),
			GuardSuperShardCapacityMultiplier = ClampFinite(stats.GuardSuperShardCapacityMultiplier, 0f, 10f, defaults.GuardSuperShardCapacityMultiplier),
			FieldBurstFullCharge = Utils.Clamp(stats.FieldBurstFullCharge, 1, 999),
			FieldBurstMinimumCharge = Utils.Clamp(stats.FieldBurstMinimumCharge, 0, 999),
			FieldBurstShardCount = Utils.Clamp(stats.FieldBurstShardCount, 0, 999),
			FieldMinimumRadius = ClampFinite(stats.FieldMinimumRadius, 0f, 4000f, defaults.FieldMinimumRadius),
			FieldMaximumRadius = ClampFinite(stats.FieldMaximumRadius, 0f, 4000f, defaults.FieldMaximumRadius),
			FinalAssistShardDamageMultiplier = ClampFinite(stats.FinalAssistShardDamageMultiplier, 0f, 50f, defaults.FinalAssistShardDamageMultiplier),
			FinalConvergenceShardDamageMultiplier = ClampFinite(stats.FinalConvergenceShardDamageMultiplier, 0f, 50f, defaults.FinalConvergenceShardDamageMultiplier),
			FinalConvergenceDuration = Utils.Clamp(stats.FinalConvergenceDuration, 1, 999),
			FinalConvergencePulseRate = Utils.Clamp(stats.FinalConvergencePulseRate, 1, 999),
			CrimsonRiftDamageMultiplier = ClampFinite(stats.CrimsonRiftDamageMultiplier, 0f, 500f, defaults.CrimsonRiftDamageMultiplier),
			CrimsonRiftBladeLengthMultiplier = ClampFinite(stats.CrimsonRiftBladeLengthMultiplier, 0f, 20f, defaults.CrimsonRiftBladeLengthMultiplier),
			CrimsonRiftShardChargeGain = Utils.Clamp(stats.CrimsonRiftShardChargeGain, 0, 999),
			CrimsonRiftWindupFrames = Utils.Clamp(stats.CrimsonRiftWindupFrames, 1, 999),
			CrimsonRiftSlashFrames = Utils.Clamp(stats.CrimsonRiftSlashFrames, 1, 999),
			CrimsonRiftRecoveryFrames = Utils.Clamp(stats.CrimsonRiftRecoveryFrames, 1, 999),
			CrimsonRiftLocalHitCooldown = Utils.Clamp(stats.CrimsonRiftLocalHitCooldown, 1, 999)
		};
	}

	private static MoDaoQianRenStageStats MergeRuntimeDefaults(MoDaoQianRenStageStats stats, MoDaoQianRenStageStats defaults)
	{
		MoDaoQianRenStageStats merged = defaults.Clone();
		merged.Damage = stats.Damage;
		merged.KnockBack = stats.KnockBack;
		merged.CritChance = stats.CritChance;
		merged.UseTime = stats.UseTime;
		merged.BladeLength = stats.BladeLength;
		merged.MaxShardCharge = stats.MaxShardCharge;
		merged.ShardStreamConsumeRate = stats.ShardStreamConsumeRate;
		merged.ShardStreamShardCount = stats.ShardStreamShardCount;
		merged.ShardStreamDamageMultiplier = stats.ShardStreamDamageMultiplier;
		merged.ShardPrismSetupUseTime = stats.ShardPrismSetupUseTime;
		merged.ShardPrismBaseShardCount = stats.ShardPrismBaseShardCount;
		merged.ShardPrismShardsPerBonusMinionSlot = stats.ShardPrismShardsPerBonusMinionSlot;
		merged.ShardPrismMaxShardCount = stats.ShardPrismMaxShardCount;
		merged.ShardPrismDamageMultiplier = stats.ShardPrismDamageMultiplier;
		merged.GreatswordCritChance = stats.GreatswordCritChance;
		merged.GreatswordDamageMultiplier = stats.GreatswordDamageMultiplier;
		merged.GreatswordHeavyDamageMultiplier = stats.GreatswordHeavyDamageMultiplier;
		merged.GreatswordKnockbackMultiplier = stats.GreatswordKnockbackMultiplier;
		merged.GreatswordHeavyKnockbackMultiplier = stats.GreatswordHeavyKnockbackMultiplier;
		merged.ComboShardDamageMultiplier = stats.ComboShardDamageMultiplier;
		merged.GreatswordAssistShardDamageMultiplier = stats.GreatswordAssistShardDamageMultiplier;
		merged.FieldBurstDamageMultiplier = stats.FieldBurstDamageMultiplier;
		return merged;
	}

	private static float ClampFinite(float value, float min, float max, float fallback)
	{
		if (float.IsNaN(value) || float.IsInfinity(value))
		{
			value = fallback;
		}
		return MathHelper.Clamp(value, min, max);
	}

	private static int ClampStage(int growthStage)
	{
		return Utils.Clamp(growthStage, 0, 7);
	}

	private static int GetDefaultDamage(int stage, bool calamity)
	{
		if (calamity)
		{
			return stage switch
			{
				7 => 235, 
				6 => 150, 
				5 => 112, 
				4 => 82, 
				3 => 54, 
				2 => 34, 
				1 => 21, 
				_ => 12, 
			};
		}
		return stage switch
		{
			7 => 148, 
			6 => 100, 
			5 => 76, 
			4 => 58, 
			3 => 40, 
			2 => 24, 
			1 => 15, 
			_ => 10, 
		};
	}

	private static float GetDefaultKnockBack(int stage, bool calamity)
	{
		if (calamity)
		{
			return stage switch
			{
				7 => 7.25f, 
				6 => 6.65f, 
				5 => 6.1f, 
				4 => 5.35f, 
				3 => 4.75f, 
				2 => 4.05f, 
				1 => 3.25f, 
				_ => 2.2f, 
			};
		}
		return stage switch
		{
			7 => 6.75f, 
			6 => 6.15f, 
			5 => 5.65f, 
			4 => 4.9f, 
			3 => 4.35f, 
			2 => 3.75f, 
			1 => 3f, 
			_ => 2f, 
		};
	}

	private static int GetDefaultUseTime(int stage, bool calamity)
	{
		if (calamity)
		{
			return stage switch
			{
				7 => 14, 
				6 => 16, 
				5 => 17, 
				4 => 18, 
				3 => 19, 
				2 => 20, 
				1 => 22, 
				_ => 21, 
			};
		}
		return stage switch
		{
			7 => 15, 
			6 => 17, 
			5 => 18, 
			4 => 19, 
			3 => 20, 
			2 => 21, 
			1 => 23, 
			_ => 22, 
		};
	}

	private static float GetDefaultBladeLength(int stage)
	{
		return stage switch
		{
			7 => 880f, 
			6 => 760f, 
			5 => 660f, 
			4 => 560f, 
			3 => 460f, 
			2 => 320f, 
			1 => 220f, 
			_ => 160f, 
		};
	}

	private static int GetDefaultMaxShardCharge(int stage, bool calamity)
	{
		if (calamity)
		{
			if (stage < 4)
			{
				if (stage < 3)
				{
					if (stage < 2)
					{
						if (stage < 1)
						{
							return 12;
						}
						return 20;
					}
					return 30;
				}
				return 44;
			}
			if (stage < 7)
			{
				if (stage < 6)
				{
					if (stage < 5)
					{
						return 70;
					}
					return 100;
				}
				return 128;
			}
			return 180;
		}
		if (stage < 4)
		{
			if (stage < 3)
			{
				if (stage < 2)
				{
					if (stage < 1)
					{
						return 10;
					}
					return 16;
				}
				return 24;
			}
			return 35;
		}
		if (stage < 7)
		{
			if (stage < 6)
			{
				if (stage < 5)
				{
					return 55;
				}
				return 80;
			}
			return 100;
		}
		return 140;
	}

	private static int GetDefaultShardPrismSetupUseTime(int stage, bool calamity)
	{
		if (calamity)
		{
			if (stage >= 5)
			{
				if (stage < 7)
				{
					if (stage >= 6)
					{
						return 21;
					}
					return 22;
				}
				return 20;
			}
			if (stage >= 4)
			{
				return 24;
			}
			return 30;
		}
		if (stage >= 5)
		{
			if (stage < 7)
			{
				if (stage >= 6)
				{
					return 23;
				}
				return 24;
			}
			return 22;
		}
		if (stage >= 4)
		{
			return 26;
		}
		return 30;
	}

	private static int GetDefaultShardPrismBaseShardCount(int stage, bool calamity)
	{
		if (calamity)
		{
			if (stage >= 5)
			{
				if (stage < 7)
				{
					if (stage >= 6)
					{
						return 34;
					}
					return 27;
				}
				return 42;
			}
			if (stage >= 4)
			{
				return 20;
			}
			return 0;
		}
		if (stage >= 5)
		{
			if (stage < 7)
			{
				if (stage >= 6)
				{
					return 30;
				}
				return 24;
			}
			return 36;
		}
		if (stage >= 4)
		{
			return 18;
		}
		return 0;
	}

	private static int GetDefaultShardPrismShardsPerBonusMinionSlot(int stage, bool calamity)
	{
		if (calamity)
		{
			if (stage >= 5)
			{
				if (stage < 7)
				{
					if (stage >= 6)
					{
						return 4;
					}
					return 3;
				}
				return 5;
			}
			if (stage >= 4)
			{
				return 2;
			}
			return 0;
		}
		if (stage >= 5)
		{
			if (stage < 7)
			{
				if (stage >= 6)
				{
					return 4;
				}
				return 3;
			}
			return 4;
		}
		if (stage >= 4)
		{
			return 2;
		}
		return 0;
	}

	private static int GetDefaultShardPrismMaxShardCount(int stage, bool calamity)
	{
		if (calamity)
		{
			if (stage >= 5)
			{
				if (stage < 7)
				{
					if (stage >= 6)
					{
						return 72;
					}
					return 54;
				}
				return 84;
			}
			if (stage >= 4)
			{
				return 38;
			}
			return 0;
		}
		if (stage >= 5)
		{
			if (stage < 7)
			{
				if (stage >= 6)
				{
					return 64;
				}
				return 48;
			}
			return 72;
		}
		if (stage >= 4)
		{
			return 34;
		}
		return 0;
	}

	private static float GetDefaultShardPrismDamageMultiplier(int stage, bool calamity)
	{
		if (calamity)
		{
			if (stage >= 5)
			{
				if (stage < 7)
				{
					if (stage >= 6)
					{
						return 0.41f;
					}
					return 0.37f;
				}
				return 0.46f;
			}
			if (stage >= 4)
			{
				return 0.34f;
			}
			return 0f;
		}
		if (stage >= 5)
		{
			if (stage < 7)
			{
				if (stage >= 6)
				{
					return 0.38f;
				}
				return 0.35f;
			}
			return 0.42f;
		}
		if (stage >= 4)
		{
			return 0.32f;
		}
		return 0f;
	}

	private static int GetDefaultGreatswordCritChance(int stage)
	{
		if (stage >= 6)
		{
			if (stage >= 7)
			{
				return 100;
			}
			return 62;
		}
		if (stage >= 5)
		{
			return 36;
		}
		return 4;
	}

	private static float GetDefaultGreatswordDamageMultiplier(int stage, bool calamity, bool heavySlash)
	{
		if (calamity)
		{
			if (stage >= 7)
			{
				if (!heavySlash)
				{
					return 3.25f;
				}
				return 5f;
			}
			if (stage >= 6)
			{
				if (!heavySlash)
				{
					return 2.8f;
				}
				return 4.15f;
			}
			if (stage >= 5)
			{
				if (!heavySlash)
				{
					return 2.35f;
				}
				return 3.45f;
			}
			if (!heavySlash)
			{
				return 2f;
			}
			return 2.6f;
		}
		if (stage >= 7)
		{
			if (!heavySlash)
			{
				return 3f;
			}
			return 4.6f;
		}
		if (stage >= 6)
		{
			if (!heavySlash)
			{
				return 2.55f;
			}
			return 3.8f;
		}
		if (stage >= 5)
		{
			if (!heavySlash)
			{
				return 2.15f;
			}
			return 3.15f;
		}
		if (!heavySlash)
		{
			return 1.85f;
		}
		return 2.4f;
	}

	private static float GetDefaultGreatswordKnockbackMultiplier(int stage, bool calamity, bool heavySlash)
	{
		if (calamity)
		{
			if (stage >= 6)
			{
				if (stage >= 7)
				{
					return heavySlash ? 2f : 1.72f;
				}
				return heavySlash ? 1.84f : 1.62f;
			}
			if (stage >= 5)
			{
				return heavySlash ? 1.68f : 1.52f;
			}
			return heavySlash ? 1.5f : 1.35f;
		}
		if (stage >= 6)
		{
			if (stage >= 7)
			{
				return heavySlash ? 1.9f : 1.65f;
			}
			return heavySlash ? 1.75f : 1.55f;
		}
		if (stage >= 5)
		{
			return heavySlash ? 1.6f : 1.45f;
		}
		return heavySlash ? 1.45f : 1.3f;
	}

	private static int GetDefaultShardStreamConsumeRate(int stage, bool calamity)
	{
		if (calamity)
		{
			if (stage >= 5)
			{
				if (stage < 7)
				{
					if (stage >= 6)
					{
						return 3;
					}
					return 4;
				}
				return 3;
			}
			if (stage >= 2)
			{
				if (stage >= 4)
				{
					return 4;
				}
				return 5;
			}
			if (stage >= 1)
			{
				return 6;
			}
			return 7;
		}
		if (stage >= 2)
		{
			if (stage >= 5)
			{
				if (stage >= 7)
				{
					return 3;
				}
				return 4;
			}
			if (stage >= 3)
			{
				return 5;
			}
			return 6;
		}
		if (stage >= 1)
		{
			return 7;
		}
		return 8;
	}

	private static int GetDefaultShardStreamShardCount(int stage, bool calamity)
	{
		if (calamity)
		{
			if (stage >= 5)
			{
				if (stage < 7)
				{
					if (stage >= 6)
					{
						return 5;
					}
					return 5;
				}
				return 6;
			}
			if (stage >= 2)
			{
				if (stage >= 4)
				{
					return 4;
				}
				return 3;
			}
			if (stage >= 1)
			{
				return 2;
			}
			return 1;
		}
		if (stage >= 2)
		{
			if (stage >= 5)
			{
				if (stage >= 7)
				{
					return 5;
				}
				return 4;
			}
			if (stage >= 4)
			{
				return 4;
			}
			return 3;
		}
		if (stage >= 1)
		{
			return 2;
		}
		return 1;
	}

	private static float GetDefaultShardStreamDamageMultiplier(int stage)
	{
		if (stage >= 4)
		{
			if (stage >= 6)
			{
				if (stage >= 7)
				{
					return 0.7f;
				}
				return 0.68f;
			}
			if (stage >= 5)
			{
				return 0.66f;
			}
			return 0.68f;
		}
		if (stage >= 2)
		{
			if (stage >= 3)
			{
				return 0.72f;
			}
			return 0.78f;
		}
		if (stage >= 1)
		{
			return 0.9f;
		}
		return 1.05f;
	}

	private static float GetDefaultShardPrismSearchRange(int stage)
	{
		if (stage >= 7)
		{
			return 2000f;
		}
		if (stage >= 6)
		{
			return 1720f;
		}
		if (stage >= 5)
		{
			return 1480f;
		}
		return stage >= 4 ? 1220f : 0f;
	}

	private static float GetDefaultShardPrismPlayerOrbitRadius(int stage)
	{
		if (stage >= 7)
		{
			return 170f;
		}
		if (stage >= 6)
		{
			return 162f;
		}
		if (stage >= 5)
		{
			return 150f;
		}
		return stage >= 4 ? 136f : 120f;
	}

	private static float GetDefaultShardPrismTargetOrbitRadius(int stage)
	{
		if (stage >= 7)
		{
			return 180f;
		}
		if (stage >= 6)
		{
			return 170f;
		}
		if (stage >= 5)
		{
			return 160f;
		}
		return stage >= 4 ? 148f : 136f;
	}

	private static int GetDefaultShardPrismChargeTime(int stage)
	{
		if (stage >= 7)
		{
			return 45;
		}
		if (stage >= 6)
		{
			return 48;
		}
		if (stage >= 5)
		{
			return 52;
		}
		return stage >= 4 ? 56 : 60;
	}

	private static float GetDefaultShardPrismIdleSpeed(int stage)
	{
		if (stage >= 7)
		{
			return 10f;
		}
		if (stage >= 6)
		{
			return 9.5f;
		}
		if (stage >= 5)
		{
			return 8.9f;
		}
		return stage >= 4 ? 8.2f : 7.5f;
	}

	private static float GetDefaultShardPrismTargetOrbitSpeed(int stage)
	{
		if (stage >= 7)
		{
			return 30f;
		}
		if (stage >= 6)
		{
			return 27f;
		}
		if (stage >= 5)
		{
			return 24f;
		}
		return stage >= 4 ? 21f : 18f;
	}

	private static float GetDefaultShardPrismLungeSpeed(int stage)
	{
		if (stage >= 7)
		{
			return 20f;
		}
		if (stage >= 6)
		{
			return 18.5f;
		}
		if (stage >= 5)
		{
			return 17f;
		}
		return stage >= 4 ? 15.5f : 14f;
	}

	private static float GetDefaultShardPrismRepulsionSpeed(int stage)
	{
		if (stage >= 7)
		{
			return 11f;
		}
		if (stage >= 6)
		{
			return 10f;
		}
		if (stage >= 5)
		{
			return 9f;
		}
		return stage >= 4 ? 8f : 7f;
	}

	private static int GetDefaultShardPrismLocalHitCooldown(int stage)
	{
		if (stage >= 7)
		{
			return 22;
		}
		if (stage >= 6)
		{
			return 24;
		}
		if (stage >= 5)
		{
			return 26;
		}
		return stage >= 4 ? 28 : 30;
	}

	private static int GetDefaultFieldBurstFullCharge(int stage, bool calamity)
	{
		if (calamity)
		{
			if (stage >= 7)
			{
				return 26;
			}
			return stage >= 6 ? 21 : 15;
		}
		if (stage >= 7)
		{
			return 30;
		}
		return stage >= 6 ? 24 : 16;
	}

	private static int GetDefaultFieldBurstMinimumCharge(int stage, bool calamity)
	{
		if (calamity)
		{
			if (stage >= 7)
			{
				return 8;
			}
			return stage >= 6 ? 7 : 5;
		}
		if (stage >= 7)
		{
			return 10;
		}
		return stage >= 6 ? 8 : 6;
	}

	private static int GetDefaultFieldBurstShardCount(int stage, bool calamity)
	{
		if (calamity)
		{
			if (stage >= 7)
			{
				return 52;
			}
			return stage >= 6 ? 40 : 27;
		}
		if (stage >= 7)
		{
			return 46;
		}
		return stage >= 6 ? 36 : 24;
	}

	private static float GetDefaultFieldMinimumRadius(int stage)
	{
		if (stage >= 7)
		{
			return 118f;
		}
		return stage >= 6 ? 96f : 82f;
	}

	private static float GetDefaultFieldMaximumRadius(int stage)
	{
		if (stage >= 7)
		{
			return 340f;
		}
		return stage >= 6 ? 270f : 220f;
	}
}
