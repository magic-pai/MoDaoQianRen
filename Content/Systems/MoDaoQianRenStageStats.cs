namespace 魔刀千刃.Content.Systems;

public sealed class MoDaoQianRenStageStats
{
	public int Damage { get; set; }

	public float KnockBack { get; set; }

	public int CritChance { get; set; }

	public int UseTime { get; set; }

	public float BladeLength { get; set; }

	public int MaxShardCharge { get; set; }

	public int ShardStreamConsumeRate { get; set; }

	public int ShardStreamShardCount { get; set; }

	public float ShardStreamDamageMultiplier { get; set; }

	public int ShardPrismSetupUseTime { get; set; }

	public int ShardPrismBaseShardCount { get; set; }

	public int ShardPrismShardsPerBonusMinionSlot { get; set; }

	public int ShardPrismMaxShardCount { get; set; }

	public float ShardPrismDamageMultiplier { get; set; }

	public int GreatswordCritChance { get; set; }

	public float GreatswordDamageMultiplier { get; set; }

	public float GreatswordHeavyDamageMultiplier { get; set; }

	public float GreatswordKnockbackMultiplier { get; set; }

	public float GreatswordHeavyKnockbackMultiplier { get; set; }

	public float ComboShardDamageMultiplier { get; set; }

	public float GreatswordAssistShardDamageMultiplier { get; set; }

	public float FieldBurstDamageMultiplier { get; set; }

	public float BladeDistanceDamageMultiplierMax { get; set; }

	public float BladeHeavyDamageMultiplier { get; set; }

	public float BladeSpinDamageMultiplier { get; set; }

	public int BladeHitShardChargeGain { get; set; }

	public int BladeFinisherShardChargeBonus { get; set; }

	public int TipHitShardChargeBonus { get; set; }

	public float ShardPrismSearchRange { get; set; }

	public float ShardPrismPlayerOrbitRadius { get; set; }

	public float ShardPrismTargetOrbitRadius { get; set; }

	public int ShardPrismChargeTime { get; set; }

	public float ShardPrismIdleSpeed { get; set; }

	public float ShardPrismTargetOrbitSpeed { get; set; }

	public float ShardPrismLungeSpeed { get; set; }

	public float ShardPrismRepulsionSpeed { get; set; }

	public int ShardPrismLocalHitCooldown { get; set; }

	public float ShearsSummonDamageMultiplier { get; set; }

	public float ShearsTyphoonDamageMultiplier { get; set; }

	public int ShearsLocalHitCooldown { get; set; }

	public int ShearsTyphoonLocalHitCooldown { get; set; }

	public float GreatswordBurstDamageMultiplier { get; set; }

	public float GreatswordBurstLengthMultiplier { get; set; }

	public float GreatswordBurstMaximumBladeLength { get; set; }

	public int GreatswordBurstChargeConsumeInterval { get; set; }

	public int GreatswordBurstChargePerHit { get; set; }

	public int GreatswordBurstMaximumHitCount { get; set; }

	public int GreatswordComboDashShardCost { get; set; }

	public float GreatswordComboDashLengthMultiplier { get; set; }

	public int GreatswordComboRingSlashFrames { get; set; }

	public int GreatswordComboRingSlashConsumeInterval { get; set; }

	public float GreatswordComboRingSlashDamageMultiplier { get; set; }

	public int GreatswordHitShardChargeGain { get; set; }

	public int GreatswordFinisherShardChargeBonus { get; set; }

	public float GuardNormalDamageMultiplier { get; set; }

	public float GuardWallDamageMultiplier { get; set; }

	public float GuardSuperDamageMultiplier { get; set; }

	public float GuardNormalShardCapacityMultiplier { get; set; }

	public float GuardWallShardCapacityMultiplier { get; set; }

	public float GuardSuperShardCapacityMultiplier { get; set; }

	public int FieldBurstFullCharge { get; set; }

	public int FieldBurstMinimumCharge { get; set; }

	public int FieldBurstShardCount { get; set; }

	public float FieldMinimumRadius { get; set; }

	public float FieldMaximumRadius { get; set; }

	public float FinalAssistShardDamageMultiplier { get; set; }

	public float FinalConvergenceShardDamageMultiplier { get; set; }

	public int FinalConvergenceDuration { get; set; }

	public int FinalConvergencePulseRate { get; set; }

	public float CrimsonRiftDamageMultiplier { get; set; }

	public float CrimsonRiftBladeLengthMultiplier { get; set; }

	public int CrimsonRiftShardChargeGain { get; set; }

	public int CrimsonRiftWindupFrames { get; set; }

	public int CrimsonRiftSlashFrames { get; set; }

	public int CrimsonRiftRecoveryFrames { get; set; }

	public int CrimsonRiftLocalHitCooldown { get; set; }

	public MoDaoQianRenStageStats Clone()
	{
		return (MoDaoQianRenStageStats)MemberwiseClone();
	}
}
