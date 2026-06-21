using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using 魔刀千刃.Content.Items.Weapons;
using 魔刀千刃.Content.Players;

namespace 魔刀千刃.Content.Systems;

public class MoDaoQianRenUISystem : ModSystem
{
	private sealed class StageParameterField
	{
		private readonly Func<MoDaoQianRenStageStats, float> getter;

		private readonly Action<MoDaoQianRenStageStats, float> setter;

		public string LabelKey { get; }

		public string DescriptionKey { get; }

		public bool IsFloat { get; }

		public StageParameterField(string labelKey, bool isFloat, Func<MoDaoQianRenStageStats, float> getter, Action<MoDaoQianRenStageStats, float> setter)
		{
			LabelKey = labelKey;
			DescriptionKey = labelKey + "Desc";
			IsFloat = isFloat;
			this.getter = getter;
			this.setter = setter;
		}

		public string Format(MoDaoQianRenStageStats stats)
		{
			float value = getter(stats);
			if (!IsFloat)
			{
				return ((int)MathF.Round(value)).ToString(CultureInfo.InvariantCulture);
			}
			return value.ToString("0.###", CultureInfo.InvariantCulture);
		}

		public bool TryApply(MoDaoQianRenStageStats stats, string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return false;
			}
			bool parsed;
			float value;
			if (IsFloat)
			{
				parsed = float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
			}
			else
			{
				parsed = int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue);
				value = intValue;
			}
			if (!parsed || float.IsNaN(value) || float.IsInfinity(value))
			{
				return false;
			}
			setter(stats, value);
			return true;
		}
	}

	private static readonly int SlotContext = 4;

	private static readonly StageParameterField[] ParameterFields = new StageParameterField[]
	{
		new StageParameterField("FieldDamage", isFloat: false, (MoDaoQianRenStageStats stats) => stats.Damage, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.Damage = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldKnockBack", isFloat: true, (MoDaoQianRenStageStats stats) => stats.KnockBack, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.KnockBack = value;
		}),
		new StageParameterField("FieldCritChance", isFloat: false, (MoDaoQianRenStageStats stats) => stats.CritChance, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.CritChance = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldUseTime", isFloat: false, (MoDaoQianRenStageStats stats) => stats.UseTime, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.UseTime = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldBladeLength", isFloat: true, (MoDaoQianRenStageStats stats) => stats.BladeLength, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.BladeLength = value;
		}),
		new StageParameterField("FieldMaxShardCharge", isFloat: false, (MoDaoQianRenStageStats stats) => stats.MaxShardCharge, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.MaxShardCharge = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldShardStreamConsumeRate", isFloat: false, (MoDaoQianRenStageStats stats) => stats.ShardStreamConsumeRate, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardStreamConsumeRate = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldShardStreamShardCount", isFloat: false, (MoDaoQianRenStageStats stats) => stats.ShardStreamShardCount, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardStreamShardCount = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldShardStreamDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.ShardStreamDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardStreamDamageMultiplier = value;
		}),
		new StageParameterField("FieldShardPrismSetupUseTime", isFloat: false, (MoDaoQianRenStageStats stats) => stats.ShardPrismSetupUseTime, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismSetupUseTime = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldShardPrismBaseShardCount", isFloat: false, (MoDaoQianRenStageStats stats) => stats.ShardPrismBaseShardCount, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismBaseShardCount = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldShardPrismShardsPerBonusMinionSlot", isFloat: false, (MoDaoQianRenStageStats stats) => stats.ShardPrismShardsPerBonusMinionSlot, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismShardsPerBonusMinionSlot = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldShardPrismMaxShardCount", isFloat: false, (MoDaoQianRenStageStats stats) => stats.ShardPrismMaxShardCount, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismMaxShardCount = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldShardPrismDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.ShardPrismDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismDamageMultiplier = value;
		}),
		new StageParameterField("FieldGreatswordCritChance", isFloat: false, (MoDaoQianRenStageStats stats) => stats.GreatswordCritChance, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordCritChance = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldGreatswordDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GreatswordDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordDamageMultiplier = value;
		}),
		new StageParameterField("FieldGreatswordHeavyDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GreatswordHeavyDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordHeavyDamageMultiplier = value;
		}),
		new StageParameterField("FieldGreatswordKnockbackMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GreatswordKnockbackMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordKnockbackMultiplier = value;
		}),
		new StageParameterField("FieldGreatswordHeavyKnockbackMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GreatswordHeavyKnockbackMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordHeavyKnockbackMultiplier = value;
		}),
		new StageParameterField("FieldComboShardDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.ComboShardDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ComboShardDamageMultiplier = value;
		}),
		new StageParameterField("FieldGreatswordAssistShardDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GreatswordAssistShardDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordAssistShardDamageMultiplier = value;
		}),
		new StageParameterField("FieldFieldBurstDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.FieldBurstDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.FieldBurstDamageMultiplier = value;
		}),
		new StageParameterField("FieldBladeDistanceDamageMultiplierMax", isFloat: true, (MoDaoQianRenStageStats stats) => stats.BladeDistanceDamageMultiplierMax, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.BladeDistanceDamageMultiplierMax = value;
		}),
		new StageParameterField("FieldBladeHeavyDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.BladeHeavyDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.BladeHeavyDamageMultiplier = value;
		}),
		new StageParameterField("FieldBladeSpinDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.BladeSpinDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.BladeSpinDamageMultiplier = value;
		}),
		new StageParameterField("FieldBladeHitShardChargeGain", isFloat: false, (MoDaoQianRenStageStats stats) => stats.BladeHitShardChargeGain, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.BladeHitShardChargeGain = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldBladeFinisherShardChargeBonus", isFloat: false, (MoDaoQianRenStageStats stats) => stats.BladeFinisherShardChargeBonus, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.BladeFinisherShardChargeBonus = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldTipHitShardChargeBonus", isFloat: false, (MoDaoQianRenStageStats stats) => stats.TipHitShardChargeBonus, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.TipHitShardChargeBonus = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldShardPrismSearchRange", isFloat: true, (MoDaoQianRenStageStats stats) => stats.ShardPrismSearchRange, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismSearchRange = value;
		}),
		new StageParameterField("FieldShardPrismPlayerOrbitRadius", isFloat: true, (MoDaoQianRenStageStats stats) => stats.ShardPrismPlayerOrbitRadius, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismPlayerOrbitRadius = value;
		}),
		new StageParameterField("FieldShardPrismTargetOrbitRadius", isFloat: true, (MoDaoQianRenStageStats stats) => stats.ShardPrismTargetOrbitRadius, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismTargetOrbitRadius = value;
		}),
		new StageParameterField("FieldShardPrismChargeTime", isFloat: false, (MoDaoQianRenStageStats stats) => stats.ShardPrismChargeTime, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismChargeTime = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldShardPrismIdleSpeed", isFloat: true, (MoDaoQianRenStageStats stats) => stats.ShardPrismIdleSpeed, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismIdleSpeed = value;
		}),
		new StageParameterField("FieldShardPrismTargetOrbitSpeed", isFloat: true, (MoDaoQianRenStageStats stats) => stats.ShardPrismTargetOrbitSpeed, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismTargetOrbitSpeed = value;
		}),
		new StageParameterField("FieldShardPrismLungeSpeed", isFloat: true, (MoDaoQianRenStageStats stats) => stats.ShardPrismLungeSpeed, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismLungeSpeed = value;
		}),
		new StageParameterField("FieldShardPrismRepulsionSpeed", isFloat: true, (MoDaoQianRenStageStats stats) => stats.ShardPrismRepulsionSpeed, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismRepulsionSpeed = value;
		}),
		new StageParameterField("FieldShardPrismLocalHitCooldown", isFloat: false, (MoDaoQianRenStageStats stats) => stats.ShardPrismLocalHitCooldown, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShardPrismLocalHitCooldown = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldShearsSummonDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.ShearsSummonDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShearsSummonDamageMultiplier = value;
		}),
		new StageParameterField("FieldShearsTyphoonDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.ShearsTyphoonDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShearsTyphoonDamageMultiplier = value;
		}),
		new StageParameterField("FieldShearsLocalHitCooldown", isFloat: false, (MoDaoQianRenStageStats stats) => stats.ShearsLocalHitCooldown, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShearsLocalHitCooldown = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldShearsTyphoonLocalHitCooldown", isFloat: false, (MoDaoQianRenStageStats stats) => stats.ShearsTyphoonLocalHitCooldown, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.ShearsTyphoonLocalHitCooldown = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldGreatswordBurstDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GreatswordBurstDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordBurstDamageMultiplier = value;
		}),
		new StageParameterField("FieldGreatswordBurstLengthMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GreatswordBurstLengthMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordBurstLengthMultiplier = value;
		}),
		new StageParameterField("FieldGreatswordBurstMaximumBladeLength", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GreatswordBurstMaximumBladeLength, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordBurstMaximumBladeLength = value;
		}),
		new StageParameterField("FieldGreatswordBurstChargeConsumeInterval", isFloat: false, (MoDaoQianRenStageStats stats) => stats.GreatswordBurstChargeConsumeInterval, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordBurstChargeConsumeInterval = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldGreatswordBurstChargePerHit", isFloat: false, (MoDaoQianRenStageStats stats) => stats.GreatswordBurstChargePerHit, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordBurstChargePerHit = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldGreatswordBurstMaximumHitCount", isFloat: false, (MoDaoQianRenStageStats stats) => stats.GreatswordBurstMaximumHitCount, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordBurstMaximumHitCount = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldGreatswordComboDashShardCost", isFloat: false, (MoDaoQianRenStageStats stats) => stats.GreatswordComboDashShardCost, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordComboDashShardCost = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldGreatswordComboDashLengthMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GreatswordComboDashLengthMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordComboDashLengthMultiplier = value;
		}),
		new StageParameterField("FieldGreatswordComboRingSlashFrames", isFloat: false, (MoDaoQianRenStageStats stats) => stats.GreatswordComboRingSlashFrames, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordComboRingSlashFrames = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldGreatswordComboRingSlashConsumeInterval", isFloat: false, (MoDaoQianRenStageStats stats) => stats.GreatswordComboRingSlashConsumeInterval, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordComboRingSlashConsumeInterval = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldGreatswordComboRingSlashDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GreatswordComboRingSlashDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordComboRingSlashDamageMultiplier = value;
		}),
		new StageParameterField("FieldGreatswordHitShardChargeGain", isFloat: false, (MoDaoQianRenStageStats stats) => stats.GreatswordHitShardChargeGain, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordHitShardChargeGain = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldGreatswordFinisherShardChargeBonus", isFloat: false, (MoDaoQianRenStageStats stats) => stats.GreatswordFinisherShardChargeBonus, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GreatswordFinisherShardChargeBonus = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldGuardNormalDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GuardNormalDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GuardNormalDamageMultiplier = value;
		}),
		new StageParameterField("FieldGuardWallDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GuardWallDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GuardWallDamageMultiplier = value;
		}),
		new StageParameterField("FieldGuardSuperDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GuardSuperDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GuardSuperDamageMultiplier = value;
		}),
		new StageParameterField("FieldGuardNormalShardCapacityMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GuardNormalShardCapacityMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GuardNormalShardCapacityMultiplier = value;
		}),
		new StageParameterField("FieldGuardWallShardCapacityMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GuardWallShardCapacityMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GuardWallShardCapacityMultiplier = value;
		}),
		new StageParameterField("FieldGuardSuperShardCapacityMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.GuardSuperShardCapacityMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.GuardSuperShardCapacityMultiplier = value;
		}),
		new StageParameterField("FieldFieldBurstFullCharge", isFloat: false, (MoDaoQianRenStageStats stats) => stats.FieldBurstFullCharge, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.FieldBurstFullCharge = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldFieldBurstMinimumCharge", isFloat: false, (MoDaoQianRenStageStats stats) => stats.FieldBurstMinimumCharge, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.FieldBurstMinimumCharge = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldFieldBurstShardCount", isFloat: false, (MoDaoQianRenStageStats stats) => stats.FieldBurstShardCount, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.FieldBurstShardCount = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldFieldMinimumRadius", isFloat: true, (MoDaoQianRenStageStats stats) => stats.FieldMinimumRadius, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.FieldMinimumRadius = value;
		}),
		new StageParameterField("FieldFieldMaximumRadius", isFloat: true, (MoDaoQianRenStageStats stats) => stats.FieldMaximumRadius, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.FieldMaximumRadius = value;
		}),
		new StageParameterField("FieldFinalAssistShardDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.FinalAssistShardDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.FinalAssistShardDamageMultiplier = value;
		}),
		new StageParameterField("FieldFinalConvergenceShardDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.FinalConvergenceShardDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.FinalConvergenceShardDamageMultiplier = value;
		}),
		new StageParameterField("FieldFinalConvergenceDuration", isFloat: false, (MoDaoQianRenStageStats stats) => stats.FinalConvergenceDuration, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.FinalConvergenceDuration = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldFinalConvergencePulseRate", isFloat: false, (MoDaoQianRenStageStats stats) => stats.FinalConvergencePulseRate, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.FinalConvergencePulseRate = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldCrimsonRiftDamageMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.CrimsonRiftDamageMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.CrimsonRiftDamageMultiplier = value;
		}),
		new StageParameterField("FieldCrimsonRiftBladeLengthMultiplier", isFloat: true, (MoDaoQianRenStageStats stats) => stats.CrimsonRiftBladeLengthMultiplier, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.CrimsonRiftBladeLengthMultiplier = value;
		}),
		new StageParameterField("FieldCrimsonRiftShardChargeGain", isFloat: false, (MoDaoQianRenStageStats stats) => stats.CrimsonRiftShardChargeGain, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.CrimsonRiftShardChargeGain = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldCrimsonRiftWindupFrames", isFloat: false, (MoDaoQianRenStageStats stats) => stats.CrimsonRiftWindupFrames, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.CrimsonRiftWindupFrames = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldCrimsonRiftSlashFrames", isFloat: false, (MoDaoQianRenStageStats stats) => stats.CrimsonRiftSlashFrames, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.CrimsonRiftSlashFrames = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldCrimsonRiftRecoveryFrames", isFloat: false, (MoDaoQianRenStageStats stats) => stats.CrimsonRiftRecoveryFrames, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.CrimsonRiftRecoveryFrames = (int)MathF.Round(value);
		}),
		new StageParameterField("FieldCrimsonRiftLocalHitCooldown", isFloat: false, (MoDaoQianRenStageStats stats) => stats.CrimsonRiftLocalHitCooldown, delegate(MoDaoQianRenStageStats stats, float value)
		{
			stats.CrimsonRiftLocalHitCooldown = (int)MathF.Round(value);
		})
	};

	private const int FieldRowsPerColumn = 7;

	private const int FieldsPerPage = 14;

	private const int DefaultPanelWidth = 860;

	private const int DefaultPanelHeight = 560;

	private Item editorItem = new Item();

	private readonly string[] parameterTexts = new string[ParameterFields.Length];

	private readonly Rectangle[] fieldBounds = new Rectangle[ParameterFields.Length];

	private bool editorVisible;

	private int loadedStage = -1;

	private int selectedField = -1;

	private Rectangle panelBounds;

	private Rectangle slotBounds;

	private Rectangle titleBarBounds;

	private Rectangle previousPageButtonBounds;

	private Rectangle nextPageButtonBounds;

	private Rectangle saveButtonBounds;

	private Rectangle resetButtonBounds;

	private Rectangle autoUpgradeButtonBounds;

	private Rectangle closeButtonBounds;

	private string statusText = string.Empty;

	private Color statusColor = Color.White;

	private int statusTimer;

	private int loadedSlotItemType = -1;

	private bool inventoryOpenBeforeEditor;

	private bool panelPositionInitialized;

	private bool draggingEditor;

	private bool wasEditorMouseLeftDown;

	private bool clearSelectedFieldOnNextInput;

	private int editorPage;

	private Vector2 panelPosition;

	private Vector2 dragOffset;

	public static ModKeybind OpenStageEditorKeybind { get; private set; }

	public static ModKeybind SwitchToBladeModeKeybind { get; private set; }

	public static ModKeybind SwitchToShardPrismModeKeybind { get; private set; }

	public static ModKeybind SwitchToGreatswordModeKeybind { get; private set; }

	public static ModKeybind SwitchToGuardModeKeybind { get; private set; }

	public static ModKeybind SwitchGreatswordDevilsModeKeybind { get; private set; }

	public static bool IsStageEditorOpen { get; private set; }

	public override void Load()
	{
		if (!Main.dedServ)
		{
			OpenStageEditorKeybind = KeybindLoader.RegisterKeybind(base.Mod, "OpenStageEditor", Keys.F9);
			SwitchToBladeModeKeybind = KeybindLoader.RegisterKeybind(base.Mod, "SwitchToBladeMode", Keys.NumPad1);
			SwitchToShardPrismModeKeybind = KeybindLoader.RegisterKeybind(base.Mod, "SwitchToShardPrismMode", Keys.NumPad2);
			SwitchToGreatswordModeKeybind = KeybindLoader.RegisterKeybind(base.Mod, "SwitchToGreatswordMode", Keys.NumPad3);
			SwitchToGuardModeKeybind = KeybindLoader.RegisterKeybind(base.Mod, "SwitchToGuardMode", Keys.NumPad4);
			SwitchGreatswordDevilsModeKeybind = KeybindLoader.RegisterKeybind(base.Mod, "SwitchGreatswordDevilsMode", Keys.NumPad5);
		}
		editorItem.TurnToAir();
	}

	public override void Unload()
	{
		OpenStageEditorKeybind = null;
		SwitchToBladeModeKeybind = null;
		SwitchToShardPrismModeKeybind = null;
		SwitchToGreatswordModeKeybind = null;
		SwitchToGuardModeKeybind = null;
		SwitchGreatswordDevilsModeKeybind = null;
		IsStageEditorOpen = false;
	}

	public void ToggleStageEditor()
	{
		if (Main.dedServ)
		{
			return;
		}
		if (editorVisible)
		{
			CloseStageEditor();
			return;
		}
		editorVisible = true;
		IsStageEditorOpen = true;
		inventoryOpenBeforeEditor = Main.playerInventory;
		Main.playerInventory = true;
		selectedField = -1;
		editorPage = 0;
		wasEditorMouseLeftDown = IsEditorLeftMouseDown();
		InitializePanelPositionIfNeeded();
		if (editorItem.IsAir && TryFindBestOwnedBlade(out var blade))
		{
			LoadEditorItemFromSource(blade);
		}
		SyncFieldsFromSlot();
		Main.clrInput();
		SoundEngine.PlaySound(in SoundID.MenuOpen);
	}

	public override void UpdateUI(GameTime gameTime)
	{
		if (!editorVisible)
		{
			return;
		}
		if (Main.gameMenu)
		{
			CloseStageEditor();
			return;
		}
		Player player = Main.LocalPlayer;
		if (player == null || !player.active)
		{
			return;
		}
		UpdatePanelLayout();
		bool leftMouseDown = IsEditorLeftMouseDown();
		bool num = leftMouseDown && !wasEditorMouseLeftDown;
		UpdateEditorDrag(leftMouseDown);
		bool mouseInPanel = panelBounds.Contains(Main.mouseX, Main.mouseY);
		if (mouseInPanel)
		{
			player.mouseInterface = true;
		}
		bool mouseInSlot = slotBounds.Contains(Main.mouseX, Main.mouseY);
		if (mouseInSlot)
		{
			player.mouseInterface = true;
		}
		SyncFieldsFromSlot();
		if (selectedField >= 0)
		{
			player.mouseInterface = true;
			string newText = parameterTexts[selectedField];
			if (ApplyKeyboardInput(ref newText, ParameterFields[selectedField].IsFloat))
			{
				parameterTexts[selectedField] = newText;
			}
			if (Main.keyState.IsKeyDown(Keys.Enter) && Main.oldKeyState.IsKeyUp(Keys.Enter))
			{
				TrySaveCurrentStage();
			}
			if (Main.keyState.IsKeyDown(Keys.Escape) && Main.oldKeyState.IsKeyUp(Keys.Escape))
			{
				selectedField = -1;
				clearSelectedFieldOnNextInput = false;
			}
		}
		if (num && mouseInPanel)
		{
			HandleEditorClick(mouseInSlot);
			Main.mouseLeftRelease = false;
		}
		wasEditorMouseLeftDown = leftMouseDown;
		if (statusTimer > 0)
		{
			statusTimer--;
		}
	}

	public override void PostUpdateInput()
	{
		if (!Main.dedServ && !Main.gameMenu && !MoDaoQianRenWarmupSystem.ShouldBlockBladeUseForLights)
		{
			bool num = OpenStageEditorKeybind?.JustPressed ?? false;
			bool fallbackPressed = Main.keyState.IsKeyDown(Keys.F9) && Main.oldKeyState.IsKeyUp(Keys.F9);
			if (num || fallbackPressed)
			{
				ToggleStageEditor();
			}
		}
	}

	public override void PostDrawInterface(SpriteBatch spriteBatch)
	{
		if (MoDaoQianRenWarmupSystem.ShouldSkipCustomDrawingForLights)
		{
			return;
		}
		Player player = Main.LocalPlayer;
		if (player != null && player.active)
		{
			if (MoDaoQianRen.TryGetGrowthStage(player.HeldItem, out var _))
			{
				MoDaoQianRenPlayer bladePlayer = player.GetModPlayer<MoDaoQianRenPlayer>();
				DrawShardChargeBar(spriteBatch, bladePlayer.ShardCharge, bladePlayer.GetCurrentMaxShardCharge());
			}
			if (editorVisible)
			{
				DrawStageEditor(spriteBatch);
			}
		}
	}

	private void HandleEditorClick(bool mouseInSlot)
	{
		if (mouseInSlot)
		{
			TryInteractWithEditorSlot();
			selectedField = -1;
			clearSelectedFieldOnNextInput = false;
			Main.clrInput();
			return;
		}
		if (titleBarBounds.Contains(Main.mouseX, Main.mouseY))
		{
			StartEditorDrag();
			return;
		}
		if (previousPageButtonBounds.Contains(Main.mouseX, Main.mouseY))
		{
			SetEditorPage(editorPage - 1);
			return;
		}
		if (nextPageButtonBounds.Contains(Main.mouseX, Main.mouseY))
		{
			SetEditorPage(editorPage + 1);
			return;
		}
		if (closeButtonBounds.Contains(Main.mouseX, Main.mouseY))
		{
			CloseStageEditor();
			return;
		}
		if (saveButtonBounds.Contains(Main.mouseX, Main.mouseY))
		{
			TrySaveCurrentStage();
			return;
		}
		if (resetButtonBounds.Contains(Main.mouseX, Main.mouseY))
		{
			TryResetCurrentStage();
			return;
		}
		if (autoUpgradeButtonBounds.Contains(Main.mouseX, Main.mouseY))
		{
			ToggleAutoUpgradeAfterBoss();
			return;
		}
		selectedField = -1;
		clearSelectedFieldOnNextInput = false;
		int num = editorPage * FieldsPerPage;
		int lastField = Math.Min(num + FieldsPerPage, ParameterFields.Length);
		for (int i = num; i < lastField; i++)
		{
			if (fieldBounds[i].Contains(Main.mouseX, Main.mouseY))
			{
				selectedField = i;
				clearSelectedFieldOnNextInput = true;
				Main.clrInput();
				break;
			}
		}
	}

	private void TryInteractWithEditorSlot()
	{
		Item mouseItem = Main.mouseItem;
		Item blade;
		if (mouseItem != null && !mouseItem.IsAir)
		{
			if (!MoDaoQianRen.TryGetGrowthStage(Main.mouseItem, out var _))
			{
				SetStatus(Text("StatusInvalidItem"), Color.OrangeRed);
				SoundStyle style = SoundID.MenuClose with
				{
					Volume = 0.55f
				};
				SoundEngine.PlaySound(in style);
			}
			else
			{
				LoadEditorItemFromSource(Main.mouseItem);
			}
		}
		else if (TryFindBestOwnedBlade(out blade))
		{
			LoadEditorItemFromSource(blade);
		}
		else
		{
			editorItem.TurnToAir();
			SyncFieldsFromSlot(force: true);
			SetStatus(Text("StatusNeedBlade"), Color.OrangeRed);
			SoundStyle style = SoundID.MenuClose with
			{
				Volume = 0.55f
			};
			SoundEngine.PlaySound(in style);
		}
	}

	private void LoadEditorItemFromSource(Item source)
	{
		editorItem = source.Clone();
		editorItem.stack = 1;
		SyncFieldsFromSlot(force: true);
		SoundEngine.PlaySound(in SoundID.MenuTick);
	}

	private static bool TryFindBestOwnedBlade(out Item blade)
	{
		blade = null;
		Player player = Main.LocalPlayer;
		if (player == null)
		{
			return false;
		}
		TryUseBetterBlade(player.HeldItem, ref blade);
		Item[] inventory = player.inventory;
		for (int i = 0; i < inventory.Length; i++)
		{
			TryUseBetterBlade(inventory[i], ref blade);
		}
		return blade != null;
	}

	private static void TryUseBetterBlade(Item candidate, ref Item blade)
	{
		if (MoDaoQianRen.TryGetGrowthStage(candidate, out var candidateStage) && (blade == null || !MoDaoQianRen.TryGetGrowthStage(blade, out var currentStage) || candidateStage > currentStage))
		{
			blade = candidate;
		}
	}

	private void TrySaveCurrentStage()
	{
		if (!TryGetEditorStage(out var stage))
		{
			SetStatus(Text("StatusNeedBlade"), Color.OrangeRed);
			return;
		}
		if (!TryBuildEditedStats(out var stats, out var invalidField))
		{
			SetStatus(Language.GetTextValue(GetTextKey("StatusInvalidField"), invalidField), Color.OrangeRed);
			return;
		}
		MoDaoQianRenStageParameterSystem.SaveStageStats(stage, stats);
		RefreshBladeItems(Main.LocalPlayer);
		SyncFieldsFromSlot(force: true);
		SetStatus(Language.GetTextValue(GetTextKey("StatusSaved"), GetStageName(stage)), new Color(120, 255, 170));
		SoundEngine.PlaySound(in SoundID.MenuTick);
	}

	private void TryResetCurrentStage()
	{
		if (!TryGetEditorStage(out var stage))
		{
			SetStatus(Text("StatusNeedBlade"), Color.OrangeRed);
			return;
		}
		MoDaoQianRenStageParameterSystem.ResetStageStats(stage);
		RefreshBladeItems(Main.LocalPlayer);
		SyncFieldsFromSlot(force: true);
		SetStatus(Language.GetTextValue(GetTextKey("StatusReset"), GetStageName(stage)), new Color(255, 210, 120));
		SoundEngine.PlaySound(in SoundID.MenuTick);
	}

	private void ToggleAutoUpgradeAfterBoss()
	{
		bool enabled = !MoDaoQianRenStageParameterSystem.AutoUpgradeAfterBoss;
		MoDaoQianRenStageParameterSystem.SetAutoUpgradeAfterBoss(enabled);
		SetStatus(Text(enabled ? "StatusAutoUpgradeEnabled" : "StatusAutoUpgradeDisabled"), enabled ? new Color(120, 255, 170) : new Color(255, 210, 120));
		SoundEngine.PlaySound(in SoundID.MenuTick);
	}

	private bool TryBuildEditedStats(out MoDaoQianRenStageStats stats, out string invalidField)
	{
		stats = (TryGetEditorStage(out var stage) ? MoDaoQianRenStageParameterSystem.GetEditableStageStats(stage) : null);
		invalidField = string.Empty;
		if (stats == null)
		{
			return false;
		}
		for (int i = 0; i < ParameterFields.Length; i++)
		{
			if (!ParameterFields[i].TryApply(stats, parameterTexts[i]))
			{
				invalidField = Text(ParameterFields[i].LabelKey);
				return false;
			}
		}
		return true;
	}

	private void SyncFieldsFromSlot(bool force = false)
	{
		int editorStage;
		int stage = (TryGetEditorStage(out editorStage) ? editorStage : (-1));
		int slotItemType = editorItem.type;
		if (!force && stage == loadedStage && slotItemType == loadedSlotItemType)
		{
			return;
		}
		loadedStage = stage;
		loadedSlotItemType = slotItemType;
		selectedField = -1;
		clearSelectedFieldOnNextInput = false;
		if (stage < 0)
		{
			Array.Fill<string>(parameterTexts, string.Empty);
			if (!editorItem.IsAir)
			{
				SetStatus(Text("StatusInvalidItem"), Color.OrangeRed);
				return;
			}
			statusText = string.Empty;
			statusTimer = 0;
		}
		else
		{
			MoDaoQianRenStageStats stats = MoDaoQianRenStageParameterSystem.GetEditableStageStats(stage);
			for (int i = 0; i < ParameterFields.Length; i++)
			{
				parameterTexts[i] = ParameterFields[i].Format(stats);
			}
			SetStatus(Language.GetTextValue(GetTextKey(MoDaoQianRenStageParameterSystem.HasCustomStats(stage) ? "StatusLoadedCustom" : "StatusLoadedDefault"), GetStageName(stage)), new Color(210, 185, 255), 180);
		}
	}

	private bool TryGetEditorStage(out int stage)
	{
		return MoDaoQianRen.TryGetGrowthStage(editorItem, out stage);
	}

	private void CloseStageEditor()
	{
		editorItem.TurnToAir();
		editorVisible = false;
		IsStageEditorOpen = false;
		Main.playerInventory = inventoryOpenBeforeEditor;
		Main.blockInput = false;
		draggingEditor = false;
		wasEditorMouseLeftDown = false;
		loadedStage = -1;
		loadedSlotItemType = -1;
		selectedField = -1;
		clearSelectedFieldOnNextInput = false;
		statusText = string.Empty;
		SoundEngine.PlaySound(in SoundID.MenuClose);
	}

	private static void RefreshBladeItems(Player player)
	{
		if (player != null)
		{
			RefreshBladeItem(player.HeldItem);
			Item[] inventory = player.inventory;
			for (int i = 0; i < inventory.Length; i++)
			{
				RefreshBladeItem(inventory[i]);
			}
			inventory = player.bank.item;
			for (int i = 0; i < inventory.Length; i++)
			{
				RefreshBladeItem(inventory[i]);
			}
			inventory = player.bank2.item;
			for (int i = 0; i < inventory.Length; i++)
			{
				RefreshBladeItem(inventory[i]);
			}
			inventory = player.bank3.item;
			for (int i = 0; i < inventory.Length; i++)
			{
				RefreshBladeItem(inventory[i]);
			}
			inventory = player.bank4.item;
			for (int i = 0; i < inventory.Length; i++)
			{
				RefreshBladeItem(inventory[i]);
			}
			RefreshBladeItem(Main.mouseItem);
		}
	}

	private static void RefreshBladeItem(Item item)
	{
		if (item?.ModItem is MoDaoQianRen blade)
		{
			blade.RefreshGrowthStatsFromCustomParameters();
		}
	}

	private void InitializePanelPositionIfNeeded()
	{
		if (!panelPositionInitialized)
		{
			int width = GetPanelWidth();
			int height = GetPanelHeight();
			panelPosition = new Vector2(MathHelper.Clamp((float)(Main.screenWidth - width) * 0.5f, 12f, Math.Max(12f, (float)(Main.screenWidth - width) - 12f)), MathHelper.Clamp((float)(Main.screenHeight - height) * 0.46f, 48f, Math.Max(48f, (float)(Main.screenHeight - height) - 12f)));
			panelPositionInitialized = true;
		}
	}

	private static bool IsEditorLeftMouseDown()
	{
		if (!Main.mouseLeft)
		{
			return Mouse.GetState().LeftButton == ButtonState.Pressed;
		}
		return true;
	}

	private void UpdateEditorDrag(bool leftMouseDown)
	{
		if (draggingEditor)
		{
			if (leftMouseDown)
			{
				panelPosition = new Vector2(Main.mouseX, Main.mouseY) - dragOffset;
				ClampPanelPosition(GetPanelWidth(), GetPanelHeight());
			}
			else
			{
				draggingEditor = false;
			}
		}
	}

	private void StartEditorDrag()
	{
		draggingEditor = true;
		dragOffset = new Vector2(Main.mouseX - panelBounds.X, Main.mouseY - panelBounds.Y);
	}

	private void SetEditorPage(int page)
	{
		int pageCount = GetEditorPageCount();
		int newPage = Utils.Clamp(page, 0, pageCount - 1);
		if (editorPage != newPage)
		{
			editorPage = newPage;
			selectedField = -1;
			clearSelectedFieldOnNextInput = false;
			SoundEngine.PlaySound(in SoundID.MenuTick);
		}
	}

	private static int GetEditorPageCount()
	{
		return (ParameterFields.Length + FieldsPerPage - 1) / FieldsPerPage;
	}

	private static int GetPanelWidth()
	{
		return Math.Min(DefaultPanelWidth, Math.Max(560, Main.screenWidth - 80));
	}

	private static int GetPanelHeight()
	{
		return Math.Min(DefaultPanelHeight, Math.Max(440, Main.screenHeight - 80));
	}

	private void ClampPanelPosition(int width, int height)
	{
		panelPosition.X = MathHelper.Clamp(panelPosition.X, 8f, Math.Max(8f, (float)(Main.screenWidth - width) - 8f));
		panelPosition.Y = MathHelper.Clamp(panelPosition.Y, 8f, Math.Max(8f, (float)(Main.screenHeight - height) - 8f));
	}

	private void UpdatePanelLayout()
	{
		int width = GetPanelWidth();
		int height = GetPanelHeight();
		InitializePanelPositionIfNeeded();
		ClampPanelPosition(width, height);
		int x = (int)panelPosition.X;
		int y = (int)panelPosition.Y;
		panelBounds = new Rectangle(x, y, width, height);
		titleBarBounds = new Rectangle(x, y, width, 46);
		slotBounds = new Rectangle(x + 24, y + 76, 48, 48);
		previousPageButtonBounds = new Rectangle(panelBounds.Right - 112, y + 124, 28, 24);
		nextPageButtonBounds = new Rectangle(panelBounds.Right - 36, y + 124, 28, 24);
		autoUpgradeButtonBounds = new Rectangle(panelBounds.Right - 196, y + 76, 172, 28);
		saveButtonBounds = new Rectangle(panelBounds.Right - 276, panelBounds.Bottom - 42, 80, 28);
		resetButtonBounds = new Rectangle(panelBounds.Right - 188, panelBounds.Bottom - 42, 80, 28);
		closeButtonBounds = new Rectangle(panelBounds.Right - 100, panelBounds.Bottom - 42, 76, 28);
		int fieldsLeft = x + 24;
		int fieldsTop = y + 260;
		int columnWidth = (width - 48) / 2;
		int rowHeight = 30;
		Array.Fill<Rectangle>(fieldBounds, Rectangle.Empty);
		int firstField = editorPage * FieldsPerPage;
		int lastField = Math.Min(firstField + FieldsPerPage, ParameterFields.Length);
		for (int i = firstField; i < lastField; i++)
		{
			int num = i - firstField;
			int column = num / FieldRowsPerColumn;
			int row = num % FieldRowsPerColumn;
			fieldBounds[i] = new Rectangle(fieldsLeft + column * columnWidth, fieldsTop + row * rowHeight, columnWidth - 12, 24);
		}
	}

	private void DrawStageEditor(SpriteBatch spriteBatch)
	{
		UpdatePanelLayout();
		DrawRect(spriteBatch, panelBounds, new Color(18, 14, 24, 238));
		DrawRect(spriteBatch, titleBarBounds, new Color(34, 22, 46, 236));
		DrawBorder(spriteBatch, panelBounds, new Color(188, 78, 255, 220), 2);
		Utils.DrawBorderString(pos: new Vector2((float)panelBounds.X + 18f, (float)panelBounds.Y + 12f), sb: spriteBatch, text: Text("Title"), color: new Color(245, 222, 255), scale: 0.84f);
		Utils.DrawBorderString(spriteBatch, Text("Subtitle"), new Vector2((float)panelBounds.X + 24f, (float)panelBounds.Y + 52f), new Color(188, 158, 220), 0.64f);
		Utils.DrawBorderString(spriteBatch, Text("Hint"), new Vector2((float)panelBounds.X + 24f, (float)panelBounds.Y + 72f), new Color(176, 148, 205), 0.56f);
		DrawItemSlot(spriteBatch);
		DrawSlotInfo(spriteBatch);
		DrawModeGuide(spriteBatch);
		DrawButton(spriteBatch, previousPageButtonBounds, "<", new Color(66, 48, 86), previousPageButtonBounds.Contains(Main.mouseX, Main.mouseY));
		DrawButton(spriteBatch, nextPageButtonBounds, ">", new Color(66, 48, 86), nextPageButtonBounds.Contains(Main.mouseX, Main.mouseY));
		string pageText = $"{editorPage + 1}/{GetEditorPageCount()}";
		Utils.DrawBorderString(spriteBatch, pageText, new Vector2((float)previousPageButtonBounds.Right + 10f, (float)previousPageButtonBounds.Y + 4f), new Color(220, 195, 240), 0.64f);
		if (loadedStage >= 0)
		{
			DrawParameterFields(spriteBatch);
		}
		else
		{
			Rectangle emptyArea = new Rectangle(panelBounds.X + 24, panelBounds.Y + 260, panelBounds.Width - 48, 68);
			DrawRect(spriteBatch, emptyArea, new Color(36, 25, 48, 190));
			DrawBorder(spriteBatch, emptyArea, new Color(116, 62, 150, 180), 1);
			Utils.DrawBorderString(spriteBatch, Text("EmptySlot"), emptyArea.Location.ToVector2() + new Vector2(14f, 22f), new Color(220, 195, 240), 0.78f);
		}
		DrawAutoUpgradeButton(spriteBatch);
		DrawButton(spriteBatch, saveButtonBounds, Text("Save"), new Color(70, 132, 82), saveButtonBounds.Contains(Main.mouseX, Main.mouseY));
		DrawButton(spriteBatch, resetButtonBounds, Text("Reset"), new Color(132, 96, 58), resetButtonBounds.Contains(Main.mouseX, Main.mouseY));
		DrawButton(spriteBatch, closeButtonBounds, Text("Close"), new Color(86, 68, 104), closeButtonBounds.Contains(Main.mouseX, Main.mouseY));
		if (statusTimer > 0 && !string.IsNullOrWhiteSpace(statusText))
		{
			Utils.DrawBorderString(spriteBatch, statusText, new Vector2((float)panelBounds.X + 24f, (float)panelBounds.Bottom - 36f), statusColor, 0.66f);
		}
	}

	private void DrawAutoUpgradeButton(SpriteBatch spriteBatch)
	{
		bool enabled = MoDaoQianRenStageParameterSystem.AutoUpgradeAfterBoss;
		bool hovered = autoUpgradeButtonBounds.Contains(Main.mouseX, Main.mouseY);
		DrawButton(spriteBatch, autoUpgradeButtonBounds, Text(enabled ? "AutoUpgradeOn" : "AutoUpgradeOff"), enabled ? new Color(64, 132, 88) : new Color(86, 68, 104), hovered);
		if (hovered)
		{
			Rectangle descriptionBounds = new Rectangle(panelBounds.X + 24, panelBounds.Bottom - 76, panelBounds.Width - 48, 30);
			DrawRect(spriteBatch, descriptionBounds, new Color(28, 20, 38, 224));
			DrawBorder(spriteBatch, descriptionBounds, new Color(122, 76, 152, 190), 1);
			Utils.DrawBorderString(spriteBatch, Text("AutoUpgradeDesc"), descriptionBounds.Location.ToVector2() + new Vector2(8f, 7f), new Color(218, 196, 235), 0.55f);
		}
	}

	private void DrawItemSlot(SpriteBatch spriteBatch)
	{
		DrawRect(spriteBatch, new Rectangle(slotBounds.X - 4, slotBounds.Y - 4, slotBounds.Width + 8, slotBounds.Height + 8), new Color(42, 30, 56, 230));
		DrawBorder(spriteBatch, new Rectangle(slotBounds.X - 4, slotBounds.Y - 4, slotBounds.Width + 8, slotBounds.Height + 8), new Color(146, 72, 190, 210), 1);
		float inventoryScale = Main.inventoryScale;
		Main.inventoryScale = 0.85f;
		ItemSlot.Draw(spriteBatch, ref editorItem, SlotContext, slotBounds.Location.ToVector2());
		Main.inventoryScale = inventoryScale;
	}

	private void DrawSlotInfo(SpriteBatch spriteBatch)
	{
		Vector2 textPosition = new Vector2((float)slotBounds.Right + 14f, (float)slotBounds.Y + 2f);
		string stageText = ((loadedStage >= 0) ? GetStageName(loadedStage) : Text(editorItem.IsAir ? "SlotHint" : "InvalidItem"));
		Color stageColor = ((loadedStage >= 0) ? new Color(235, 210, 255) : new Color(210, 150, 150));
		Utils.DrawBorderString(spriteBatch, stageText, textPosition, stageColor, 0.72f);
		Utils.DrawBorderString(spriteBatch, Text("SlotSubhint"), textPosition + new Vector2(0f, 26f), new Color(166, 136, 190), 0.58f);
	}

	private void DrawModeGuide(SpriteBatch spriteBatch)
	{
		Rectangle guideBounds = new Rectangle(panelBounds.X + 24, panelBounds.Y + 136, panelBounds.Width - 48, 108);
		DrawRect(spriteBatch, guideBounds, new Color(30, 22, 42, 204));
		DrawBorder(spriteBatch, guideBounds, new Color(116, 62, 150, 180), 1);
		Utils.DrawBorderString(spriteBatch, Text("ModeGuideTitle"), guideBounds.Location.ToVector2() + new Vector2(10f, 8f), new Color(235, 210, 255), 0.62f);
		int stage = Math.Max(0, loadedStage);
		string[] keys = new string[5]
		{
			"ModeGuide1",
			stage >= 4 ? "ModeGuide2" : "ModeGuide2Locked",
			stage >= 5 ? "ModeGuide3" : "ModeGuide3Locked",
			stage >= 4 ? "ModeGuide4" : "ModeGuide4Locked",
			stage >= 5 ? "ModeGuide5" : "ModeGuide5Locked"
		};
		for (int i = 0; i < keys.Length; i++)
		{
			string line = Text(keys[i]);
			float scale = 0.5f;
			float maxWidth = guideBounds.Width - 20f;
			float width = FontAssets.MouseText.Value.MeasureString(line).X * scale;
			if (width > maxWidth)
			{
				scale = MathHelper.Clamp(maxWidth / MathF.Max(1f, width) * scale, 0.38f, scale);
			}
			Utils.DrawBorderString(spriteBatch, line, guideBounds.Location.ToVector2() + new Vector2(10f, 30f + i * 14f), new Color(205, 178, 226), scale);
		}
	}

	private void DrawParameterFields(SpriteBatch spriteBatch)
	{
		int firstField = editorPage * FieldsPerPage;
		int lastField = Math.Min(firstField + FieldsPerPage, ParameterFields.Length);
		for (int i = firstField; i < lastField; i++)
		{
			Rectangle bounds = fieldBounds[i];
			bool selected = selectedField == i;
			bool hovered = bounds.Contains(Main.mouseX, Main.mouseY);
			DrawRect(spriteBatch, bounds, selected ? new Color(72, 48, 94, 235) : (hovered ? new Color(50, 36, 66, 230) : new Color(34, 25, 45, 218)));
			DrawBorder(spriteBatch, bounds, selected ? new Color(232, 164, 255, 230) : new Color(104, 68, 132, 180), 1);
			string label = Text(ParameterFields[i].LabelKey);
			Utils.DrawBorderString(spriteBatch, label, bounds.Location.ToVector2() + new Vector2(7f, 4f), new Color(214, 188, 232), 0.61f);
			string value = (selected ? (parameterTexts[i] + ((Main.GameUpdateCount / 25 % 2 == 0) ? "|" : string.Empty)) : parameterTexts[i]);
			Vector2 valueSize = FontAssets.MouseText.Value.MeasureString(value) * 0.66f;
			Utils.DrawBorderString(spriteBatch, value, new Vector2((float)bounds.Right - valueSize.X - 8f, (float)bounds.Y + 3f), Color.White, 0.66f);
		}
		DrawFieldDescription(spriteBatch, firstField, lastField);
	}

	private void DrawFieldDescription(SpriteBatch spriteBatch, int firstField, int lastField)
	{
		if (autoUpgradeButtonBounds.Contains(Main.mouseX, Main.mouseY))
		{
			return;
		}
		int fieldIndex = -1;
		for (int i = firstField; i < lastField; i++)
		{
			if (fieldBounds[i].Contains(Main.mouseX, Main.mouseY))
			{
				fieldIndex = i;
				break;
			}
		}
		if (fieldIndex < 0 && selectedField >= firstField && selectedField < lastField)
		{
			fieldIndex = selectedField;
		}
		if (fieldIndex >= 0)
		{
			Rectangle descriptionBounds = new Rectangle(panelBounds.X + 24, panelBounds.Bottom - 76, panelBounds.Width - 48, 30);
			string description = Text(ParameterFields[fieldIndex].DescriptionKey);
			float descriptionScale = 0.55f;
			float maxDescriptionWidth = (float)descriptionBounds.Width - 16f;
			float descriptionWidth = FontAssets.MouseText.Value.MeasureString(description).X * descriptionScale;
			if (descriptionWidth > maxDescriptionWidth)
			{
				descriptionScale = MathHelper.Clamp(maxDescriptionWidth / MathF.Max(1f, descriptionWidth) * descriptionScale, 0.35f, descriptionScale);
			}
			DrawRect(spriteBatch, descriptionBounds, new Color(28, 20, 38, 224));
			DrawBorder(spriteBatch, descriptionBounds, new Color(122, 76, 152, 190), 1);
			Utils.DrawBorderString(spriteBatch, description, descriptionBounds.Location.ToVector2() + new Vector2(8f, 7f), new Color(218, 196, 235), descriptionScale);
		}
	}

	private static void DrawButton(SpriteBatch spriteBatch, Rectangle bounds, string text, Color baseColor, bool hovered)
	{
		DrawRect(spriteBatch, bounds, hovered ? (baseColor * 1.15f) : baseColor);
		DrawBorder(spriteBatch, bounds, hovered ? (Color.White * 0.82f) : (Color.Black * 0.38f), 1);
		Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text) * 0.72f;
		Utils.DrawBorderString(spriteBatch, text, bounds.Center.ToVector2() - textSize * 0.5f, Color.White, 0.72f);
	}

	private static void DrawShardChargeBar(SpriteBatch spriteBatch, int charge, int maxCharge)
	{
		Texture2D pixel = TextureAssets.MagicPixel.Value;
		int width = 178;
		int height = 12;
		Vector2 position = new Vector2(28f, (float)Main.screenHeight - 92f);
		Rectangle border = new Rectangle((int)position.X, (int)position.Y, width, height);
		Rectangle background = new Rectangle(border.X + 2, border.Y + 2, border.Width - 4, border.Height - 4);
		float fillRatio = ((maxCharge > 0) ? MathHelper.Clamp((float)charge / (float)maxCharge, 0f, 1f) : 0f);
		Rectangle fill = new Rectangle(background.X, background.Y, (int)((float)background.Width * fillRatio), background.Height);
		spriteBatch.Draw(pixel, border, new Color(20, 16, 26, 210));
		spriteBatch.Draw(pixel, background, new Color(54, 40, 64, 210));
		if (fill.Width > 0)
		{
			spriteBatch.Draw(pixel, fill, new Color(172, 74, 255, 235));
			Rectangle shine = new Rectangle(fill.X, fill.Y, fill.Width, 3);
			spriteBatch.Draw(pixel, shine, new Color(245, 190, 255, 170));
		}
		string label = Language.GetTextValue("Mods.魔刀千刃.UI.ShardCharge", charge, maxCharge);
		Utils.DrawBorderString(spriteBatch, label, position + new Vector2(0f, -24f), new Color(230, 210, 255), 0.85f);
	}

	private static void DrawRect(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
	{
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, color);
	}

	private static void DrawBorder(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int thickness)
	{
		DrawRect(spriteBatch, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
		DrawRect(spriteBatch, new Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
		DrawRect(spriteBatch, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
		DrawRect(spriteBatch, new Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
	}

	private bool ApplyKeyboardInput(ref string text, bool allowFloat)
	{
		bool changed = false;
		bool clearFirst = clearSelectedFieldOnNextInput;
		for (int digit = 0; digit <= 9; digit++)
		{
			if (IsKeyPressed((Keys)(48 + digit)) || IsKeyPressed((Keys)(96 + digit)))
			{
				if (clearFirst)
				{
					text = string.Empty;
					clearFirst = false;
				}
				if (text.Length < 16)
				{
					text += digit.ToString(CultureInfo.InvariantCulture);
					changed = true;
				}
			}
		}
		if (allowFloat && (IsKeyPressed(Keys.OemPeriod) || IsKeyPressed(Keys.Decimal)) && !text.Contains('.') && text.Length < 16)
		{
			if (clearFirst)
			{
				text = string.Empty;
				clearFirst = false;
			}
			text = (string.IsNullOrEmpty(text) ? "0." : (text + "."));
			changed = true;
		}
		if (IsKeyPressed(Keys.Back))
		{
			if (clearFirst)
			{
				text = string.Empty;
				clearFirst = false;
				changed = true;
			}
			else if (text.Length > 0)
			{
				string text2 = text;
				text = text2.Substring(0, text2.Length - 1);
				changed = true;
			}
		}
		if (IsKeyPressed(Keys.Delete))
		{
			text = string.Empty;
			clearFirst = false;
			changed = true;
		}
		if (changed)
		{
			clearSelectedFieldOnNextInput = false;
		}
		return changed;
	}

	private static bool IsKeyPressed(Keys key)
	{
		if (Main.keyState.IsKeyDown(key))
		{
			return Main.oldKeyState.IsKeyUp(key);
		}
		return false;
	}

	private static string FilterNumericText(string text, bool allowFloat)
	{
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		System.Span<char> buffer = stackalloc char[Math.Min(text.Length, 16)];
		int length = 0;
		bool hasDecimal = false;
		foreach (char character in text)
		{
			if (char.IsDigit(character))
			{
				if (length < buffer.Length)
				{
					buffer[length++] = character;
				}
			}
			else if (!(!allowFloat || character != '.' || hasDecimal) && length < buffer.Length)
			{
				hasDecimal = true;
				buffer[length++] = character;
			}
		}
		return new string(buffer.Slice(0, length));
	}

	private void SetStatus(string text, Color color, int ticks = 240)
	{
		statusText = text;
		statusColor = color;
		statusTimer = ticks;
	}

	private string GetStageName(int stage)
	{
		return Language.GetTextValue($"Mods.{base.Mod.Name}.Items.MoDaoQianRen.Stage{stage}");
	}

	private string Text(string suffix)
	{
		return Language.GetTextValue(GetTextKey(suffix));
	}

	private string GetTextKey(string suffix)
	{
		return "Mods." + base.Mod.Name + ".UI.StageEditor." + suffix;
	}
}
