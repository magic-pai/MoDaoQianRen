using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using 魔刀千刃.Content.Items.Weapons;
using 魔刀千刃.Content.Players;
using 魔刀千刃.Content.Systems;

namespace 魔刀千刃.Content.Projectiles;

public class MoDaoQianRenHeldProjectile : ModProjectile
{
	public const int BladeAttackMode = 0;

	public const int ShardStreamMode = 1;

	public const int BladeReverseAttackMode = 2;

	public const int BladeHeavySlashAttackMode = 3;

	public const int BladeSpinAttackMode = 4;

	public const int GreatswordAttackMode = 5;

	public const int GreatswordReverseAttackMode = 6;

	public const int GreatswordHeavySlashAttackMode = 7;

	public const int GreatswordBurstAttackMode = 8;

	public const int GreatswordBurstChargeMode = 9;

	public const int GreatswordComboDashAttackMode = 10;

	public const int GreatswordDevilsHoldoutAttackMode = 11;

	public const float MinimumBladeLength = 120f;

	public const float CompleteBladeDistance = 150f;

	public const float SpinAttackMinimumBladeLength = 220f;

	public const float MaximumBladeLength = 880f;

	public const float GreatswordBurstMaximumBladeLength = 1540f;

	public const float GreatswordBurstLengthMultiplier = 1.62f;

	public const int GreatswordBurstChargePerHit = 8;

	public const int GreatswordBurstChargeConsumeInterval = 2;

	public const int GreatswordBurstMaximumHitCount = 24;

	public const float GreatswordBurstDamageMultiplier = 100f;

	public const int GreatswordComboDashShardCost = 5;

	public const float GreatswordComboDashLengthMultiplier = 1.55f;

	public const float GreatswordComboDashBraceProbeLength = 180f;

	private const int GreatswordComboDashWindupFrames = 8;

	private const int GreatswordComboDashExtendFrames = 16;

	private const int GreatswordComboDashActiveFrames = 26;

	private const int GreatswordComboDashRecoveryFrames = 12;

	private const int GreatswordComboDashImpulseFrames = 12;

	private const float GreatswordDevilsAimDistance = 65f;

	private const float GreatswordDevilsHoldAngle = 120f;

	private const float GreatswordDevilsSwingStartAngle = 150f;

	private const float GreatswordDevilsSwingEndAngle = 120f;

	private const float GreatswordDevilsSwingAnimationScale = 0.7f;

	private const float GreatswordDevilsPostSwingCooldownScale = 0.65f;

	private const int GreatswordDevilsMinimumUseAnimation = 72;

	private const float GreatswordDevilsFogBladeRootDistance = 46f;

	private const float GreatswordDevilsFogBladeWidth = 92f;

	private const float CrimsonRiftDamageMultiplier = 1.15f;

	private const float GreatswordComboDashImpactSpeed = 32f;

	private const float GreatswordComboDashSustainSpeed = 24f;

	private const int GreatswordComboRingSlashFrames = 34;

	private const int GreatswordComboRingSlashConsumeInterval = 6;

	private const int GreatswordComboRingSlashShardCost = 1;

	private const float GreatswordComboRingSlashDamageMultiplier = 1.8f;

	private const float GreatswordComboRingSlashMovementDamping = 0.84f;

	private const float GreatswordComboRingSlashForwardStorageOffset = 1000f;

	private const float GreatswordComboRingSlashReverseStorageOffset = 2000f;

	private const float GreatswordComboDashRootLength = 42f;

	private const float GreatswordComboDashMinimumVisibleLength = 28f;

	private const float GreatswordComboDashTileProbeStep = 6f;

	private const float GreatswordComboDashTileProbeRadius = 7f;

	public const float WeaponSourceGripOriginX = 18f;

	public const float WeaponSourceGripOriginY = 10.5f;

	public const float WeaponSourceGuardOriginX = 18f;

	public const float WeaponSourceGuardOriginY = 10.5f;

	public const float WeaponSourceBladeLength = 120.1642f;

	private const float GripOriginX = 18f;

	private const float GripOriginY = 10.5f;

	public const float WeaponOutPulseGripOriginX = 156f;

	public const float WeaponOutPulseGripOriginY = 17.5f;

	public const float GreatswordBladeRootDistance = 46f;

	private const float IconBladeLength = 120.1642f;

	public const float BaseDrawScale = 0.99863356f;

	private const float GreatswordBurstChargeGripYOffset = 6f;

	public const int WeaponOutPulseFrameCount = 8;

	public const int WeaponOutPulseFrameTicks = 5;

	private const float HeavySwingArc = 2.82f;

	private const float GreatswordSwingArc = 2.36f;

	private const float GreatswordHeavySwingArc = 3.18f;

	private const float GreatswordSlashReleaseProgress = 0.24f;

	private const float GreatswordHeavySlashReleaseProgress = 0.3f;

	private const int BladeComboTrailSamples = 22;

	private const int BladeComboFinisherTrailSamples = 31;

	private const int GreatswordSlashTrailSamples = 30;

	private const int GreatswordHeavySlashTrailSamples = 38;

	private const float BladeComboImpactFlashDuration = 9f;

	private const string TerrariaOverhaulModName = "TerrariaOverhaul";

	private static readonly SoundStyle OverhaulSwordMediumSwing = new SoundStyle("TerrariaOverhaul/Assets/Sounds/Items/Melee/CuttingSwingMedium", 2)
	{
		Volume = 0.8f,
		PitchVariance = 0.1f
	};

	private static readonly SoundStyle OverhaulSwordHeavySwing = new SoundStyle("TerrariaOverhaul/Assets/Sounds/Items/Melee/CuttingSwingHeavy", 2)
	{
		PitchVariance = 0.1f
	};

	private static readonly SoundStyle OverhaulSwordKillingBlow = new SoundStyle("TerrariaOverhaul/Assets/Sounds/Items/Melee/KillingBlow", 2)
	{
		Volume = 0.46f,
		PitchVariance = 0.08f
	};

	private Vector2 bladeImpactFlashCenter;

	private float bladeImpactFlashTimer;

	private bool greatswordSlashArcLocked;

	private Vector2 greatswordSlashArcCenter;

	private float greatswordSlashArcRotation;

	private float greatswordSlashArcOuterRadius;

	private float greatswordSlashArcDistanceFactor;

	private float greatswordSlashArcLockTimer;

	private bool greatswordDevilsInitialized;

	private bool greatswordDevilsSwinging;

	private bool greatswordDevilsCanHit;

	private bool greatswordDevilsSwooshFade;

	private bool greatswordDevilsPlaySwingSound = true;

	private int greatswordDevilsUseAnim;

	private int greatswordDevilsSwingTimer;

	private int greatswordDevilsSwingSide = -1;

	private int greatswordDevilsSwingCount;

	private int greatswordDevilsPostSwingCooldown;

	private float greatswordDevilsRotationOffset;

	private Vector2 greatswordDevilsPreviousBladeStart;

	private Vector2 greatswordDevilsPreviousBladeEnd;

	private bool greatswordDevilsHasPreviousBladeLine;

	public override string Texture => "Terraria/Images/Item_0";

	private bool IsShardStream => base.Projectile.ai[0] == 1f;

	private bool IsModeOneBladeSwing
	{
		get
		{
			if (base.Projectile.ai[0] != 0f && base.Projectile.ai[0] != 2f && base.Projectile.ai[0] != 3f)
			{
				return base.Projectile.ai[0] == 4f;
			}
			return true;
		}
	}

	private bool IsModeOneFinisher
	{
		get
		{
			if (base.Projectile.ai[0] != 3f)
			{
				return base.Projectile.ai[0] == 4f;
			}
			return true;
		}
	}

	private bool IsGreatswordSwing
	{
		get
		{
			if (base.Projectile.ai[0] != 5f && base.Projectile.ai[0] != 6f)
			{
				return base.Projectile.ai[0] == 7f;
			}
			return true;
		}
	}

	private bool IsGreatswordReverseSwing => base.Projectile.ai[0] == 6f;

	private bool IsGreatswordHeavySlash => base.Projectile.ai[0] == 7f;

	private bool IsGreatswordBurst => base.Projectile.ai[0] == 8f;

	private bool IsGreatswordBurstCharge => base.Projectile.ai[0] == 9f;

	private bool IsGreatswordComboDash => base.Projectile.ai[0] == 10f;

	private bool IsGreatswordDevilsHoldout => base.Projectile.ai[0] == 11f;

	private bool IsGreatswordLeftSlash
	{
		get
		{
			if (!IsGreatswordSwing)
			{
				return IsGreatswordDevilsHoldout;
			}
			return true;
		}
	}

	private bool IsGreatswordBladeForm
	{
		get
		{
			if (!IsGreatswordSwing && !IsGreatswordBurst && !IsGreatswordBurstCharge && !IsGreatswordDevilsHoldout)
			{
				return IsGreatswordComboDash;
			}
			return true;
		}
	}

	private bool IsReverseSwing
	{
		get
		{
			if (base.Projectile.ai[0] != 2f)
			{
				return IsGreatswordReverseSwing;
			}
			return true;
		}
	}

	private bool IsHeavySlash
	{
		get
		{
			if (base.Projectile.ai[0] != 3f)
			{
				return IsGreatswordHeavySlash;
			}
			return true;
		}
	}

	private bool IsSpinAttack => base.Projectile.ai[0] == 4f;

	private bool IsFinisherAttack
	{
		get
		{
			if (!IsModeOneFinisher)
			{
				return IsGreatswordHeavySlash;
			}
			return true;
		}
	}

	private float TargetBladeLength
	{
		get
		{
			return base.Projectile.ai[1];
		}
		set
		{
			base.Projectile.ai[1] = value;
		}
	}

	private float BaseRotation
	{
		get
		{
			if (!IsGreatswordBurst && !IsGreatswordBurstCharge && !IsGreatswordComboDash)
			{
				return base.Projectile.ai[2];
			}
			return base.Projectile.velocity.ToRotation();
		}
		set
		{
			if (IsGreatswordBurst || IsGreatswordBurstCharge || IsGreatswordComboDash)
			{
				base.Projectile.velocity = value.ToRotationVector2();
			}
			else
			{
				base.Projectile.ai[2] = value;
			}
		}
	}

	private float GreatswordBurstHitCount
	{
		get
		{
			return base.Projectile.ai[2];
		}
		set
		{
			base.Projectile.ai[2] = value;
		}
	}

	private float GreatswordBurstChargeAmount
	{
		get
		{
			return base.Projectile.ai[2];
		}
		set
		{
			base.Projectile.ai[2] = value;
		}
	}

	private ref float Timer => ref base.Projectile.localAI[0];

	private ref float AssistShardCooldown => ref base.Projectile.localAI[1];

	private ref float GreatswordComboRingSlashStartRotation => ref base.Projectile.localAI[1];

	private float GreatswordComboRingSlashRotation
	{
		get
		{
			float storedRotation = GreatswordComboRingSlashStartRotation;
			if (IsGreatswordComboRingSlashStoredRotation(storedRotation, 2000f))
			{
				return storedRotation - 2000f;
			}
			if (IsGreatswordComboRingSlashStoredRotation(storedRotation, 1000f))
			{
				return storedRotation - 1000f;
			}
			return base.Projectile.velocity.ToRotation();
		}
	}

	private float GreatswordComboRingSlashSpinDirection
	{
		get
		{
			if (!IsGreatswordComboRingSlashStoredRotation(GreatswordComboRingSlashStartRotation, 2000f))
			{
				return 1f;
			}
			return -1f;
		}
	}

	private bool GreatswordComboDashHasHit
	{
		get
		{
			return base.Projectile.ai[2] > 0f;
		}
		set
		{
			base.Projectile.ai[2] = (value ? MathF.Max(1f, Timer) : 0f);
		}
	}

	private bool IsGreatswordComboRingSlash => base.Projectile.ai[2] < 0f;

	private float GreatswordComboRingSlashTimer
	{
		get
		{
			return base.Projectile.localAI[2];
		}
		set
		{
			base.Projectile.localAI[2] = MathF.Max(1f, value);
		}
	}

	private static bool IsGreatswordComboRingSlashStoredRotation(float value, float offset)
	{
		float rotation = value - offset;
		if (rotation >= -3.1425927f)
		{
			return rotation <= 3.1425927f;
		}
		return false;
	}

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[base.Type] = true;
	}

	public override void SetDefaults()
	{
		base.Projectile.width = 32;
		base.Projectile.height = 32;
		base.Projectile.friendly = true;
		base.Projectile.hostile = false;
		base.Projectile.penetrate = -1;
		base.Projectile.tileCollide = false;
		base.Projectile.ignoreWater = true;
		base.Projectile.ownerHitCheck = false;
		base.Projectile.DamageType = DamageClass.Melee;
		base.Projectile.noEnchantmentVisuals = true;
		base.Projectile.usesLocalNPCImmunity = true;
		base.Projectile.localNPCHitCooldown = 8;
		base.Projectile.timeLeft = 2;
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	private int GetLocalNpcHitCooldown()
	{
		if (IsGreatswordComboDash)
		{
			if (!IsGreatswordComboRingSlash)
			{
				return 999;
			}
			return 5;
		}
		if (IsGreatswordBurst && base.Projectile.owner >= 0 && base.Projectile.owner < 255)
		{
			int hitCount = GetGreatswordBurstHitCount();
			int duration = GetAttackDuration(Main.player[base.Projectile.owner]);
			GetDamageWindow(out var damageWindowStart, out var damageWindowEnd);
			int activeFrames = Math.Max(1, (int)MathF.Round((float)duration * (damageWindowEnd - damageWindowStart)));
			return Math.Max(1, activeFrames / hitCount);
		}
		if (IsModeOneBladeSwing)
		{
			if (!IsModeOneFinisher)
			{
				return 5;
			}
			return 6;
		}
		if (IsGreatswordDevilsHoldout)
		{
			return -1;
		}
		if (IsGreatswordHeavySlash)
		{
			return 5;
		}
		if (!IsGreatswordSwing)
		{
			return 8;
		}
		return 7;
	}

	public override bool? CanDamage()
	{
		if (IsShardStream || IsGreatswordBurstCharge || base.Projectile.owner < 0 || base.Projectile.owner >= 255)
		{
			return false;
		}
		if (IsGreatswordDevilsHoldout)
		{
			return greatswordDevilsCanHit ? null : false;
		}
		if (IsGreatswordComboDash)
		{
			if (!IsGreatswordComboRingSlash)
			{
				return false;
			}
			return null;
		}
		int duration = GetAttackDuration(Main.player[base.Projectile.owner]);
		float progress = Utils.GetLerpValue(0f, duration, Timer, clamped: true);
		GetDamageWindow(out var damageWindowStart, out var damageWindowEnd);
		if (!DoesCurrentFrameReachDamageWindow(duration, progress, damageWindowStart, damageWindowEnd))
		{
			return false;
		}
		return null;
	}

	public override void AI()
	{
		Player player = Main.player[base.Projectile.owner];
		if (!player.active || player.dead)
		{
			base.Projectile.Kill();
			return;
		}
		if (MoDaoQianRenWarmupSystem.ShouldBlockBladeUseForLights)
		{
			base.Projectile.Kill();
			return;
		}
		base.Projectile.scale = GetOwnerBladeScale(player);
		base.Projectile.localNPCHitCooldown = GetLocalNpcHitCooldown();
		if (IsGreatswordBladeForm)
		{
			base.Projectile.CritChance = Math.Max(base.Projectile.CritChance, MoDaoQianRen.GetGreatswordCritChance(GetCurrentGrowthStage(player)));
		}
		Timer += 1f;
		if (bladeImpactFlashTimer > 0f)
		{
			bladeImpactFlashTimer -= 1f;
		}
		if (!IsGreatswordComboRingSlash && AssistShardCooldown > 0f)
		{
			AssistShardCooldown -= 1f;
		}
		Vector2 handPosition = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
		if (IsGreatswordDevilsHoldout)
		{
			UpdateGreatswordDevilsHoldout(player, handPosition);
			if (!base.Projectile.active)
			{
				return;
			}
			UpdatePlayerVisuals(player, handPosition);
		}
		else
		{
			UpdateAimAndSwing(player, handPosition);
			UpdatePlayerVisuals(player, handPosition);
			UpdateGreatswordSlashArcLock(player);
			if (IsShardStream)
		{
			UpdateShardStream(player, handPosition);
			ProduceStreamChargeEffects(handPosition);
		}
		else if (IsGreatswordBurstCharge)
		{
			UpdateGreatswordBurstCharge(player, handPosition);
			if (!base.Projectile.active)
			{
				return;
			}
			ProduceGreatswordBurstChargeEffects(player, handPosition);
		}
		else if (IsGreatswordComboDash)
		{
			UpdateGreatswordComboDash(player, handPosition);
			if (!base.Projectile.active)
			{
				return;
			}
			ProduceGreatswordComboDashEffects(player);
		}
		else
		{
			ProduceAttackRhythmEvents(player);
			ProduceSwingEffects();
			int duration = GetAttackDuration(player);
			if (Timer >= (float)duration)
			{
				base.Projectile.Kill();
				return;
			}
		}
		}
		MoDaoQianRenWarmupSystem.AddLight(base.Projectile.Center, 0.45f, 0.16f, 0.9f);
		base.Projectile.timeLeft = 2;
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		if (IsShardStream || IsGreatswordBurstCharge)
		{
			return false;
		}
		if (IsGreatswordComboDash)
		{
			if (IsGreatswordComboRingSlash)
			{
				if (base.Projectile.owner < 0 || base.Projectile.owner >= 255)
				{
					return false;
				}
				Player ownerPlayer = Main.player[base.Projectile.owner];
				float radius = GetGreatswordComboRingSlashRadius();
				float ringWidth = MathHelper.Lerp(52f, 86f, GetDistanceFactor(radius)) * MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f);
				float innerHitRadius = MathHelper.Lerp(112f, 156f, GetDistanceFactor(radius)) * MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f);
				Vector2 targetOffset = targetHitbox.Center.ToVector2() - ownerPlayer.Center;
				float distance = targetOffset.Length();
				Rectangle innerBounds = Utils.CenteredRectangle(ownerPlayer.Center, new Vector2(innerHitRadius * 2f));
				if (targetHitbox.Intersects(innerBounds))
				{
					return true;
				}
				if (distance > radius + ringWidth)
				{
					return false;
				}
				float sweptAngle = (float)Math.PI * 2f * GetGreatswordComboRingSlashProgress();
				if (sweptAngle < 6.2031856f && !IsAngleInsideGreatswordComboRingSweep(targetOffset.ToRotation(), GreatswordComboRingSlashRotation, sweptAngle, ringWidth / MathF.Max(64f, radius), GreatswordComboRingSlashSpinDirection))
				{
					return false;
				}
				Rectangle ringBounds = Utils.CenteredRectangle(ownerPlayer.Center, new Vector2((radius + ringWidth) * 2f));
				return targetHitbox.Intersects(ringBounds);
			}
			if (!IsGreatswordComboDashActive() || GreatswordComboDashHasHit)
			{
				return false;
			}
			Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
			if (!TryGetGreatswordComboDashCollisionLine(direction, out var start, out var end, out var dashWidth))
			{
				return false;
			}
			float dashCollisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, dashWidth, ref dashCollisionPoint);
		}
		if (IsGreatswordDevilsHoldout)
		{
			if (!greatswordDevilsCanHit || base.Projectile.owner < 0 || base.Projectile.owner >= 255)
			{
				return false;
			}
			Player devilsPlayer = Main.player[base.Projectile.owner];
			float devilsCollisionPoint = 0f;
			float devilsWidth = MathHelper.Lerp(60f, 98f, GetDistanceFactor(TargetBladeLength)) * MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f);
			GetGreatswordDevilsBladeLine(devilsPlayer, out var currentStart, out var currentEnd);
			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), currentStart, currentEnd, devilsWidth, ref devilsCollisionPoint))
			{
				return true;
			}
			if (greatswordDevilsHasPreviousBladeLine && Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), greatswordDevilsPreviousBladeStart, greatswordDevilsPreviousBladeEnd, devilsWidth, ref devilsCollisionPoint))
			{
				return true;
			}
			return false;
		}
		if (base.Projectile.owner < 0 || base.Projectile.owner >= 255)
		{
			return false;
		}
		Player player = Main.player[base.Projectile.owner];
		int duration = GetAttackDuration(player);
		float progress = Utils.GetLerpValue(0f, duration, Timer, clamped: true);
		GetDamageWindow(out var damageWindowStart, out var damageWindowEnd);
		if (!DoesCurrentFrameReachDamageWindow(duration, progress, damageWindowStart, damageWindowEnd))
		{
			return false;
		}
		float collisionPoint = 0f;
		float distanceFactor = GetDistanceFactor(TargetBladeLength);
		float width = (IsGreatswordBurst ? MathHelper.Lerp(92f, 168f, distanceFactor) : ((!IsGreatswordSwing) ? (IsHeavySlash ? MathHelper.Lerp(50f, 82f, distanceFactor) : (IsSpinAttack ? MathHelper.Lerp(32f, 50f, GetDistanceFactor(TargetBladeLength)) : MathHelper.Lerp(24f, 40f, GetDistanceFactor(TargetBladeLength)))) : (IsGreatswordHeavySlash ? MathHelper.Lerp(84f, 132f, distanceFactor) : MathHelper.Lerp(60f, 98f, distanceFactor))));
		width *= MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f);
		float sweepBack = (IsGreatswordBurst ? 0.48f : ((!IsGreatswordSwing) ? (IsHeavySlash ? 0.32f : (IsSpinAttack ? 0.3f : 0.24f)) : (IsGreatswordHeavySlash ? 0.42f : 0.34f)));
		float sampleEnd = MathHelper.Clamp(progress, damageWindowStart, damageWindowEnd);
		float sweepStart = MathF.Max(damageWindowStart, sampleEnd - sweepBack);
		int samples = (IsGreatswordBurst ? 34 : ((!IsGreatswordSwing) ? (IsHeavySlash ? 18 : (IsSpinAttack ? 18 : 12)) : (IsGreatswordHeavySlash ? 30 : 24)));
		for (int i = 0; i < samples; i++)
		{
			float sampleProgress = MathHelper.Lerp(sweepStart, sampleEnd, (samples <= 1) ? 1f : ((float)i / (float)(samples - 1)));
			if (IsDamageWindowActive(sampleProgress))
			{
				GetBladeLineAtProgress(player, sampleProgress, out var start2, out var end2);
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start2, end2, width, ref collisionPoint))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void CutTiles()
	{
		if (!IsShardStream && !IsGreatswordBurstCharge)
		{
			Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			float cutWidth = (IsGreatswordComboDash ? 52f : (IsGreatswordBurst ? 96f : (IsGreatswordDevilsHoldout ? 62f : ((!IsGreatswordSwing) ? 32f : (IsGreatswordHeavySlash ? 84f : 62f)))));
			float bladeLength = (IsGreatswordComboDash ? GetGreatswordComboDashVisibleLength() : GetVisualBladeLength());
			if (!(bladeLength <= 34f))
			{
				Utils.PlotTileLine(base.Projectile.Center + direction * 32f, base.Projectile.Center + direction * bladeLength, cutWidth * MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f), DelegateMethods.CutTiles);
			}
		}
	}

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
	{
		if (IsGreatswordDevilsHoldout)
		{
			modifiers.SourceDamage *= CrimsonRiftDamageMultiplier;
		}
		if (IsGreatswordComboDash && IsGreatswordComboRingSlash)
		{
			int growthStage = ((base.Projectile.owner >= 0 && base.Projectile.owner < 255) ? GetCurrentGrowthStage(Main.player[base.Projectile.owner]) : 0);
			modifiers.SourceDamage *= MoDaoQianRen.GetGreatswordDamageMultiplier(growthStage, 7) * MoDaoQianRen.GetRuntimeStats(growthStage).GreatswordComboRingSlashDamageMultiplier;
		}
		if (IsGreatswordBladeForm && base.Projectile.owner >= 0 && base.Projectile.owner < 255 && MoDaoQianRen.GetGreatswordCritChance(GetCurrentGrowthStage(Main.player[base.Projectile.owner])) >= 100)
		{
			modifiers.SetCrit();
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (IsShardStream || base.Projectile.owner < 0 || base.Projectile.owner >= 255)
		{
			return;
		}
		Player player = Main.player[base.Projectile.owner];
		if (!MoDaoQianRen.TryGetGrowthStage(player.HeldItem, out var growthStage))
		{
			return;
		}
		if (IsGreatswordBurst)
		{
			ProduceHitEffects(target, damageDone);
			if (AssistShardCooldown <= 0f)
			{
				AssistShardCooldown = 4f;
				TryAddSwordScreenShake(player, base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction) * 0.82f);
			}
			return;
		}
		if (IsGreatswordComboDash)
		{
			if (IsGreatswordComboRingSlash)
			{
				ProduceHitEffects(target, damageDone);
				TryAddSwordScreenShake(player, (target.Center - player.Center).SafeNormalize(base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction)) * 0.82f);
				return;
			}
			GreatswordComboDashHasHit = true;
			base.Projectile.velocity *= 0.18f;
			player.velocity *= 0.18f;
			ProduceHitEffects(target, damageDone);
			TryAddSwordScreenShake(player, (target.Center - player.Center).SafeNormalize(base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction)) * 1.9f);
			base.Projectile.netUpdate = true;
			return;
		}
		if (IsGreatswordDevilsHoldout)
		{
			MoDaoQianRenPlayer bladePlayer = player.GetModPlayer<MoDaoQianRenPlayer>();
			bladePlayer.AddShardCharge(GetShardChargeGain(target, growthStage));
			ProduceCrimsonRiftHitEffects(target, player, damageDone);
			if (growthStage >= 6 && growthStage < 7 && base.Projectile.owner == Main.myPlayer)
			{
				TryReleaseMoonlitGreatswordAssistShards(player, target, growthStage);
			}
			if (growthStage >= 7 && base.Projectile.owner == Main.myPlayer)
			{
				TryReleaseFinalAssistShards(player, target, growthStage);
			}
			return;
		}
		player.GetModPlayer<MoDaoQianRenPlayer>().AddShardCharge(GetShardChargeGain(target, growthStage));
		bool num = IsModeOneBladeSwing && bladeImpactFlashTimer <= 0f;
		ProduceHitEffects(target, damageDone);
		if (num)
		{
			TryAddSwordScreenShake(player, base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction) * (IsModeOneFinisher ? 0.95f : 0.52f));
		}
		if (IsModeOneBladeSwing && growthStage >= 1 && growthStage < 7 && base.Projectile.owner == Main.myPlayer)
		{
			TryReleaseComboShard(player, target, growthStage);
		}
		if (IsGreatswordLeftSlash && growthStage >= 6 && growthStage < 7 && base.Projectile.owner == Main.myPlayer)
		{
			TryReleaseMoonlitGreatswordAssistShards(player, target, growthStage);
		}
		if (growthStage >= 7 && base.Projectile.owner == Main.myPlayer)
		{
				TryReleaseFinalAssistShards(player, target, growthStage);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		if (MoDaoQianRenWarmupSystem.ShouldSkipCustomDrawingForLights)
		{
			return false;
		}
		Texture2D hiltTexture = ModContent.Request<Texture2D>(MoDaoQianRenMod.WeaponHiltTexture).Value;
		Player player = ((base.Projectile.owner >= 0 && base.Projectile.owner < 255) ? Main.player[base.Projectile.owner] : null);
		Vector2 v = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
		Vector2 drawPosition = base.Projectile.Center - Main.screenPosition;
		float rotation = v.ToRotation();
		SpriteEffects effects = ((base.Projectile.spriteDirection == -1) ? SpriteEffects.FlipVertically : SpriteEffects.None);
		if (player != null && IsGreatswordBurstCharge)
		{
			float chargePower = GetGreatswordBurstChargePower(player.GetModPlayer<MoDaoQianRenPlayer>());
			float slowShake = MathF.Sin(Timer * MathHelper.Lerp(0.75f, 1.35f, chargePower)) * MathHelper.Lerp(0.004f, 0.026f, chargePower);
			float snapShake = MathF.Sin(Timer * 2.15f) * MathHelper.Lerp(0.0015f, 0.011f, chargePower);
			rotation += slowShake + snapShake;
		}
		if (player != null && IsModeOneBladeSwing)
		{
			DrawModeOneBladeSlash(player, foreground: false);
		}
		if (player != null && IsGreatswordSwing)
		{
			DrawGreatswordSlashArc(player, foreground: false);
		}
		if (player != null && IsGreatswordComboRingSlash)
		{
			DrawGreatswordComboRingSlash(player, foreground: false);
		}
		if (IsShardStream)
		{
			DrawHandle(hiltTexture, drawPosition, rotation, effects, base.Projectile.scale);
		}
		else if (IsGreatswordDevilsHoldout)
		{
			DrawGreatswordDevilsFogBlade(hiltTexture, drawPosition, v, rotation, effects);
		}
		else if (!IsGreatswordBladeForm && TargetBladeLength <= 150f)
		{
			Texture2D completeBladeTexture = ModContent.Request<Texture2D>("魔刀千刃/Content/Items/Weapons/MoDaoQianRen_weaponout_pulse").Value;
			DrawCompleteBlade(completeBladeTexture, drawPosition, rotation, effects);
		}
		else
		{
			DrawSplitBlade(hiltTexture, drawPosition, rotation, effects);
		}
		if (player != null && IsModeOneBladeSwing)
		{
			DrawModeOneBladeSlash(player, foreground: true);
			DrawModeOneImpactFlash();
		}
		if (player != null && IsGreatswordSwing)
		{
			DrawGreatswordSlashArc(player, foreground: true);
		}
		if (player != null && IsGreatswordComboRingSlash)
		{
			DrawGreatswordComboRingSlash(player, foreground: true);
		}
		return false;
	}

	public static float GetDistanceFactor(float bladeLength)
	{
		return Utils.GetLerpValue(150f, 880f, bladeLength, clamped: true);
	}

	public static int GetGreatswordBurstHitCountFromCharge(int shardCharge)
	{
		return GetGreatswordBurstHitCountFromCharge(shardCharge, 0);
	}

	private static int GetGreatswordBurstHitCountFromCharge(int shardCharge, int growthStage)
	{
		if (shardCharge <= 0)
		{
			return 0;
		}
		MoDaoQianRenStageStats stats = MoDaoQianRen.GetRuntimeStats(growthStage);
		return Utils.Clamp((int)MathF.Ceiling((float)shardCharge / (float)stats.GreatswordBurstChargePerHit), 1, stats.GreatswordBurstMaximumHitCount);
	}

	private int GetGreatswordBurstHitCount()
	{
		int growthStage = ((base.Projectile.owner >= 0 && base.Projectile.owner < 255) ? GetCurrentGrowthStage(Main.player[base.Projectile.owner]) : 0);
		return Utils.Clamp((int)MathF.Round(GreatswordBurstHitCount), 1, MoDaoQianRen.GetRuntimeStats(growthStage).GreatswordBurstMaximumHitCount);
	}

	public static int GetShardStreamConsumeRate(int growthStage)
	{
		return MoDaoQianRen.GetShardStreamConsumeRate(growthStage);
	}

	public static Rectangle GetWeaponOutPulseFrame(Texture2D texture)
	{
		int frameWidth = texture.Width / 8;
		int frameIndex = (int)(Main.GameUpdateCount / 5 % 8);
		return new Rectangle(frameWidth * frameIndex, 0, frameWidth, texture.Height);
	}

	private static int GetBaseAttackDuration(float bladeLength, int growthStage)
	{
		float distanceFactor = GetDistanceFactor(bladeLength);
		return growthStage switch
		{
			7 => (int)MathHelper.Lerp(13f, 20f, distanceFactor), 
			6 => (int)MathHelper.Lerp(15f, 22f, distanceFactor), 
			5 => (int)MathHelper.Lerp(16f, 23f, distanceFactor), 
			4 => (int)MathHelper.Lerp(18f, 25f, distanceFactor), 
			3 => (int)MathHelper.Lerp(20f, 28f, distanceFactor), 
			2 => (int)MathHelper.Lerp(21f, 29f, distanceFactor), 
			1 => (int)MathHelper.Lerp(23f, 30f, distanceFactor), 
			_ => (int)MathHelper.Lerp(22f, 28f, distanceFactor), 
		};
	}

	private int GetAttackDuration(Player player)
	{
		int growthStage = GetCurrentGrowthStage(player);
		int baseDuration = GetBaseAttackDuration(TargetBladeLength, growthStage);
		if (IsGreatswordComboDash)
		{
			return 46;
		}
		if (IsGreatswordBurst)
		{
			int num = ((growthStage >= 7) ? 72 : ((growthStage < 6) ? 84 : 78));
			baseDuration = num;
		}
		else if (IsGreatswordSwing)
		{
			baseDuration = MoDaoQianRen.GetGreatswordAttackDuration(growthStage, TargetBladeLength, (int)base.Projectile.ai[0]);
		}
		else if (IsHeavySlash)
		{
			float distanceFactor = GetDistanceFactor(TargetBladeLength);
			baseDuration = Math.Max(20, (int)MathF.Round((float)baseDuration * 1.08f + MathHelper.Lerp(5f, 9f, distanceFactor)));
		}
		else if (IsSpinAttack)
		{
			float distanceFactor2 = GetDistanceFactor(TargetBladeLength);
			baseDuration = Math.Max(24, baseDuration + (int)MathF.Round(MathHelper.Lerp(7f, 11f, distanceFactor2)));
		}
		else
		{
			float distanceFactor3 = GetDistanceFactor(TargetBladeLength);
			baseDuration = (int)MathF.Round((float)baseDuration * 1.2f + MathHelper.Lerp(3f, 5f, distanceFactor3));
		}
		int prefixedDuration = ((player.HeldItem?.ModItem is MoDaoQianRen blade) ? blade.ApplyUseTimePrefix(baseDuration) : baseDuration);
		int minimumDuration = ((!IsGreatswordBurst) ? (IsGreatswordSwing ? MoDaoQianRen.GetGreatswordMinimumAttackDuration(growthStage, (int)base.Projectile.ai[0]) : ((!IsHeavySlash && !IsSpinAttack) ? ((growthStage >= 5) ? 8 : 10) : ((growthStage >= 5) ? 13 : 15))) : ((growthStage >= 6) ? 58 : 62));
		return MoDaoQianRen.ApplyMeleeAttackSpeed(player, prefixedDuration, minimumDuration);
	}

	private bool IsDamageWindowActive(float progress)
	{
		GetDamageWindow(out var damageWindowStart, out var damageWindowEnd);
		if (progress >= damageWindowStart)
		{
			return progress <= damageWindowEnd;
		}
		return false;
	}

	private void GetDamageWindow(out float start, out float end)
	{
		if (IsGreatswordComboDash)
		{
			int duration = 46;
			start = 8f / (float)duration;
			end = 34f / (float)duration;
		}
		else if (IsGreatswordBurst)
		{
			start = 0.14f;
			end = 0.72f;
		}
		else if (IsGreatswordHeavySlash)
		{
			start = 0.24f;
			end = 0.88f;
		}
		else if (IsGreatswordSwing)
		{
			start = 0.26f;
			end = 0.8f;
		}
		else if (IsHeavySlash)
		{
			start = 0.16f;
			end = 0.92f;
		}
		else if (IsSpinAttack)
		{
			start = 0.14f;
			end = 0.92f;
		}
		else
		{
			start = 0.16f;
			end = 0.88f;
		}
	}

	private bool DoesCurrentFrameReachDamageWindow(int duration, float progress, float damageWindowStart, float damageWindowEnd)
	{
		if (progress >= damageWindowStart && progress <= damageWindowEnd)
		{
			return true;
		}
		if (Utils.GetLerpValue(0f, duration, MathF.Max(0f, Timer - 1f), clamped: true) <= damageWindowEnd)
		{
			return progress >= damageWindowStart;
		}
		return false;
	}

	private float GetHeavySlashProgress(float progress)
	{
		if (progress < 0.14f)
		{
			return 0f;
		}
		if (progress < 0.28f)
		{
			return MathHelper.Lerp(0f, 0.14f, SmoothStep(Utils.GetLerpValue(0.14f, 0.28f, progress, clamped: true)));
		}
		if (progress < 0.62f)
		{
			return MathHelper.Lerp(0.14f, 0.9f, SmoothStep(Utils.GetLerpValue(0.28f, 0.62f, progress, clamped: true)));
		}
		if (progress < 0.78f)
		{
			return MathHelper.Lerp(0.9f, 1f, SmoothStep(Utils.GetLerpValue(0.62f, 0.78f, progress, clamped: true)));
		}
		if (progress < 0.94f)
		{
			return MathHelper.Lerp(1f, 0.96f, SmoothStep(Utils.GetLerpValue(0.78f, 0.94f, progress, clamped: true)));
		}
		return 0.96f;
	}

	private float GetGreatswordSwingProgress(float progress)
	{
		if (IsGreatswordHeavySlash)
		{
			if (progress < 0.12f)
			{
				return MathHelper.Lerp(0f, 0.08f, SmoothStep(Utils.GetLerpValue(0f, 0.12f, progress, clamped: true)));
			}
			if (progress < 0.28f)
			{
				return MathHelper.Lerp(0.08f, 0.24f, SmoothStep(Utils.GetLerpValue(0.12f, 0.28f, progress, clamped: true)));
			}
			if (progress < 0.62f)
			{
				return MathHelper.Lerp(0.24f, 0.94f, SmoothStep(Utils.GetLerpValue(0.28f, 0.62f, progress, clamped: true)));
			}
			if (progress < 0.78f)
			{
				return MathHelper.Lerp(0.94f, 1f, SmoothStep(Utils.GetLerpValue(0.62f, 0.78f, progress, clamped: true)));
			}
			if (progress < 0.94f)
			{
				return MathHelper.Lerp(1f, 0.985f, SmoothStep(Utils.GetLerpValue(0.78f, 0.94f, progress, clamped: true)));
			}
			return 0.985f;
		}
		if (progress < 0.1f)
		{
			return MathHelper.Lerp(0f, 0.08f, SmoothStep(Utils.GetLerpValue(0f, 0.1f, progress, clamped: true)));
		}
		if (progress < 0.24f)
		{
			return MathHelper.Lerp(0.08f, 0.2f, SmoothStep(Utils.GetLerpValue(0.1f, 0.24f, progress, clamped: true)));
		}
		if (progress < 0.56f)
		{
			return MathHelper.Lerp(0.2f, 0.94f, SmoothStep(Utils.GetLerpValue(0.24f, 0.56f, progress, clamped: true)));
		}
		if (progress < 0.72f)
		{
			return MathHelper.Lerp(0.94f, 1f, SmoothStep(Utils.GetLerpValue(0.56f, 0.72f, progress, clamped: true)));
		}
		if (progress < 0.9f)
		{
			return MathHelper.Lerp(1f, 0.985f, SmoothStep(Utils.GetLerpValue(0.72f, 0.9f, progress, clamped: true)));
		}
		return 0.985f;
	}

	private static float GetGreatswordSlashArcSwingProgress(float progress)
	{
		if (progress < 0.2f)
		{
			return 0f;
		}
		if (progress < 0.36f)
		{
			return MathHelper.Lerp(0f, 0.12f, SmoothStep(Utils.GetLerpValue(0.2f, 0.36f, progress, clamped: true)));
		}
		if (progress < 0.56f)
		{
			return MathHelper.Lerp(0.12f, 0.9f, SmoothStep(Utils.GetLerpValue(0.36f, 0.56f, progress, clamped: true)));
		}
		if (progress < 0.78f)
		{
			return MathHelper.Lerp(0.9f, 1f, SmoothStep(Utils.GetLerpValue(0.56f, 0.78f, progress, clamped: true)));
		}
		if (progress < 0.96f)
		{
			return MathHelper.Lerp(1f, 0.985f, SmoothStep(Utils.GetLerpValue(0.78f, 0.96f, progress, clamped: true)));
		}
		return 0.985f;
	}

	private Vector2 GetHeldCenterOffset(Player player)
	{
		int duration = GetAttackDuration(player);
		float progress = Utils.GetLerpValue(0f, duration, Timer, clamped: true);
		Vector2 direction = GetAttackDirectionAtProgress(progress);
		return GetHeldCenterOffsetAtProgress(player, progress, direction);
	}

	private Vector2 GetHeldCenterOffsetAtProgress(Player player, float progress, Vector2 direction)
	{
		if (IsShardStream)
		{
			return Vector2.Zero;
		}
		if (IsGreatswordDevilsHoldout)
		{
			return GetGreatswordDevilsHeldOffsetAtFrame(greatswordDevilsSwingTimer, direction);
		}
		Vector2 normal = direction.RotatedBy(1.5707963705062866);
		float distanceFactor = GetDistanceFactor(TargetBladeLength);
		if (IsGreatswordComboDash)
		{
			if (IsGreatswordComboRingSlash)
			{
				float ringProgress = GetGreatswordComboRingSlashProgress();
				Vector2 ringLine = GetGreatswordComboRingSlashDirection();
				Vector2 ringNormal = ringLine.RotatedBy(1.5707963705062866);
				return StabilizeGripOffset(ringLine * MathF.Sin(ringProgress * ((float)Math.PI * 2f)) * 10f + ringNormal * MathF.Cos(ringProgress * ((float)Math.PI * 2f)) * 6f);
			}
			float duration = 46f;
			float dashProgress = Timer / duration;
			Vector2 centerLine = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
			float reverseGrip = MathHelper.Lerp(-18f, -8f, SmoothStep(Utils.GetLerpValue(0f, 0.22f, dashProgress, clamped: true)));
			float braceSway = MathF.Sin(Timer * 0.42f) * MathHelper.Lerp(0.5f, 2.4f, GetDistanceFactor(TargetBladeLength));
			return StabilizeGripOffset(centerLine * reverseGrip + normal * braceSway);
		}
		if (IsGreatswordBurstCharge)
		{
			return Vector2.Zero;
		}
		if (IsGreatswordBurst)
		{
			float greatswordBurstPower = GetGreatswordBurstPower(progress);
			float windupPull = ((progress < 0.18f) ? MathHelper.Lerp(-8f, -30f, SmoothStep(Utils.GetLerpValue(0f, 0.18f, progress, clamped: true))) : ((progress > 0.76f) ? MathHelper.Lerp(14f, -4f, SmoothStep(Utils.GetLerpValue(0.76f, 1f, progress, clamped: true))) : 0f));
			Vector2 centerLine2 = BaseRotation.ToRotationVector2();
			float forwardDrive = greatswordBurstPower * MathHelper.Lerp(24f, 52f, distanceFactor);
			float recoilSway = MathF.Sin(Utils.GetLerpValue(0.16f, 0.58f, progress, clamped: true) * (float)Math.PI) * MathHelper.Lerp(2f, 7f, distanceFactor);
			return StabilizeGripOffset(centerLine2 * (windupPull + forwardDrive) + normal * recoilSway);
		}
		if (IsGreatswordSwing)
		{
			float greatswordSlashPower = GetGreatswordSlashPower(progress);
			float windupEnd = IsGreatswordHeavySlash ? 0.12f : 0.08f;
			float windupPull2 = ((progress < windupEnd) ? MathHelper.Lerp(-6f, IsGreatswordHeavySlash ? (-14f) : (-10f), SmoothStep(Utils.GetLerpValue(0f, windupEnd, progress, clamped: true))) : ((progress > 0.5f) ? MathHelper.Lerp(IsGreatswordHeavySlash ? 6f : 4f, -3f, SmoothStep(Utils.GetLerpValue(0.5f, 1f, progress, clamped: true))) : 0f));
			Vector2 centerLine3 = direction.SafeNormalize(BaseRotation.ToRotationVector2());
			float forwardDrive2 = greatswordSlashPower * (IsGreatswordHeavySlash ? MathHelper.Lerp(18f, 30f, distanceFactor) : MathHelper.Lerp(16f, 28f, distanceFactor));
			float sideWeight = MathF.Sin(GetGreatswordSwingProgress(progress) * (float)Math.PI) * (IsReverseSwing ? (-1f) : 1f) * (IsGreatswordHeavySlash ? MathHelper.Lerp(6f, 12f, distanceFactor) : MathHelper.Lerp(4f, 10f, distanceFactor));
			return StabilizeGripOffset(centerLine3 * (windupPull2 + forwardDrive2) + normal * sideWeight);
		}
		if (IsHeavySlash)
		{
			float heavySlashPower = GetSlashPower(progress);
			float heavyPullback = ((progress < 0.18f) ? MathHelper.Lerp(-18f, -34f, Utils.GetLerpValue(0f, 0.18f, progress, clamped: true)) : 0f);
			Vector2 centerLine4 = BaseRotation.ToRotationVector2();
			return StabilizeGripOffset(centerLine4 * (heavyPullback + heavySlashPower * MathHelper.Lerp(8f, 18f, distanceFactor)));
		}
		if (IsSpinAttack)
		{
			float spinPower = GetSpinPower(progress);
			float spinProgress = GetSpinProgress(progress);
			return StabilizeGripOffset(direction * (spinPower * MathHelper.Lerp(4f, 13f, distanceFactor)) + normal * MathF.Sin(spinProgress * ((float)Math.PI * 2f)) * MathHelper.Lerp(2f, 9f, distanceFactor));
		}
		float slashPower = GetSlashPower(progress);
		float pullback = ((progress < 0.2f) ? MathHelper.Lerp(-6f, -11f, Utils.GetLerpValue(0f, 0.2f, progress, clamped: true)) : 0f);
		return StabilizeGripOffset(direction * (pullback + slashPower * MathHelper.Lerp(2f, 8f, distanceFactor)) + normal * MathF.Sin(slashPower * (float)Math.PI) * (IsReverseSwing ? (-3f) : 3f));
	}

	private Vector2 StabilizeGripOffset(Vector2 animatedOffset)
	{
		float gripSwayScale = (IsGreatswordComboDash ? 0.34f : ((!IsGreatswordSwing) ? (IsGreatswordBurstCharge ? 0.42f : (IsGreatswordBurst ? 0.12f : ((IsHeavySlash || IsSpinAttack) ? 0.22f : 0.35f))) : (IsGreatswordHeavySlash ? 0.14f : 0.18f)));
		return animatedOffset * gripSwayScale;
	}

	private float GetSlashPower(float progress)
	{
		if (!(progress < 0.16f) && !(progress > 0.82f))
		{
			return MathF.Sin(Utils.GetLerpValue(0.16f, 0.82f, progress, clamped: true) * (float)Math.PI);
		}
		return 0f;
	}

	private float GetSpinProgress(float progress)
	{
		return SmoothStep(Utils.GetLerpValue(0.16f, 0.86f, progress, clamped: true));
	}

	private float GetSpinPower(float progress)
	{
		if (progress < 0.14f || progress > 0.94f)
		{
			return 0f;
		}
		float lerpValue = Utils.GetLerpValue(0.14f, 0.26f, progress, clamped: true);
		float fadeOut = 1f - Utils.GetLerpValue(0.86f, 0.94f, progress, clamped: true);
		return MathHelper.Clamp(MathF.Min(lerpValue, fadeOut), 0f, 1f);
	}

	private float GetGreatswordSlashPower(float progress)
	{
		float start = IsGreatswordHeavySlash ? 0.24f : 0.22f;
		float peak = IsGreatswordHeavySlash ? 0.48f : 0.42f;
		float fadeStart = IsGreatswordHeavySlash ? 0.76f : 0.72f;
		float end = IsGreatswordHeavySlash ? 0.9f : 0.88f;
		if (progress < start || progress > end)
		{
			return 0f;
		}
		float num = SmoothStep(Utils.GetLerpValue(start, peak, progress, clamped: true));
		float fadeOut = 1f - SmoothStep(Utils.GetLerpValue(fadeStart, end, progress, clamped: true));
		return MathHelper.Clamp(MathF.Min(num, fadeOut), 0f, 1f);
	}

	private static float GetGreatswordSlashArcPower(float progress)
	{
		if (progress < 0.24f || progress > 0.9f)
		{
			return 0f;
		}
		float num = SmoothStep(Utils.GetLerpValue(0.24f, 0.48f, progress, clamped: true));
		float fadeOut = 1f - SmoothStep(Utils.GetLerpValue(0.74f, 0.9f, progress, clamped: true));
		return MathHelper.Clamp(MathF.Min(num, fadeOut), 0f, 1f);
	}

	private float GetGreatswordBurstPower(float progress)
	{
		if (progress < 0.16f || progress > 0.92f)
		{
			return 0f;
		}
		float num = SmoothStep(Utils.GetLerpValue(0.16f, 0.32f, progress, clamped: true));
		float fadeOut = 1f - SmoothStep(Utils.GetLerpValue(0.78f, 0.92f, progress, clamped: true));
		return MathHelper.Clamp(MathF.Min(num, fadeOut), 0f, 1f);
	}

	private float GetGreatswordBurstChargePower(MoDaoQianRenPlayer bladePlayer)
	{
		int maxCharge = Math.Max(1, bladePlayer.GetCurrentMaxShardCharge());
		return SmoothStep(MathHelper.Clamp(GreatswordBurstChargeAmount / (float)maxCharge, 0f, 1f));
	}

	private float GetCurrentAttackPower(float progress)
	{
		if (IsGreatswordBurstCharge && base.Projectile.owner >= 0 && base.Projectile.owner < 255)
		{
			return MathHelper.Clamp(GetGreatswordBurstChargePower(Main.player[base.Projectile.owner].GetModPlayer<MoDaoQianRenPlayer>()) * 1.2f, 0f, 1f);
		}
		if (IsGreatswordBurst)
		{
			return MathHelper.Clamp(GetGreatswordBurstPower(progress) * 1.35f, 0f, 1f);
		}
		if (IsGreatswordSwing)
		{
			return MathHelper.Clamp(GetGreatswordSlashPower(progress) * (IsGreatswordHeavySlash ? 1.45f : 1.2f), 0f, 1f);
		}
		if (IsGreatswordDevilsHoldout)
		{
			if (!greatswordDevilsSwinging || greatswordDevilsSwooshFade)
			{
				return MathHelper.Lerp(0.04f, 0.18f, MathHelper.Clamp(greatswordDevilsPostSwingCooldown / (float)Math.Max(1, GetGreatswordDevilsPostSwingCooldownMax()), 0f, 1f));
			}
			float swingProgress = GetGreatswordDevilsSwingProgress(greatswordDevilsSwingTimer);
			return MathHelper.Clamp(MathF.Sin(MathHelper.Clamp(swingProgress, 0f, 1f) * MathHelper.Pi) * 1.2f, 0f, 1f);
		}
		if (IsHeavySlash)
		{
			return MathHelper.Clamp(GetSlashPower(progress) * 1.35f, 0f, 1f);
		}
		if (!IsSpinAttack)
		{
			return GetSlashPower(progress);
		}
		return GetSpinPower(progress);
	}

	private static float SmoothStep(float value)
	{
		value = MathHelper.Clamp(value, 0f, 1f);
		return value * value * (3f - 2f * value);
	}

	private Vector2 GetAttackDirectionAtProgress(float progress)
	{
		if (IsShardStream || IsGreatswordBurstCharge || IsGreatswordComboDash)
		{
			return base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
		}
		if (IsGreatswordDevilsHoldout)
		{
			return GetGreatswordDevilsDirection();
		}
		float facingSign = ((MathF.Cos(BaseRotation) >= 0f) ? 1f : (-1f));
		if (IsSpinAttack)
		{
			float spinProgress = GetSpinProgress(progress);
			float spinOffset = (float)Math.PI * -39f / 50f + (float)Math.PI * 2f * spinProgress;
			return (BaseRotation + spinOffset * facingSign).ToRotationVector2();
		}
		if (IsGreatswordBurst)
		{
			return BaseRotation.ToRotationVector2().SafeNormalize(Vector2.UnitX * base.Projectile.direction);
		}
		float swingProgress = (IsGreatswordSwing ? GetGreatswordSwingProgress(progress) : GetHeavySlashProgress(progress));
		if (IsHeavySlash)
		{
			float halfArc = (IsGreatswordSwing ? 3.18f : 3.95f) * 0.5f;
			float heavySwingOffset = MathHelper.Lerp(0f - halfArc, halfArc, swingProgress) * facingSign;
			return (BaseRotation + heavySwingOffset).ToRotationVector2();
		}
		float arc = (IsGreatswordSwing ? 2.36f : 2.82f);
		float value = (IsReverseSwing ? (arc * 0.58f) : ((0f - arc) * 0.54f));
		float swingEnd = (IsReverseSwing ? ((0f - arc) * 0.58f) : (arc * 0.54f));
		float swingOffset = MathHelper.Lerp(value, swingEnd, swingProgress) * facingSign;
		return (BaseRotation + swingOffset).ToRotationVector2();
	}

	private bool IsGreatswordAimTrackingWindow(float progress)
	{
		if (!IsGreatswordSwing)
		{
			return false;
		}
		return progress <= (IsGreatswordHeavySlash ? 0.5f : 0.44f);
	}

	private float GetGreatswordAimFollowStrength(float progress)
	{
		if (progress < 0.18f)
		{
			return 1f;
		}
		if (progress < 0.32f)
		{
			return 0.72f;
		}
		float releaseEnd = IsGreatswordHeavySlash ? 0.5f : 0.44f;
		float fade = 1f - SmoothStep(Utils.GetLerpValue(0.32f, releaseEnd, progress, clamped: true));
		return MathHelper.Lerp(0.18f, 0.42f, fade);
	}

	private void GetBladeLineAtProgress(Player player, float progress, out Vector2 start, out Vector2 end)
	{
		Vector2 direction = GetAttackDirectionAtProgress(progress);
		Vector2 center = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true) + GetHeldCenterOffsetAtProgress(player, progress, direction);
		start = center + direction * (IsModeOneBladeSwing ? 18f : 32f);
		end = center + direction * GetVisualBladeLength();
	}

	private int GetGreatswordDevilsUseAnimation(Player player)
	{
		return Math.Max(GreatswordDevilsMinimumUseAnimation, player.HeldItem.useAnimation);
	}

	private int GetGreatswordDevilsPostSwingCooldownMax()
	{
		return Math.Max(1, (int)(greatswordDevilsUseAnim * GreatswordDevilsPostSwingCooldownScale));
	}

	private float GetGreatswordDevilsSwingProgress(float frame)
	{
		if (greatswordDevilsUseAnim <= 0)
		{
			return 1f;
		}
		float windupFrames = greatswordDevilsUseAnim / 3f;
		float swingFrames = MathF.Max(1f, greatswordDevilsUseAnim - windupFrames);
		return MathHelper.Clamp((frame - windupFrames) / swingFrames, 0f, 1f);
	}

	private float GetGreatswordDevilsHoldSettle(float frame)
	{
		if (greatswordDevilsUseAnim <= 0)
		{
			return 0f;
		}
		return Utils.GetLerpValue(greatswordDevilsUseAnim * GreatswordDevilsSwingAnimationScale, greatswordDevilsUseAnim, frame, clamped: true);
	}

	private static float EaseGreatswordDevilsSwing(float progress)
	{
		progress = MathHelper.Clamp(progress * 0.9f, 0f, 1f);
		if (progress == 0f || progress == 1f)
		{
			return progress;
		}
		if (progress < 0.5f)
		{
			return MathF.Pow(2f, 20f * progress - 10f) / 2f;
		}
		return (2f - MathF.Pow(2f, -20f * progress - 10f)) / 2f;
	}

	private float GetGreatswordDevilsTargetRotationOffset(float frame)
	{
		if (!greatswordDevilsSwinging)
		{
			float settle = GetGreatswordDevilsHoldSettle(frame);
			return MathHelper.ToRadians(GreatswordDevilsHoldAngle * greatswordDevilsSwingSide * base.Projectile.direction * (1f + settle * 0.35f));
		}
		float progress = GetGreatswordDevilsSwingProgress(frame);
		float from = GreatswordDevilsSwingStartAngle * greatswordDevilsSwingSide * base.Projectile.direction;
		float to = GreatswordDevilsSwingEndAngle * -greatswordDevilsSwingSide * base.Projectile.direction;
		return MathHelper.ToRadians(MathHelper.Lerp(from, to, EaseGreatswordDevilsSwing(progress)));
	}

	private Vector2 GetGreatswordDevilsDirection()
	{
		return (BaseRotation + greatswordDevilsRotationOffset).ToRotationVector2();
	}

	private void UpdateGreatswordDevilsDirection()
	{
		greatswordDevilsRotationOffset = MathHelper.Lerp(greatswordDevilsRotationOffset, GetGreatswordDevilsTargetRotationOffset(greatswordDevilsSwingTimer), 0.2f);
		base.Projectile.velocity = GetGreatswordDevilsDirection();
	}

	private Vector2 GetGreatswordDevilsHeldOffsetAtFrame(float frame, Vector2 direction)
	{
		Vector2 baseDirection = BaseRotation.ToRotationVector2().SafeNormalize(direction);
		float distanceFactor = GetDistanceFactor(TargetBladeLength);
		if (!greatswordDevilsSwinging)
		{
			float settle = GetGreatswordDevilsHoldSettle(frame);
			float holdOffset = MathHelper.ToRadians(GreatswordDevilsHoldAngle * greatswordDevilsSwingSide * base.Projectile.direction * (1f + settle * 0.35f));
			return holdOffset.ToRotationVector2() * MathHelper.Lerp(4f, 10f, distanceFactor) * 0.18f;
		}
		return StabilizeGripOffset(baseDirection * MathHelper.Lerp(-10f, 18f, MathF.Sin(GetGreatswordDevilsSwingProgress(frame) * MathHelper.Pi)));
	}

	private void GetGreatswordDevilsBladeLine(Player player, out Vector2 start, out Vector2 end)
	{
		Vector2 direction = GetGreatswordDevilsDirection();
		Vector2 center = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true) + GetGreatswordDevilsHeldOffsetAtFrame(greatswordDevilsSwingTimer, direction);
		float rootDistance = GreatswordDevilsFogBladeRootDistance * MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f);
		start = center + direction * rootDistance;
		end = center + direction * MathF.Max(rootDistance + 36f, GetVisualBladeLength());
	}

	private Vector2 GetGreatswordSlashArcDirectionAtProgress(float progress, float lockedRotation)
	{
		float facingSign = ((MathF.Cos(lockedRotation) >= 0f) ? 1f : (-1f));
		float swingProgress = GetGreatswordSlashArcSwingProgress(progress);
		if (IsGreatswordHeavySlash)
		{
			float halfArc = 3.18f * 0.5f;
			float heavySwingOffset = MathHelper.Lerp(0f - halfArc, halfArc, swingProgress) * facingSign;
			return (lockedRotation + heavySwingOffset).ToRotationVector2();
		}
		float value = (IsReverseSwing ? (2.36f * 0.58f) : ((0f - 2.36f) * 0.54f));
		float swingEnd = (IsReverseSwing ? ((0f - 2.36f) * 0.58f) : (2.36f * 0.54f));
		float swingOffset = MathHelper.Lerp(value, swingEnd, swingProgress) * facingSign;
		return (lockedRotation + swingOffset).ToRotationVector2();
	}

	private Vector2 GetGreatswordSlashArcHeldOffsetAtProgress(float progress, Vector2 direction, float lockedRotation)
	{
		Vector2 normal = direction.RotatedBy(1.5707963705062866);
		float distanceFactor = GetDistanceFactor(TargetBladeLength);
		float greatswordSlashPower = GetGreatswordSlashArcPower(progress);
		float windupPull = ((progress < 0.34f) ? MathHelper.Lerp(-14f, IsGreatswordHeavySlash ? (-36f) : (-28f), SmoothStep(Utils.GetLerpValue(0f, 0.34f, progress, clamped: true))) : ((progress > 0.78f) ? MathHelper.Lerp(IsGreatswordHeavySlash ? 10f : 7f, -6f, SmoothStep(Utils.GetLerpValue(0.78f, 1f, progress, clamped: true))) : 0f));
		Vector2 centerLine = lockedRotation.ToRotationVector2();
		float forwardDrive = greatswordSlashPower * (IsGreatswordHeavySlash ? MathHelper.Lerp(8f, 18f, distanceFactor) : MathHelper.Lerp(6f, 14f, distanceFactor));
		float sideWeight = MathF.Sin(GetGreatswordSlashArcSwingProgress(progress) * (float)Math.PI) * (IsReverseSwing ? (-1f) : 1f) * (IsGreatswordHeavySlash ? MathHelper.Lerp(4f, 10f, distanceFactor) : MathHelper.Lerp(3f, 7f, distanceFactor));
		return StabilizeGripOffset(centerLine * (windupPull + forwardDrive) + normal * sideWeight);
	}

	private void GetGreatswordSlashArcBladeLineAtProgress(Player player, float progress, float lockedRotation, out Vector2 start, out Vector2 end)
	{
		Vector2 direction = GetGreatswordSlashArcDirectionAtProgress(progress, lockedRotation);
		Vector2 center = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true) + GetGreatswordSlashArcHeldOffsetAtProgress(progress, direction, lockedRotation);
		start = center + direction * 32f;
		end = center + direction * GetVisualBladeLength();
	}

	private void UpdateAimAndSwing(Player player, Vector2 handPosition)
	{
		if (IsShardStream)
		{
			if (base.Projectile.owner == Main.myPlayer)
			{
				Vector2 aim = Main.MouseWorld - handPosition;
				if (aim == Vector2.Zero || aim.HasNaNs())
				{
					aim = Vector2.UnitX * player.direction;
				}
				TargetBladeLength = MathHelper.Clamp(aim.Length(), 120f, 880f);
				base.Projectile.velocity = aim.SafeNormalize(Vector2.UnitX * player.direction);
				BaseRotation = base.Projectile.velocity.ToRotation();
				base.Projectile.netUpdate = true;
			}
			base.Projectile.velocity = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
		}
		else if (IsGreatswordBurstCharge)
		{
			if (base.Projectile.owner == Main.myPlayer)
			{
				Vector2 aim2 = Main.MouseWorld - handPosition;
				if (aim2 == Vector2.Zero || aim2.HasNaNs())
				{
					aim2 = ((base.Projectile.velocity == Vector2.Zero || base.Projectile.velocity.HasNaNs()) ? (Vector2.UnitX * player.direction) : base.Projectile.velocity);
				}
				float baseLength = ((player.HeldItem?.ModItem is MoDaoQianRen blade) ? blade.GetScaledStageBladeLength(player) : TargetBladeLength);
				TargetBladeLength = MathHelper.Clamp(baseLength, 120f, 880f);
				base.Projectile.velocity = aim2.SafeNormalize(Vector2.UnitX * player.direction);
				BaseRotation = base.Projectile.velocity.ToRotation();
				if (Timer % 6f == 0f)
				{
					base.Projectile.netUpdate = true;
				}
			}
			base.Projectile.velocity = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
		}
		else if (IsGreatswordComboDash)
		{
			if (IsGreatswordComboRingSlash)
			{
				base.Projectile.velocity = GetGreatswordComboRingSlashDirection();
			}
			else
			{
				base.Projectile.velocity = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
			}
		}
		else if (IsGreatswordDevilsHoldout)
		{
			base.Projectile.velocity = GetGreatswordDevilsDirection();
		}
		else
		{
			int duration = GetAttackDuration(player);
			float progress = Utils.GetLerpValue(0f, duration, Timer, clamped: true);
			if (IsGreatswordAimTrackingWindow(progress) && base.Projectile.owner == Main.myPlayer)
			{
				Vector2 aim3 = Main.MouseWorld - handPosition;
				if (aim3 == Vector2.Zero || aim3.HasNaNs())
				{
					aim3 = BaseRotation.ToRotationVector2();
				}
				Vector2 newDirection = aim3.SafeNormalize(Vector2.UnitX * player.direction);
				float previousRotation = BaseRotation;
				float targetRotation = newDirection.ToRotation();
				float followStrength = GetGreatswordAimFollowStrength(progress);
				BaseRotation = (followStrength >= 1f) ? targetRotation : (previousRotation + MathHelper.WrapAngle(targetRotation - previousRotation) * followStrength);
				if (Timer % 4f == 0f || MathF.Abs(MathHelper.WrapAngle(BaseRotation - previousRotation)) > 0.01f)
				{
					base.Projectile.netUpdate = true;
				}
			}
			base.Projectile.velocity = GetAttackDirectionAtProgress(progress);
		}
	}

	private void UpdatePlayerVisuals(Player player, Vector2 handPosition)
	{
		base.Projectile.Center = handPosition + GetHeldCenterOffset(player);
		base.Projectile.rotation = base.Projectile.velocity.ToRotation();
		int playerFacing;
		int spriteFacing = (playerFacing = ((!IsGreatswordComboRingSlash) ? ((!IsShardStream) ? ((MathF.Cos(BaseRotation) >= 0f) ? 1 : (-1)) : ((base.Projectile.velocity.X >= 0f) ? 1 : (-1))) : ((!(GreatswordComboRingSlashSpinDirection < 0f)) ? 1 : (-1))));
		if (IsGreatswordDevilsHoldout)
		{
			playerFacing = MathF.Cos(BaseRotation) >= 0f ? 1 : -1;
			spriteFacing = playerFacing;
		}
		if (IsGreatswordComboDash && !IsGreatswordComboRingSlash)
		{
			Vector2 flightDirection = -base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
			playerFacing = ((!(MathF.Abs(flightDirection.X) > 0.05f)) ? player.direction : ((flightDirection.X >= 0f) ? 1 : (-1)));
			spriteFacing = ((!(MathF.Abs(base.Projectile.velocity.X) > 0.05f)) ? (-playerFacing) : ((base.Projectile.velocity.X >= 0f) ? 1 : (-1)));
		}
		base.Projectile.direction = playerFacing;
		base.Projectile.spriteDirection = spriteFacing;
		player.ChangeDir(playerFacing);
		player.heldProj = base.Projectile.whoAmI;
		player.itemTime = 2;
		player.itemAnimation = 2;
		player.itemRotation = (base.Projectile.velocity * player.direction).ToRotation();
	}

	private void ProduceAttackRhythmEvents(Player player)
	{
		int duration = GetAttackDuration(player);
		float releaseProgress = (IsGreatswordBurst ? 0.16f : ((!IsGreatswordSwing) ? (IsHeavySlash ? 0.28f : (IsSpinAttack ? 0.22f : 0.2f)) : (IsGreatswordHeavySlash ? GreatswordHeavySlashReleaseProgress : GreatswordSlashReleaseProgress)));
		int releaseFrame = Math.Max(2, (int)MathF.Round((float)duration * releaseProgress));
		if (Timer == 1f && (IsHeavySlash || IsGreatswordBladeForm))
		{
			if (!TryPlayOverhaulSwordSwingSound(windup: true))
			{
				SoundStyle style = SoundID.Item1 with
				{
					Volume = (IsGreatswordBladeForm ? 0.34f : 0.24f),
					Pitch = (IsGreatswordBladeForm ? (-0.72f) : (-0.55f)),
					PitchVariance = 0.04f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
				SoundEngine.PlaySound(SoundID.Item15 with
				{
					Volume = (IsGreatswordBladeForm ? 0.25f : 0.18f),
					Pitch = (IsGreatswordBladeForm ? (-0.48f) : (-0.35f))
				}, base.Projectile.Center);
			}
			ProduceWindupDust();
		}
		if (Timer != (float)releaseFrame)
		{
			return;
		}
		if (IsGreatswordBurst)
		{
			if (!TryPlayOverhaulSwordSwingSound(windup: false))
			{
				SoundStyle style = SoundID.Item1 with
				{
					Volume = 1.12f,
					Pitch = -0.8f,
					PitchVariance = 0.05f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
				style = SoundID.Item15 with
				{
					Volume = 0.66f,
					Pitch = -0.58f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
				style = SoundID.Item15 with
				{
					Volume = 0.44f,
					Pitch = -0.9f,
					PitchVariance = 0.03f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
			}
			TryAddGreatswordBurstScreenShake(player, base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction));
		}
		else if (IsGreatswordSwing)
		{
			if (!TryPlayOverhaulSwordSwingSound(windup: false))
			{
				SoundStyle style = SoundID.Item1 with
				{
					Volume = (IsGreatswordHeavySlash ? 1f : 0.94f),
					Pitch = (IsGreatswordHeavySlash ? (-0.72f) : (-0.62f)),
					PitchVariance = 0.05f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
				SoundEngine.PlaySound(SoundID.Item15 with
				{
					Volume = (IsGreatswordHeavySlash ? 0.55f : 0.42f),
					Pitch = (IsGreatswordHeavySlash ? (-0.48f) : (-0.36f))
				}, base.Projectile.Center);
				if (IsGreatswordHeavySlash)
				{
					style = SoundID.Item15 with
					{
						Volume = 0.36f,
						Pitch = -0.82f,
						PitchVariance = 0.03f
					};
					SoundEngine.PlaySound(in style, base.Projectile.Center);
				}
			}
			TryAddSwordScreenShake(player, base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction) * (IsGreatswordHeavySlash ? 2.35f : 1.85f));
		}
		else if (IsHeavySlash)
		{
			if (!TryPlayOverhaulSwordSwingSound(windup: false))
			{
				SoundStyle style = SoundID.Item1 with
				{
					Volume = 0.95f,
					Pitch = -0.48f,
					PitchVariance = 0.06f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
				style = SoundID.Item15 with
				{
					Volume = 0.38f,
					Pitch = -0.3f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
			}
			TryAddSwordScreenShake(player, base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction) * 1.55f);
		}
		else if (IsSpinAttack)
		{
			if (!TryPlayOverhaulSwordSwingSound(windup: false))
			{
				SoundStyle style = SoundID.Item1 with
				{
					Volume = 0.86f,
					Pitch = -0.32f,
					PitchVariance = 0.08f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
				style = SoundID.Item15 with
				{
					Volume = 0.24f,
					Pitch = -0.05f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
			}
			TryAddSwordScreenShake(player, base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction) * 1.25f);
		}
		else
		{
			if (!TryPlayOverhaulSwordSwingSound(windup: false))
			{
				SoundStyle style = SoundID.Item1 with
				{
					Volume = 0.8f,
					PitchVariance = 0.1f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
				style = SoundID.Item15 with
				{
					Volume = 0.18f,
					Pitch = 0.08f,
					PitchVariance = 0.06f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
			}
			TryAddSwordScreenShake(player, base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction));
		}
		if (IsModeOneBladeSwing)
		{
			ProduceModeOneReleaseBurst(player);
		}
		if (base.Projectile.owner == Main.myPlayer)
		{
			Vector2 impulse = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction) * (IsGreatswordBurst ? 2.2f : (IsGreatswordHeavySlash ? 1.85f : (IsGreatswordSwing ? 1.35f : (IsHeavySlash ? 2.05f : (IsSpinAttack ? 1.75f : 1.15f)))));
			if (player.velocity.Y == 0f && impulse.Y < 0f)
			{
				impulse.Y *= 0.18f;
			}
			player.velocity += impulse;
			player.velocity.X = MathHelper.Clamp(player.velocity.X, -9.5f, 9.5f);
			player.velocity.Y = MathHelper.Clamp(player.velocity.Y, -8f, 9.5f);
		}
		ProduceReleaseDust(IsGreatswordBurst ? 3.35f : (IsGreatswordHeavySlash ? 2.05f : (IsGreatswordSwing ? 1.55f : (IsHeavySlash ? 1.42f : (IsSpinAttack ? 1.15f : 0.85f)))));
	}

	private void InitializeGreatswordDevilsHoldout(Player player, Vector2 handPosition)
	{
		if (greatswordDevilsInitialized)
		{
			return;
		}
		greatswordDevilsUseAnim = Math.Max(1, GetGreatswordDevilsUseAnimation(player));
		greatswordDevilsSwingTimer = 0;
		greatswordDevilsPostSwingCooldown = GetGreatswordDevilsPostSwingCooldownMax() / 2;
		greatswordDevilsSwingSide = -1;
		greatswordDevilsSwingCount = 0;
		greatswordDevilsSwinging = false;
		greatswordDevilsCanHit = false;
		greatswordDevilsSwooshFade = false;
		greatswordDevilsPlaySwingSound = true;
		greatswordDevilsRotationOffset = 0f;
		greatswordDevilsHasPreviousBladeLine = false;
		Vector2 aim = (base.Projectile.owner == Main.myPlayer ? Main.MouseWorld : handPosition + base.Projectile.velocity) - handPosition;
		if (aim == Vector2.Zero || aim.HasNaNs())
		{
			aim = Vector2.UnitX * player.direction;
		}
		Vector2 direction = aim.SafeNormalize(Vector2.UnitX * player.direction);
		BaseRotation = direction.ToRotation();
		base.Projectile.velocity = direction;
		base.Projectile.direction = direction.X < 0f ? -1 : 1;
		base.Projectile.spriteDirection = base.Projectile.direction;
		player.ChangeDir(base.Projectile.direction);
		greatswordDevilsInitialized = true;
		greatswordDevilsRotationOffset = GetGreatswordDevilsTargetRotationOffset(greatswordDevilsSwingTimer);
		base.Projectile.velocity = GetGreatswordDevilsDirection();
	}

	private void UpdateGreatswordDevilsHoldout(Player player, Vector2 handPosition)
	{
		InitializeGreatswordDevilsHoldout(player, handPosition);
		if (base.Projectile.owner == Main.myPlayer && (player.noItems || player.CCed || !(player.HeldItem?.ModItem is MoDaoQianRen) || !player.GetModPlayer<MoDaoQianRenPlayer>().IsGreatswordDevilsMode))
		{
			base.Projectile.Kill();
			return;
		}
		if (base.Projectile.owner == Main.myPlayer && !Main.mouseRight && !greatswordDevilsSwinging)
		{
			base.Projectile.Kill();
			return;
		}
		if (greatswordDevilsPostSwingCooldown > 0)
		{
			greatswordDevilsPostSwingCooldown--;
		}
		if (base.Projectile.owner == Main.myPlayer && !greatswordDevilsSwinging)
		{
			Vector2 aim = Main.MouseWorld - handPosition;
			if (aim == Vector2.Zero || aim.HasNaNs())
			{
				aim = BaseRotation.ToRotationVector2();
			}
			Vector2 direction = aim.SafeNormalize(Vector2.UnitX * player.direction);
			BaseRotation = direction.ToRotation();
			base.Projectile.direction = direction.X < 0f ? -1 : 1;
			base.Projectile.spriteDirection = base.Projectile.direction;
			player.ChangeDir(base.Projectile.direction);
			if (Timer % 6f == 0f)
			{
				base.Projectile.netUpdate = true;
			}
		}
		bool preservePreviousBladeLine = greatswordDevilsSwinging || greatswordDevilsCanHit;
		if (preservePreviousBladeLine)
		{
			GetGreatswordDevilsBladeLine(player, out greatswordDevilsPreviousBladeStart, out greatswordDevilsPreviousBladeEnd);
			greatswordDevilsHasPreviousBladeLine = true;
		}
		else
		{
			greatswordDevilsHasPreviousBladeLine = false;
		}
		if (!greatswordDevilsSwinging)
		{
			greatswordDevilsCanHit = false;
			greatswordDevilsSwooshFade = false;
			bool startedSwing = false;
			if (base.Projectile.owner == Main.myPlayer && Main.mouseRight && greatswordDevilsPostSwingCooldown <= 0)
			{
				StartGreatswordDevilsSwing(player, handPosition);
				startedSwing = true;
			}
			if (!startedSwing)
			{
				UpdateGreatswordDevilsDirection();
			}
			return;
		}
		greatswordDevilsSwingTimer++;
		UpdateGreatswordDevilsDirection();
		float time = greatswordDevilsSwingTimer - greatswordDevilsUseAnim / 3f;
		float timeMax = MathF.Max(1f, greatswordDevilsUseAnim - greatswordDevilsUseAnim / 3f);
		if (time >= timeMax * 0.4f && greatswordDevilsPlaySwingSound)
		{
			PlayGreatswordDevilsSwingSound();
			greatswordDevilsPlaySwingSound = false;
			TryAddSwordScreenShake(player, base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction) * 1.85f);
			if (base.Projectile.owner == Main.myPlayer)
			{
				Vector2 impulse = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction) * 1.35f;
				if (player.velocity.Y == 0f && impulse.Y < 0f)
				{
					impulse.Y *= 0.18f;
				}
				player.velocity += impulse;
				player.velocity.X = MathHelper.Clamp(player.velocity.X, -9.5f, 9.5f);
				player.velocity.Y = MathHelper.Clamp(player.velocity.Y, -8f, 9.5f);
			}
			ProduceReleaseDust(1.55f);
		}
		greatswordDevilsCanHit = time > timeMax * 0.2f && time < timeMax * 0.9f;
		greatswordDevilsSwooshFade = time > timeMax * 0.7f;
		if (time >= timeMax * 0.9f)
		{
			FinishGreatswordDevilsSwing(player, handPosition);
		}
	}

	private void StartGreatswordDevilsSwing(Player player, Vector2 handPosition)
	{
		Vector2 aim = Main.MouseWorld - handPosition;
		if (aim == Vector2.Zero || aim.HasNaNs())
		{
			aim = BaseRotation.ToRotationVector2();
		}
		Vector2 direction = aim.SafeNormalize(Vector2.UnitX * player.direction);
		BaseRotation = direction.ToRotation();
		base.Projectile.velocity = direction;
		base.Projectile.direction = direction.X < 0f ? -1 : 1;
		base.Projectile.spriteDirection = base.Projectile.direction;
		player.ChangeDir(base.Projectile.direction);
		greatswordDevilsUseAnim = Math.Max(1, GetGreatswordDevilsUseAnimation(player));
		greatswordDevilsSwingTimer = greatswordDevilsUseAnim / 3;
		greatswordDevilsSwinging = true;
		greatswordDevilsCanHit = false;
		greatswordDevilsSwooshFade = false;
		greatswordDevilsPlaySwingSound = true;
		greatswordDevilsHasPreviousBladeLine = false;
		UpdateGreatswordDevilsDirection();
		base.Projectile.localNPCImmunity = new int[Main.maxNPCs];
		base.Projectile.numHits = 0;
		base.Projectile.netUpdate = true;
	}

	private void FinishGreatswordDevilsSwing(Player player, Vector2 handPosition)
	{
		greatswordDevilsSwinging = false;
		greatswordDevilsCanHit = false;
		greatswordDevilsSwooshFade = false;
		greatswordDevilsSwingSide = -greatswordDevilsSwingSide;
		greatswordDevilsSwingCount++;
		greatswordDevilsPostSwingCooldown = GetGreatswordDevilsPostSwingCooldownMax();
		greatswordDevilsHasPreviousBladeLine = false;
		base.Projectile.netUpdate = true;
		if (base.Projectile.owner == Main.myPlayer && !Main.mouseRight)
		{
			base.Projectile.Kill();
			return;
		}
		Vector2 aim = Main.MouseWorld - handPosition;
		if (aim != Vector2.Zero && !aim.HasNaNs())
		{
			Vector2 direction = aim.SafeNormalize(Vector2.UnitX * player.direction);
			BaseRotation = direction.ToRotation();
			base.Projectile.direction = direction.X < 0f ? -1 : 1;
			base.Projectile.spriteDirection = base.Projectile.direction;
			player.ChangeDir(base.Projectile.direction);
			UpdateGreatswordDevilsDirection();
		}
	}

	private void PlayGreatswordDevilsSwingSound()
	{
		if (!TryPlayOverhaulSwordSwingSound(windup: false))
		{
			SoundStyle style = SoundID.Item1 with
			{
				Volume = 0.94f,
				Pitch = -0.62f,
				PitchVariance = 0.05f
			};
			SoundEngine.PlaySound(in style, base.Projectile.Center);
			SoundEngine.PlaySound(SoundID.Item15 with
			{
				Volume = 0.42f,
				Pitch = -0.36f
			}, base.Projectile.Center);
		}
	}

	private bool TryPlayOverhaulSwordSwingSound(bool windup)
	{
		if (Main.dedServ || !ModLoader.HasMod("TerrariaOverhaul"))
		{
			return false;
		}
		bool heavy = IsGreatswordBladeForm || IsHeavySlash || IsSpinAttack;
		SoundStyle obj = ((windup || !heavy) ? OverhaulSwordMediumSwing : OverhaulSwordHeavySwing);
		float volume = ((!windup) ? (IsGreatswordDevilsHoldout ? 1.08f : (IsGreatswordBurst ? 1.14f : ((!IsGreatswordSwing) ? (IsHeavySlash ? 0.88f : (IsSpinAttack ? 0.78f : 0.68f)) : (IsGreatswordHeavySlash ? 1.02f : 0.92f)))) : (IsGreatswordBladeForm ? 0.36f : 0.28f));
		float pitch = ((!windup) ? (IsGreatswordDevilsHoldout ? -0.26f : (IsGreatswordBurst ? (-0.24f) : (IsGreatswordHeavySlash ? (-0.18f) : (IsGreatswordSwing ? (-0.08f) : (IsHeavySlash ? 0f : (IsSpinAttack ? 0.08f : 0.12f)))))) : (IsGreatswordBladeForm ? (-0.28f) : (-0.16f)));
		SoundEngine.PlaySound(obj with
		{
			Volume = volume,
			Pitch = pitch,
			PitchVariance = 0.12f
		}, base.Projectile.Center);
		if (!windup && (IsGreatswordHeavySlash || IsGreatswordBurst || IsGreatswordDevilsHoldout))
		{
			SoundStyle style = OverhaulSwordKillingBlow with
			{
				Volume = (IsGreatswordDevilsHoldout ? 0.58f : (IsGreatswordBurst ? 0.68f : 0.5f)),
				Pitch = (IsGreatswordDevilsHoldout ? -0.24f : (IsGreatswordBurst ? (-0.26f) : (-0.18f))),
				PitchVariance = 0.08f
			};
			SoundEngine.PlaySound(in style, base.Projectile.Center);
		}
		return true;
	}

	private bool TryPlayOverhaulGreatswordHeavyImpactSound()
	{
		if (Main.dedServ || !ModLoader.HasMod("TerrariaOverhaul"))
		{
			return false;
		}
		SoundEngine.PlaySound(OverhaulSwordHeavySwing with
		{
			Volume = 1.04f,
			Pitch = -0.2f,
			PitchVariance = 0.1f
		}, base.Projectile.Center);
		SoundEngine.PlaySound(OverhaulSwordKillingBlow with
		{
			Volume = 0.52f,
			Pitch = -0.18f,
			PitchVariance = 0.08f
		}, base.Projectile.Center);
		return true;
	}

	private void TryAddSwordScreenShake(Player player, Vector2 direction)
	{
		if (!Main.dedServ && base.Projectile.owner == Main.myPlayer)
		{
			float strength = MathHelper.Clamp(direction.Length(), 0.3f, 2.6f);
			Main.instance.CameraModifiers.Add(new PunchCameraModifier(player.Center, direction.SafeNormalize(Vector2.UnitX * player.direction), 0.3f * strength, 5.5f + strength, (int)MathHelper.Lerp(8f, 13f, Utils.GetLerpValue(0.3f, 2.6f, strength, clamped: true)), 1000f));
		}
	}

	private void TryAddGreatswordBurstScreenShake(Player player, Vector2 direction)
	{
		if (!Main.dedServ && base.Projectile.owner == Main.myPlayer)
		{
			Vector2 shakeDirection = direction.SafeNormalize(Vector2.UnitX * player.direction);
			Main.instance.CameraModifiers.Add(new PunchCameraModifier(player.Center + shakeDirection * 160f, shakeDirection, 1.18f, 14f, 20, 1600f));
			Main.instance.CameraModifiers.Add(new PunchCameraModifier(player.Center, -shakeDirection, 0.46f, 9f, 12, 1200f));
		}
	}

	private void TryAddGreatswordBurstChargeScreenShake(Player player)
	{
		if (!Main.dedServ && base.Projectile.owner == Main.myPlayer && Timer % 12f == 0f)
		{
			float chargePower = GetGreatswordBurstChargePower(player.GetModPlayer<MoDaoQianRenPlayer>());
			if (!(chargePower <= 0.08f))
			{
				Vector2 shakeDirection = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
				Main.instance.CameraModifiers.Add(new PunchCameraModifier(player.Center, shakeDirection.RotatedBy((float)Math.PI / 2f * ((Timer % 24f == 0f) ? 1f : (-1f))), MathHelper.Lerp(0.08f, 0.34f, chargePower), MathHelper.Lerp(3f, 7f, chargePower), 6, 700f));
			}
		}
	}

	private bool IsGreatswordComboDashActive()
	{
		if (Timer >= 8f && Timer <= 34f)
		{
			return !GreatswordComboDashHasHit;
		}
		return false;
	}

	private float GetGreatswordComboDashExtensionProgress()
	{
		return GetGreatswordComboDashExtensionProgress(Timer);
	}

	private float GetGreatswordComboDashExtensionProgress(float timer)
	{
		float rawProgress = Utils.GetLerpValue(8f, 24f, timer, clamped: true);
		return MathHelper.Clamp(1f - MathF.Pow(1f - rawProgress, 2.35f), 0f, 1f);
	}

	private float GetGreatswordComboDashDesiredLength(float timer)
	{
		float maximumLength = MathF.Max(42f, TargetBladeLength);
		float extensionProgress = GetGreatswordComboDashExtensionProgress(timer);
		float value = MathHelper.Lerp(42f, maximumLength, extensionProgress);
		float recoveryEnd = 46f;
		float recoveryProgress = SmoothStep(Utils.GetLerpValue(30f, recoveryEnd, timer, clamped: true));
		return MathHelper.Lerp(value, 42f, recoveryProgress);
	}

	private float GetGreatswordComboDashVisibleLength()
	{
		float desiredLength = GetGreatswordComboDashDesiredLength(Timer);
		return GetGreatswordComboDashVisibleLength(desiredLength);
	}

	private float GetGreatswordComboDashVisibleLength(float desiredLength)
	{
		Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitY);
		float safeDesiredLength = MathF.Max(28f, desiredLength);
		return MathHelper.Clamp(GetGreatswordComboDashTileLimitedLength(base.Projectile.Center, direction, safeDesiredLength), 28f, safeDesiredLength);
	}

	private bool TryGetGreatswordComboDashCollisionLine(Vector2 direction, out Vector2 start, out Vector2 end, out float width)
	{
		direction = direction.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
		float extensionLength = GetGreatswordComboDashVisibleLength();
		if (extensionLength <= 36f)
		{
			start = base.Projectile.Center;
			end = base.Projectile.Center;
			width = 0f;
			return false;
		}
		start = base.Projectile.Center + direction * MathF.Min(42f, extensionLength * 0.42f);
		end = base.Projectile.Center + direction * extensionLength;
		width = MathHelper.Lerp(34f, 58f, GetDistanceFactor(extensionLength)) * MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f);
		return true;
	}

	private static float GetGreatswordComboDashTileLimitedLength(Vector2 origin, Vector2 direction, float desiredLength)
	{
		direction = direction.SafeNormalize(Vector2.UnitY);
		Vector2 normal = direction.RotatedBy(1.5707963705062866);
		float probeRadius = 7f;
		float safeLength = MathF.Min(42f, desiredLength);
		for (float distance = 42f; distance <= desiredLength; distance += 6f)
		{
			if (IsGreatswordComboDashBlockedAt(origin + direction * distance, normal, probeRadius))
			{
				return MathF.Max(28f, distance - 6f - probeRadius);
			}
			safeLength = distance;
		}
		return MathF.Max(safeLength, desiredLength);
	}

	public static bool HasGreatswordComboDashBraceTile(Vector2 origin, Vector2 direction, float desiredLength)
	{
		direction = direction.SafeNormalize(Vector2.UnitY);
		float probeLength = MathF.Min(MathF.Max(42f, desiredLength), MathF.Max(42f, 180f));
		for (float distance = 42f; distance <= probeLength; distance += 6f)
		{
			if (IsGreatswordComboDashBlockedAt(origin + direction * distance, direction.RotatedBy(1.5707963705062866), 7f))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsGreatswordComboDashBlockedAt(Vector2 center, Vector2 normal, float radius)
	{
		Vector2 halfSize = new Vector2(radius);
		int size = Math.Max(2, (int)MathF.Ceiling(radius * 2f));
		if (!Collision.SolidCollision(center - halfSize, size, size, acceptTopSurfaces: true) && !Collision.SolidCollision(center + normal * radius - halfSize, size, size, acceptTopSurfaces: true))
		{
			return Collision.SolidCollision(center - normal * radius - halfSize, size, size, acceptTopSurfaces: true);
		}
		return true;
	}

	private float GetGreatswordComboRingSlashProgress()
	{
		int growthStage = ((base.Projectile.owner >= 0 && base.Projectile.owner < 255) ? GetCurrentGrowthStage(Main.player[base.Projectile.owner]) : 0);
		return SmoothStep(Utils.GetLerpValue(0f, MoDaoQianRen.GetRuntimeStats(growthStage).GreatswordComboRingSlashFrames, GreatswordComboRingSlashTimer, clamped: true));
	}

	private float GetGreatswordComboRingSlashRadius()
	{
		return MathHelper.Clamp(TargetBladeLength, 880f, 1364f);
	}

	private Vector2 GetGreatswordComboRingSlashDirection()
	{
		float progress = GetGreatswordComboRingSlashProgress();
		return (GreatswordComboRingSlashRotation + (float)Math.PI * 2f * progress * GreatswordComboRingSlashSpinDirection).ToRotationVector2().SafeNormalize(Vector2.UnitX * base.Projectile.direction);
	}

	private static bool IsAngleInsideGreatswordComboRingSweep(float targetRotation, float startRotation, float sweptAngle, float tolerance, float spinDirection)
	{
		float delta = MathHelper.WrapAngle((targetRotation - startRotation) * (float)MathF.Sign(spinDirection));
		if (delta < 0f)
		{
			delta += (float)Math.PI * 2f;
		}
		return delta <= sweptAngle + tolerance;
	}

	private void TryResolveGreatswordComboDashNpcImpact(Player player, Vector2 direction)
	{
		if (base.Projectile.owner != Main.myPlayer || GreatswordComboDashHasHit || !TryGetGreatswordComboDashCollisionLine(direction, out var start, out var end, out var width))
		{
			return;
		}
		NPC impactTarget = null;
		float closestDistance = float.MaxValue;
		Vector2 lineDirection = (end - start).SafeNormalize(direction);
		for (int i = 0; i < Main.maxNPCs; i++)
		{
			NPC npc = Main.npc[i];
			if (!npc.CanBeChasedBy(base.Projectile))
			{
				continue;
			}
			float collisionPoint = 0f;
			if (Collision.CheckAABBvLineCollision(npc.Hitbox.TopLeft(), npc.Hitbox.Size(), start, end, width, ref collisionPoint))
			{
				float hitDistance = Vector2.Dot(npc.Center - start, lineDirection);
				if (hitDistance < closestDistance)
				{
					closestDistance = hitDistance;
					impactTarget = npc;
				}
			}
		}
		if (impactTarget != null)
		{
			GreatswordComboDashHasHit = true;
			base.Projectile.velocity *= 0.18f;
			player.velocity *= 0.18f;
			ProduceGreatswordComboDashNpcImpactEffects(player, impactTarget, direction);
			TryAddSwordScreenShake(player, (impactTarget.Center - player.Center).SafeNormalize(direction) * 1.65f);
			base.Projectile.netUpdate = true;
		}
	}

	private void ProduceGreatswordComboDashNpcImpactEffects(Player player, NPC target, Vector2 direction)
	{
		if (!Main.dedServ)
		{
			direction = direction.SafeNormalize(Vector2.UnitX * player.direction);
			Vector2 normal = direction.RotatedBy(1.5707963705062866);
			SoundStyle style = SoundID.Item10 with
			{
				Volume = 0.42f,
				Pitch = -0.5f
			};
			SoundEngine.PlaySound(in style, target.Center);
			for (int i = 0; i < 22; i++)
			{
				Vector2 dustDirection = (direction * Main.rand.NextFloat(0.15f, 1f) + normal * Main.rand.NextFloat(-1.45f, 1.45f)).SafeNormalize(direction);
				Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular((float)target.width * 0.36f, (float)target.height * 0.36f), Main.rand.NextBool(3) ? 27 : 62, dustDirection * Main.rand.NextFloat(2.6f, 7.8f), 60, new Color(225, 170, 255), Main.rand.NextFloat(1.05f, 1.85f)).noGravity = true;
			}
			MoDaoQianRenWarmupSystem.AddLight(target.Center, 0.8f, 0.12f, 1.2f);
		}
	}

	private void UpdateGreatswordComboDash(Player player, Vector2 handPosition)
	{
		if (IsGreatswordComboRingSlash)
		{
			UpdateGreatswordComboRingSlash(player);
			return;
		}
		if (Timer == 1f)
		{
			if (!TryPlayOverhaulSwordSwingSound(windup: true))
			{
				SoundStyle style = SoundID.Item1 with
				{
					Volume = 0.42f,
					Pitch = -0.52f,
					PitchVariance = 0.06f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
			}
			ProduceWindupDust();
		}
		Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
		Vector2 pushDirection = -direction;
		BaseRotation = direction.ToRotation();
		TargetBladeLength = MathHelper.Clamp(TargetBladeLength, 120f, 1364f);
		int duration = GetAttackDuration(player);
		bool num = IsGreatswordComboDashActive();
		if (Timer == 8f)
		{
			if (!TryPlayOverhaulSwordSwingSound(windup: false))
			{
				SoundStyle style = SoundID.Item71 with
				{
					Volume = 0.62f,
					Pitch = -0.18f,
					PitchVariance = 0.08f
				};
				SoundEngine.PlaySound(in style, base.Projectile.Center);
			}
			TryAddSwordScreenShake(player, pushDirection * 2.35f);
		}
		if (num && base.Projectile.owner == Main.myPlayer)
		{
			player.immune = true;
			player.immuneNoBlink = true;
			player.immuneTime = Math.Max(player.immuneTime, 10);
			float dashFrame = MathF.Max(0f, Timer - 8f);
			float impulseProgress = SmoothStep(Utils.GetLerpValue(0f, 12f, dashFrame, clamped: true));
			float fadeProgress = SmoothStep(Utils.GetLerpValue(12f, 26f, dashFrame, clamped: true));
			float speed = ((dashFrame <= 12f) ? MathHelper.Lerp(32f, 24f, impulseProgress) : MathHelper.Lerp(24f, 12.48f, fadeProgress));
			player.velocity = pushDirection * speed;
			player.fallStart = (int)(player.position.Y / 16f);
			if (Timer % 5f == 0f)
			{
				base.Projectile.netUpdate = true;
			}
		}
		if (num)
		{
			TryResolveGreatswordComboDashNpcImpact(player, direction);
		}
		else if (GreatswordComboDashHasHit)
		{
			player.velocity *= 0.78f;
		}
		bool invalidState = player.noItems || player.CCed || !(player.HeldItem?.ModItem is MoDaoQianRen);
		if (base.Projectile.owner == Main.myPlayer && invalidState)
		{
			base.Projectile.Kill();
		}
		else if ((GreatswordComboDashHasHit && Timer >= base.Projectile.ai[2] + 8f) || Timer >= (float)duration)
		{
			if (base.Projectile.owner == Main.myPlayer && Main.mouseRight && player.HeldItem?.ModItem is MoDaoQianRen bladeForRing && player.GetModPlayer<MoDaoQianRenPlayer>().TryConsumeShardCharge(1))
			{
				BeginGreatswordComboRingSlash(player, bladeForRing);
			}
			else
			{
				base.Projectile.Kill();
			}
		}
	}

	private void BeginGreatswordComboRingSlash(Player player, MoDaoQianRen blade)
	{
		TargetBladeLength = MathHelper.Clamp(MathF.Max(blade.GetScaledStageBladeLength(player), 880f), 120f, 1364f);
		float ringStartRotation = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction).ToRotation();
		float ringStorageOffset = ((base.Projectile.direction < 0) ? 2000f : 1000f);
		GreatswordComboRingSlashStartRotation = ringStartRotation + ringStorageOffset;
		GreatswordComboRingSlashTimer = 1f;
		base.Projectile.ai[2] = -1f;
		base.Projectile.velocity = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
		base.Projectile.localNPCImmunity = new int[Main.maxNPCs];
		base.Projectile.localNPCHitCooldown = GetLocalNpcHitCooldown();
		base.Projectile.netUpdate = true;
		if (!TryPlayOverhaulGreatswordHeavyImpactSound())
		{
			SoundStyle style = SoundID.Item1 with
			{
				Volume = 1f,
				Pitch = -0.72f,
				PitchVariance = 0.06f
			};
			SoundEngine.PlaySound(in style, player.Center);
			style = SoundID.Item15 with
			{
				Volume = 0.64f,
				Pitch = -0.5f
			};
			SoundEngine.PlaySound(in style, player.Center);
			style = SoundID.Item71 with
			{
				Volume = 0.76f,
				Pitch = -0.34f,
				PitchVariance = 0.08f
			};
			SoundEngine.PlaySound(in style, player.Center);
		}
		TryAddSwordScreenShake(player, base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction) * 2.15f);
		ProduceGreatswordComboRingSlashBurst(player);
	}

	private void UpdateGreatswordComboRingSlash(Player player)
	{
		int growthStage = GetCurrentGrowthStage(player);
		MoDaoQianRenStageStats stats = MoDaoQianRen.GetRuntimeStats(growthStage);
		Vector2 direction = GetGreatswordComboRingSlashDirection();
		base.Projectile.velocity = direction;
		TargetBladeLength = MathHelper.Clamp(TargetBladeLength, 880f, 1364f);
		if (base.Projectile.owner == Main.myPlayer)
		{
			if (!Main.mouseRight || player.noItems || player.CCed || !(player.HeldItem?.ModItem is MoDaoQianRen))
			{
				base.Projectile.Kill();
				return;
			}
			if (GreatswordComboRingSlashTimer > 1f && GreatswordComboRingSlashTimer % (float)stats.GreatswordComboRingSlashConsumeInterval == 0f && !player.GetModPlayer<MoDaoQianRenPlayer>().TryConsumeShardCharge(1))
			{
				base.Projectile.Kill();
				return;
			}
			player.immune = true;
			player.immuneNoBlink = true;
			player.immuneTime = Math.Max(player.immuneTime, 8);
			player.velocity *= 0.84f;
			if (GreatswordComboRingSlashTimer < 8f && player.velocity.Y > -2f)
			{
				player.velocity.Y = MathHelper.Lerp(player.velocity.Y, -2f, 0.35f);
			}
			player.fallStart = (int)(player.position.Y / 16f);
		}
		if (GreatswordComboRingSlashTimer >= (float)stats.GreatswordComboRingSlashFrames)
		{
			base.Projectile.Kill();
			return;
		}
		GreatswordComboRingSlashTimer++;
		base.Projectile.netUpdate = Timer % 6f == 0f;
	}

	private void ProduceGreatswordComboDashEffects(Player player)
	{
		if (Main.dedServ)
		{
			return;
		}
		if (IsGreatswordComboRingSlash)
		{
			ProduceGreatswordComboRingSlashEffects(player);
			return;
		}
		Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
		Vector2 pushDirection = -direction;
		Vector2 normal = direction.RotatedBy(1.5707963705062866);
		float visibleLength = GetGreatswordComboDashVisibleLength();
		float distanceFactor = GetDistanceFactor(visibleLength);
		bool activeDash = IsGreatswordComboDashActive();
		int dustCount = (activeDash ? 12 : 3);
		for (int i = 0; i < dustCount; i++)
		{
			float alongMax = MathF.Max(30f, visibleLength * (0.45f + GetGreatswordComboDashExtensionProgress() * 0.55f));
			float along = Main.rand.NextFloat(18f, MathF.Min(visibleLength, alongMax));
			Dust obj = Dust.NewDustPerfect(base.Projectile.Center + direction * along + normal * Main.rand.NextFloat(-18f, 18f + distanceFactor * 28f) - direction * Main.rand.NextFloat(activeDash ? 0f : 2f, activeDash ? 8f : 12f), Velocity: -direction * Main.rand.NextFloat(activeDash ? 3.5f : 1.8f, activeDash ? 8.5f : 4.2f) + normal * Main.rand.NextFloat(-1.8f, 1.8f), Type: Main.rand.NextBool(3) ? 27 : 62, Alpha: 58, newColor: Color.Lerp(new Color(205, 72, 255), Color.White, activeDash ? 0.36f : 0.18f), Scale: Main.rand.NextFloat(0.92f, activeDash ? 1.9f : 1.25f));
			obj.noGravity = true;
			obj.fadeIn = Main.rand.NextFloat(0.12f, 0.45f);
		}
		if (activeDash)
		{
			MoDaoQianRenWarmupSystem.AddLight(player.Center + pushDirection * 48f, 0.75f, 0.08f, 1.35f);
			if (Timer % 2f == 0f)
			{
				TryAddSwordScreenShake(player, pushDirection * 1.25f);
			}
		}
	}

	private void ProduceGreatswordComboRingSlashBurst(Player player)
	{
		if (!Main.dedServ)
		{
			float radius = GetGreatswordComboRingSlashRadius();
			int dustCount = (int)MathHelper.Clamp(radius / 14f, 42f, 96f);
			for (int i = 0; i < dustCount; i++)
			{
				Vector2 radial = ((float)Math.PI * 2f * (float)i / (float)dustCount).ToRotationVector2();
				Dust dust = Dust.NewDustPerfect(player.Center + radial * Main.rand.NextFloat(radius * 0.22f, radius * 0.96f), Main.rand.NextBool(3) ? 27 : 62, radial * Main.rand.NextFloat(2.2f, 7.6f) + radial.RotatedBy(1.5707963705062866) * Main.rand.NextFloat(-3.2f, 3.2f), 50, new Color(222, 82, 255), Main.rand.NextFloat(1.15f, 2.3f));
				dust.noGravity = true;
				dust.fadeIn = Main.rand.NextFloat(0.14f, 0.48f);
			}
			MoDaoQianRenWarmupSystem.AddLight(player.Center, 1.2f, 0.18f, 1.8f);
		}
	}

	private void ProduceGreatswordComboRingSlashEffects(Player player)
	{
		if (!Main.dedServ)
		{
			float radius = GetGreatswordComboRingSlashRadius();
			float progress = GetGreatswordComboRingSlashProgress();
			float sweptAngle = (float)Math.PI * 2f * progress;
			int dustCount = 10;
			for (int i = 0; i < dustCount; i++)
			{
				float factor = Main.rand.NextFloat(MathF.Max(0f, progress - 0.2f), MathF.Max(0.02f, progress));
				Vector2 radial = (GreatswordComboRingSlashRotation + (float)Math.PI * 2f * factor * GreatswordComboRingSlashSpinDirection).ToRotationVector2();
				Vector2 tangent = radial.RotatedBy(GreatswordComboRingSlashSpinDirection * ((float)Math.PI / 2f));
				Dust dust = Dust.NewDustPerfect(player.Center + radial * Main.rand.NextFloat(radius * 0.86f, radius * 1.03f) + tangent * Main.rand.NextFloat(-18f, 18f), Main.rand.NextBool(4) ? 242 : 62, tangent * Main.rand.NextFloat(2.4f, 7.8f) - radial * Main.rand.NextFloat(0.6f, 2.1f), 48, Color.Lerp(new Color(215, 74, 255), Color.White, Main.rand.NextFloat(0.18f, 0.5f)), Main.rand.NextFloat(1.05f, 2.05f));
				dust.noGravity = true;
				dust.fadeIn = Main.rand.NextFloat(0.08f, 0.36f);
			}
			Vector2 front = (GreatswordComboRingSlashRotation + sweptAngle * GreatswordComboRingSlashSpinDirection).ToRotationVector2();
			MoDaoQianRenWarmupSystem.AddLight(player.Center + front * radius * 0.72f, 1.05f, 0.14f, 1.55f);
			if (GreatswordComboRingSlashTimer % 3f == 0f)
			{
				TryAddSwordScreenShake(player, front * 0.92f);
			}
		}
	}

	private void UpdateGreatswordBurstCharge(Player player, Vector2 handPosition)
	{
		if (base.Projectile.owner != Main.myPlayer)
		{
			return;
		}
		if (!player.active || player.dead || player.noItems || player.CCed || !(player.HeldItem?.ModItem is MoDaoQianRen blade) || !player.GetModPlayer<MoDaoQianRenPlayer>().IsGreatswordMode)
		{
			base.Projectile.Kill();
			return;
		}
		MoDaoQianRenPlayer bladePlayer = player.GetModPlayer<MoDaoQianRenPlayer>();
		MoDaoQianRenStageStats stats = MoDaoQianRen.GetRuntimeStats(blade.GrowthStage);
		int maxCharge = Math.Max(1, bladePlayer.GetCurrentMaxShardCharge());
		if (Timer == 1f)
		{
			PlayGreatswordBurstChargeStartSound();
			ProduceWindupDust();
		}
		if (Main.mouseMiddle)
		{
			if (Timer % (float)stats.GreatswordBurstChargeConsumeInterval == 0f && GreatswordBurstChargeAmount < (float)maxCharge && bladePlayer.TryConsumeShardCharge(1))
			{
				GreatswordBurstChargeAmount++;
				base.Projectile.netUpdate = true;
			}
			TryAddGreatswordBurstChargeScreenShake(player);
		}
		else
		{
			ReleaseGreatswordBurstCharge(player, blade, handPosition, bladePlayer);
		}
	}

	private void ReleaseGreatswordBurstCharge(Player player, MoDaoQianRen blade, Vector2 handPosition, MoDaoQianRenPlayer bladePlayer)
	{
		int consumedCharge = Utils.Clamp((int)MathF.Round(GreatswordBurstChargeAmount), 0, bladePlayer.GetCurrentMaxShardCharge());
		if (consumedCharge <= 0)
		{
			SoundStyle style = SoundID.MenuTick with
			{
				Volume = 0.36f,
				Pitch = -0.2f
			};
			SoundEngine.PlaySound(in style, base.Projectile.Center);
			base.Projectile.Kill();
			return;
		}
		Vector2 aim = Main.MouseWorld - handPosition;
		if (aim == Vector2.Zero || aim.HasNaNs())
		{
			aim = ((base.Projectile.velocity == Vector2.Zero || base.Projectile.velocity.HasNaNs()) ? (Vector2.UnitX * player.direction) : base.Projectile.velocity);
		}
		Vector2 direction = aim.SafeNormalize(Vector2.UnitX * player.direction);
		float chargePower = SmoothStep(MathHelper.Clamp((float)consumedCharge / (float)Math.Max(1, bladePlayer.GetCurrentMaxShardCharge()), 0f, 1f));
		MoDaoQianRenStageStats stats = MoDaoQianRen.GetRuntimeStats(blade.GrowthStage);
		float lengthMultiplier = MathHelper.Lerp(1.1f, stats.GreatswordBurstLengthMultiplier, chargePower);
		float targetLength = MathHelper.Clamp(MathF.Max(blade.GetScaledStageBladeLength(player) * lengthMultiplier, aim.Length()), 120f, stats.GreatswordBurstMaximumBladeLength);
		int hitCount = GetGreatswordBurstHitCountFromCharge(consumedCharge, blade.GrowthStage);
		if (hitCount <= 0)
		{
			base.Projectile.Kill();
			return;
		}
		int projectileIndex = Projectile.NewProjectile(base.Projectile.GetSource_FromThis(), handPosition, direction, base.Type, base.Projectile.damage, base.Projectile.knockBack, base.Projectile.owner, 8f, targetLength, hitCount);
		if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
		{
			Main.projectile[projectileIndex].originalDamage = ((base.Projectile.originalDamage > 0) ? base.Projectile.originalDamage : base.Projectile.damage);
		}
		ProduceGreatswordBurstChargeReleaseEffects(player, handPosition, direction, chargePower);
		base.Projectile.Kill();
	}

	private void PlayGreatswordBurstChargeStartSound()
	{
		if (!TryPlayOverhaulSwordSwingSound(windup: true))
		{
			SoundStyle style = SoundID.Item15 with
			{
				Volume = 0.42f,
				Pitch = -0.62f,
				PitchVariance = 0.06f
			};
			SoundEngine.PlaySound(in style, base.Projectile.Center);
			style = SoundID.Item1 with
			{
				Volume = 0.3f,
				Pitch = -0.78f,
				PitchVariance = 0.04f
			};
			SoundEngine.PlaySound(in style, base.Projectile.Center);
		}
	}

	private void ProduceGreatswordBurstChargeEffects(Player player, Vector2 handPosition)
	{
		if (!Main.dedServ)
		{
			MoDaoQianRenPlayer bladePlayer = player.GetModPlayer<MoDaoQianRenPlayer>();
			float chargePower = GetGreatswordBurstChargePower(bladePlayer);
			Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
			Vector2 normal = direction.RotatedBy(1.5707963705062866);
			float density = MathHelper.Lerp(0.35f, 1f, chargePower);
			int dustCount = Math.Max(2, (int)MathF.Round(MathHelper.Lerp(6f, 20f, density)));
			float visualLength = 120f * MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f);
			float gatherStart = 22f;
			float gatherEnd = MathHelper.Lerp(visualLength * 0.68f, visualLength * 0.94f, chargePower);
			Vector2 focus = handPosition + direction * MathHelper.Lerp(gatherStart, gatherEnd, 0.48f);
			for (int i = 0; i < dustCount; i++)
			{
				float targetAlongFactor = Main.rand.NextFloat(0.04f, 0.96f);
				float targetAlong = MathHelper.Lerp(gatherStart, gatherEnd, targetAlongFactor);
				Vector2 vector = handPosition + direction * targetAlong + normal * Main.rand.NextFloat(-6f, 6f) * MathHelper.Lerp(0.45f, 1f, chargePower);
				float spawnAlong = MathHelper.Clamp(targetAlong + Main.rand.NextFloat(-52f, 52f), gatherStart, gatherEnd);
				float side = Main.rand.NextFloat(-1f, 1f);
				Vector2 spawnPosition = handPosition + direction * spawnAlong + normal * side * Main.rand.NextFloat(MathHelper.Lerp(34f, 56f, chargePower), MathHelper.Lerp(72f, 124f, chargePower));
				Vector2 vector2 = (vector - spawnPosition).SafeNormalize(direction);
				Vector2 tangent = direction * Main.rand.NextFloat(-0.75f, 0.75f);
				Vector2 dustVelocity = vector2 * Main.rand.NextFloat(MathHelper.Lerp(3.8f, 6.4f, chargePower), MathHelper.Lerp(7.4f, 14.5f, chargePower)) + tangent + normal * Main.rand.NextFloat(-0.55f, 0.55f);
				Dust dust = Dust.NewDustPerfect(spawnPosition, Main.rand.NextBool(3) ? 27 : (Main.rand.NextBool() ? 242 : 62), dustVelocity, 62, Color.Lerp(new Color(192, 72, 255), Color.White, chargePower * 0.55f), Main.rand.NextFloat(MathHelper.Lerp(0.92f, 1.35f, chargePower), MathHelper.Lerp(1.42f, 2.45f, chargePower)));
				dust.noGravity = true;
				dust.fadeIn = Main.rand.NextFloat(0.38f, 0.9f);
			}
			if (Timer % 3f == 0f)
			{
				float sparkAlong = MathHelper.Lerp(gatherStart, gatherEnd, Main.rand.NextFloat(0.12f, 0.9f));
				Vector2 sparkTarget = handPosition + direction * sparkAlong + normal * Main.rand.NextFloat(-4f, 4f);
				Vector2 sparkPosition = sparkTarget + direction * Main.rand.NextFloat(-28f, 28f) + normal * Main.rand.NextFloat(MathHelper.Lerp(-62f, -104f, chargePower), MathHelper.Lerp(62f, 104f, chargePower));
				Dust.NewDustPerfect(sparkPosition, 242, (sparkTarget - sparkPosition).SafeNormalize(direction) * Main.rand.NextFloat(3.2f, 8.2f), 35, Color.White, Main.rand.NextFloat(0.78f, 1.42f + chargePower)).noGravity = true;
			}
			float lightPower = MathHelper.Lerp(0.62f, 1.9f, chargePower);
			MoDaoQianRenWarmupSystem.AddLight(focus, lightPower, 0.16f + chargePower * 0.12f, lightPower * 1.35f);
			MoDaoQianRenWarmupSystem.AddLight(base.Projectile.Center + direction * MathHelper.Lerp(30f, visualLength, 0.68f), 0.4f + chargePower, 0.08f, 0.8f + chargePower * 1.2f);
		}
	}

	private void ProduceGreatswordBurstChargeReleaseEffects(Player player, Vector2 handPosition, Vector2 direction, float chargePower)
	{
		if (!Main.dedServ)
		{
			Vector2 normal = direction.RotatedBy(1.5707963705062866);
			int dustCount = (int)MathHelper.Lerp(26f, 72f, chargePower);
			for (int i = 0; i < dustCount; i++)
			{
				float side = Main.rand.NextFloat(-1f, 1f);
				Dust.NewDustPerfect(handPosition + direction * Main.rand.NextFloat(18f, 140f) + normal * side * Main.rand.NextFloat(8f, 70f), Velocity: direction * Main.rand.NextFloat(MathHelper.Lerp(5f, 11f, chargePower), MathHelper.Lerp(12f, 26f, chargePower)) + normal * side * Main.rand.NextFloat(1.2f, 7.5f), Type: Main.rand.NextBool(3) ? 242 : 27, Alpha: 48, newColor: Color.Lerp(new Color(236, 116, 255), Color.White, chargePower * 0.5f), Scale: Main.rand.NextFloat(1.1f, 2.4f + chargePower)).noGravity = true;
			}
			MoDaoQianRenWarmupSystem.AddLight(handPosition + direction * 120f, 1.2f + chargePower * 1.4f, 0.18f, 1.8f + chargePower * 1.6f);
		}
	}

	private void ProduceWindupDust()
	{
		if (!Main.dedServ)
		{
			Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
			Vector2 normal = direction.RotatedBy(1.5707963705062866);
			int dustCount = (IsGreatswordBurst ? 42 : (IsGreatswordHeavySlash ? 36 : (IsGreatswordSwing ? 26 : (IsHeavySlash ? 16 : (IsSpinAttack ? 12 : 7)))));
			for (int i = 0; i < dustCount; i++)
			{
				Dust dust = Dust.NewDustPerfect(base.Projectile.Center - direction * Main.rand.NextFloat(18f, IsGreatswordBladeForm ? 100f : (IsHeavySlash ? 62f : (IsSpinAttack ? 46f : 38f))) + normal * Main.rand.NextFloat(IsGreatswordBladeForm ? (-48f) : (IsHeavySlash ? (-26f) : (-18f)), IsGreatswordBladeForm ? 48f : (IsHeavySlash ? 26f : 18f)), 27, direction * Main.rand.NextFloat(IsGreatswordBladeForm ? 1.9f : (IsHeavySlash ? 1.4f : 0.8f), IsGreatswordBladeForm ? 4.9f : (IsHeavySlash ? 3.4f : 2.2f)) - normal * Main.rand.NextFloat(-1.1f, 1.1f), 85, IsGreatswordBladeForm ? new Color(245, 120, 255) : (IsHeavySlash ? new Color(225, 120, 255) : new Color(170, 90, 255)), Main.rand.NextFloat(IsGreatswordBladeForm ? 1.2f : (IsHeavySlash ? 1.05f : 0.75f), IsGreatswordBladeForm ? 2.35f : (IsHeavySlash ? 1.7f : 1.25f)));
				dust.noGravity = true;
				dust.fadeIn = Main.rand.NextFloat(0.2f, 0.55f);
			}
			if (IsHeavySlash || IsGreatswordBladeForm)
			{
				MoDaoQianRenWarmupSystem.AddLight(base.Projectile.Center - direction * 32f, IsGreatswordBladeForm ? 1f : 0.75f, 0.1f, IsGreatswordBladeForm ? 1.45f : 1.15f);
			}
		}
	}

	private void ProduceReleaseDust(float intensity)
	{
		if (!Main.dedServ)
		{
			Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
			Vector2 normal = direction.RotatedBy(1.5707963705062866);
			float length = GetVisualBladeLength();
			int dustCount = (int)(MathHelper.Lerp(18f, 34f, GetDistanceFactor(TargetBladeLength)) * intensity);
			for (int i = 0; i < dustCount; i++)
			{
				float alongBlade = Main.rand.NextFloat(42f, length);
				Dust.NewDustPerfect(base.Projectile.Center + direction * alongBlade + normal * Main.rand.NextFloat(IsGreatswordBladeForm ? (-64f) : (IsHeavySlash ? (-30f) : (IsSpinAttack ? (-28f) : (-16f))), IsGreatswordBladeForm ? 64f : (IsHeavySlash ? 30f : (IsSpinAttack ? 28f : 16f))), Velocity: IsGreatswordBladeForm ? (direction * Main.rand.NextFloat(2.4f, IsGreatswordBurst ? 9.8f : (IsGreatswordHeavySlash ? 8.8f : 7.2f)) + normal * Main.rand.NextFloat(-7.8f, 7.8f)) : (IsHeavySlash ? (direction * Main.rand.NextFloat(1.8f, 5.8f) + normal * Main.rand.NextFloat(-5.4f, 5.4f)) : (IsSpinAttack ? (normal * Main.rand.NextFloat(-5.6f, 5.6f) + direction * Main.rand.NextFloat(1.2f, 3.6f)) : (normal * Main.rand.NextFloat(-4.2f, 4.2f) + direction * Main.rand.NextFloat(0.8f, 2.8f)))), Type: Main.rand.NextBool(3) ? 242 : 62, Alpha: 55, newColor: new Color(235, 185, 255), Scale: Main.rand.NextFloat(1.15f, 2.15f)).noGravity = true;
			}
			MoDaoQianRenWarmupSystem.AddLight(base.Projectile.Center + direction * MathHelper.Lerp(70f, length, 0.72f), 1f, 0.18f, 1.45f);
		}
	}

	private void UpdateShardStream(Player player, Vector2 handPosition)
	{
		bool stillChanneling = ((base.Projectile.owner != Main.myPlayer) ? (!player.noItems && !player.CCed) : (Main.mouseRight && !player.noItems && !player.CCed));
		if (base.Projectile.owner == Main.myPlayer && !stillChanneling)
		{
			base.Projectile.Kill();
			return;
		}
		int growthStage = GetCurrentGrowthStage(player);
		int fireRate = MoDaoQianRen.ApplyMeleeAttackSpeed(player, GetShardStreamConsumeRate(growthStage), 2);
		if (base.Projectile.owner == Main.myPlayer && Timer % (float)fireRate == 0f)
		{
			if (!player.GetModPlayer<MoDaoQianRenPlayer>().TryConsumeShardCharge(1))
			{
				base.Projectile.Kill();
				return;
			}
			Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
			Vector2 normal = direction.RotatedBy(1.5707963705062866);
			int shardCount = MoDaoQianRen.GetShardStreamShardCount(growthStage);
			float sideRange = ((growthStage >= 2) ? 25f : 16f);
			float spawnMin = ((growthStage >= 2) ? 52f : 40f);
			float spawnMax = ((growthStage >= 2) ? 112f : 82f);
			float minSpeed = ((growthStage >= 2) ? 13.5f : 10.5f);
			float num = ((growthStage >= 2) ? ((growthStage >= 6) ? ((growthStage < 7) ? 22f : 24f) : ((growthStage < 3) ? 18f : 19.5f)) : ((growthStage < 1) ? 14.5f : 16f));
			float maxSpeed = num;
			float shardMode = ((growthStage >= 4) ? 2f : 0f);
			for (int i = 0; i < shardCount; i++)
			{
				float sideOffset = Main.rand.NextFloat(0f - sideRange, sideRange);
				Vector2 spawnPosition = handPosition + direction * Main.rand.NextFloat(spawnMin, spawnMax) + normal * sideOffset;
				Vector2 shardVelocity = direction.RotatedBy(Main.rand.NextFloat(-0.09f, 0.09f)) * Main.rand.NextFloat(minSpeed, maxSpeed);
				Projectile.NewProjectile(base.Projectile.GetSource_FromThis(), spawnPosition, shardVelocity, ModContent.ProjectileType<MoDaoQianRenShardProjectile>(), base.Projectile.damage, base.Projectile.knockBack * 0.5f, base.Projectile.owner, Main.rand.Next(10000), Main.rand.NextFloat(0.92f, (growthStage >= 4) ? 1.34f : 1.22f), shardMode);
			}
		}
		if (base.Projectile.soundDelay <= 0)
		{
			base.Projectile.soundDelay = 12;
			SoundStyle style = SoundID.Item15 with
			{
				Volume = 0.45f
			};
			SoundEngine.PlaySound(in style, base.Projectile.Center);
		}
	}

	private int GetShardChargeGain(NPC target, int growthStage)
	{
		MoDaoQianRenStageStats stats = MoDaoQianRen.GetRuntimeStats(growthStage);
		if (IsGreatswordDevilsHoldout)
		{
			return stats.CrimsonRiftShardChargeGain;
		}
		int chargeGain = IsGreatswordLeftSlash ? stats.GreatswordHitShardChargeGain : stats.BladeHitShardChargeGain;
		if (IsFinisherAttack)
		{
			chargeGain += IsGreatswordLeftSlash ? stats.GreatswordFinisherShardChargeBonus : stats.BladeFinisherShardChargeBonus;
		}
		if (IsTipHit(target))
		{
			chargeGain += stats.TipHitShardChargeBonus;
		}
		return chargeGain;
	}

	private void ProduceHitEffects(NPC target, int damageDone)
	{
		if (!Main.dedServ)
		{
			Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
			Vector2 normal = direction.RotatedBy(1.5707963705062866);
			float impactScale = MathHelper.Clamp((float)damageDone / MathF.Max(1f, (float)base.Projectile.damage), 0.7f, 1.65f);
			bool useFinisherEffects = IsGreatswordBladeForm || IsHeavySlash || IsSpinAttack;
			if (IsModeOneBladeSwing)
			{
				bladeImpactFlashCenter = target.Center;
				bladeImpactFlashTimer = 9f;
			}
			SoundEngine.PlaySound(SoundID.Item10 with
			{
				Volume = (useFinisherEffects ? 0.58f : 0.46f),
				Pitch = (IsGreatswordBladeForm ? (-0.52f) : (IsHeavySlash ? (-0.4f) : (IsSpinAttack ? (-0.28f) : (-0.18f))))
			}, target.Center);
			int dustCount = (IsGreatswordBurst ? 46 : ((!IsGreatswordSwing) ? (useFinisherEffects ? 24 : 18) : (IsGreatswordHeavySlash ? 42 : 34)));
			for (int i = 0; i < dustCount; i++)
			{
				Vector2 burstDirection = (IsGreatswordBladeForm ? (direction * Main.rand.NextFloat(0.1f, 1.15f) + normal * Main.rand.NextFloat(-1.65f, 1.65f)).SafeNormalize(direction) : (IsHeavySlash ? (direction * Main.rand.NextFloat(0.2f, 0.95f) + normal * Main.rand.NextFloat(-1.25f, 1.25f)).SafeNormalize(direction) : (IsSpinAttack ? (normal * Main.rand.NextFloat(-1.25f, 1.25f) + direction * Main.rand.NextFloat(-0.35f, 0.75f)).SafeNormalize(direction) : (normal * Main.rand.NextFloat(-1f, 1f) + direction * Main.rand.NextFloat(0.25f, 1.1f)).SafeNormalize(direction))));
				Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular((float)target.width * 0.35f, (float)target.height * 0.35f), Main.rand.NextBool(4) ? 27 : 62, burstDirection * Main.rand.NextFloat(2.2f, useFinisherEffects ? 7.2f : 5.4f) * impactScale, 65, new Color(230, 180, 255), Main.rand.NextFloat(1.05f, 1.85f) * impactScale).noGravity = true;
			}
			if (IsModeOneBladeSwing)
			{
				ProduceModeOneHitSparks(target.Center, direction, normal, impactScale);
			}
			MoDaoQianRenWarmupSystem.AddLight(target.Center, 0.8f * impactScale, 0.12f, 1.15f * impactScale);
		}
	}

	private bool IsTipHit(NPC target)
	{
		Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
		float bladeLength = GetVisualBladeLength();
		if (bladeLength <= 128f)
		{
			return false;
		}
		float hitDistance = Vector2.Dot(target.Center - base.Projectile.Center, direction);
		if (hitDistance >= bladeLength * 0.72f)
		{
			return hitDistance <= bladeLength + 48f;
		}
		return false;
	}

	private void TryReleaseComboShard(Player player, NPC target, int growthStage)
	{
		if (!(AssistShardCooldown > 0f))
		{
			AssistShardCooldown = ((growthStage >= 5) ? 6f : 10f);
			Vector2 direction = base.Projectile.velocity.SafeNormalize((target.Center - base.Projectile.Center).SafeNormalize(Vector2.UnitX * player.direction));
			int shardCount = ((growthStage < 5) ? 1 : 2);
			int shardDamage = Math.Max(1, (int)((float)base.Projectile.damage * MoDaoQianRen.GetComboShardDamageMultiplier(growthStage)));
			float homingMode = ((growthStage >= 6) ? 1f : 0f);
			for (int i = 0; i < shardCount; i++)
			{
				Vector2 shardVelocity = direction.RotatedBy(Main.rand.NextFloat(-0.22f, 0.22f)) * Main.rand.NextFloat(12.5f, (growthStage >= 3) ? 19.5f : 16.5f);
				Projectile.NewProjectile(base.Projectile.GetSource_FromThis(), target.Center - direction * Main.rand.NextFloat(8f, 24f) + Main.rand.NextVector2Circular(8f, 8f), shardVelocity, ModContent.ProjectileType<MoDaoQianRenShardProjectile>(), shardDamage, base.Projectile.knockBack * 0.32f, base.Projectile.owner, Main.rand.Next(10000), Main.rand.NextFloat(0.82f, 1.08f), homingMode);
			}
		}
	}

	private void TryReleaseMoonlitGreatswordAssistShards(Player player, NPC target, int growthStage)
	{
		if (!(AssistShardCooldown > 0f))
		{
			AssistShardCooldown = (IsGreatswordHeavySlash ? 12f : 16f);
			Vector2 direction = base.Projectile.velocity.SafeNormalize((target.Center - base.Projectile.Center).SafeNormalize(Vector2.UnitX * player.direction));
			Vector2 normal = direction.RotatedBy(1.5707963705062866);
			float bladeLength = GetVisualBladeLength();
			int shardCount = ((!IsGreatswordHeavySlash) ? 1 : 2);
			int shardDamage = Math.Max(1, (int)((float)base.Projectile.damage * MoDaoQianRen.GetGreatswordAssistShardDamageMultiplier(growthStage)));
			for (int i = 0; i < shardCount; i++)
			{
				Vector2 spawnPosition = base.Projectile.Center + direction * Main.rand.NextFloat(60f, MathF.Max(92f, bladeLength - 32f)) + normal * Main.rand.NextFloat(-20f, 20f);
				Vector2 shardVelocity = (target.Center - spawnPosition).SafeNormalize(direction).RotatedBy(Main.rand.NextFloat(-0.18f, 0.18f)) * Main.rand.NextFloat(14f, 20f);
				Projectile.NewProjectile(base.Projectile.GetSource_FromThis(), spawnPosition, shardVelocity, ModContent.ProjectileType<MoDaoQianRenShardProjectile>(), shardDamage, base.Projectile.knockBack * 0.34f, base.Projectile.owner, Main.rand.Next(10000), Main.rand.NextFloat(0.92f, 1.16f), 1f);
			}
		}
	}

	private void TryReleaseFinalAssistShards(Player player, NPC target, int growthStage)
	{
		if (!(AssistShardCooldown > 0f))
		{
			AssistShardCooldown = 10f;
			Vector2 direction = base.Projectile.velocity.SafeNormalize((target.Center - base.Projectile.Center).SafeNormalize(Vector2.UnitX * player.direction));
			Vector2 normal = direction.RotatedBy(1.5707963705062866);
			float bladeLength = GetVisualBladeLength();
			MoDaoQianRenPlayer modPlayer = player.GetModPlayer<MoDaoQianRenPlayer>();
			int fullShardThreshold = Math.Min(modPlayer.GetCurrentMaxShardCharge(), 140);
			int shardCount = ((modPlayer.ShardCharge >= fullShardThreshold) ? 3 : 2);
			int shardDamage = Math.Max(1, (int)((float)base.Projectile.damage * MoDaoQianRen.GetFinalAssistShardDamageMultiplier(growthStage)));
			for (int i = 0; i < shardCount; i++)
			{
				Vector2 spawnPosition = base.Projectile.Center + direction * Main.rand.NextFloat(62f, MathF.Max(96f, bladeLength - 24f)) + normal * Main.rand.NextFloat(-22f, 22f);
				Vector2 shardVelocity = (target.Center - spawnPosition).SafeNormalize(direction).RotatedBy(Main.rand.NextFloat(-0.16f, 0.16f)) * Main.rand.NextFloat(15.5f, 22.5f);
				Projectile.NewProjectile(base.Projectile.GetSource_FromThis(), spawnPosition, shardVelocity, ModContent.ProjectileType<MoDaoQianRenShardProjectile>(), shardDamage, base.Projectile.knockBack * 0.38f, base.Projectile.owner, Main.rand.Next(10000), Main.rand.NextFloat(1.05f, 1.35f), 3f);
			}
		}
	}

	private void ProduceStreamChargeEffects(Vector2 handPosition)
	{
		if (!Main.dedServ && Timer % 2f == 0f)
		{
			Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
			Vector2 normal = direction.RotatedBy(1.5707963705062866);
			Vector2 position = handPosition + direction * Main.rand.NextFloat(36f, 76f) + normal * Main.rand.NextFloat(-18f, 18f);
			Dust.NewDustPerfect(position, 62, direction * Main.rand.NextFloat(1.2f, 3.2f) + normal * Main.rand.NextFloat(-0.9f, 0.9f), 40, new Color(175, 110, 255), Main.rand.NextFloat(1.05f, 1.65f)).noGravity = true;
			MoDaoQianRenWarmupSystem.AddLight(position, 0.45f, 0.08f, 0.9f);
		}
	}

	private void ProduceSwingEffects()
	{
		if (Main.dedServ)
		{
			return;
		}
		int duration = GetAttackDuration(Main.player[base.Projectile.owner]);
		float attackProgress = Utils.GetLerpValue(0f, duration, Timer, clamped: true);
		float activePower = GetCurrentAttackPower(attackProgress);
		if (!(activePower <= 0.05f) || Timer % 3f == 0f)
		{
			Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
			Vector2 normal = direction.RotatedBy(1.5707963705062866);
			float length = GetVisualBladeLength();
			float distanceFactor = GetDistanceFactor(TargetBladeLength);
			bool useFinisherEffects = IsHeavySlash || IsSpinAttack;
			bool useGreatswordEffects = IsGreatswordBladeForm;
			int dustCount = ((TargetBladeLength > 150f) ? 5 : 3) + (int)(activePower * (useGreatswordEffects ? 10f : (useFinisherEffects ? 5f : 3f)));
			for (int i = 0; i < dustCount; i++)
			{
				float progress = (useGreatswordEffects ? Main.rand.NextFloat(0.12f, 1f) : (IsHeavySlash ? Main.rand.NextFloat(0.36f, 1f) : Main.rand.NextFloat(0.2f, 1f)));
				Dust dust = Dust.NewDustPerfect(base.Projectile.Center + direction * MathHelper.Lerp(40f, length, progress) + normal * Main.rand.NextFloat(useGreatswordEffects ? (-24f) : (-10f), useGreatswordEffects ? 24f : 10f) * MathHelper.Lerp(0.45f, 1.3f + activePower * (useGreatswordEffects ? 1f : 0.6f), distanceFactor), Main.rand.NextBool(3) ? 242 : 62, useGreatswordEffects ? (normal * Main.rand.NextFloat(-4.4f, 4.4f) - direction * Main.rand.NextFloat(0.15f, 1.5f)) : (IsHeavySlash ? (normal * Main.rand.NextFloat(-2.6f, 2.6f) - direction * Main.rand.NextFloat(0.2f, 1.2f)) : (IsSpinAttack ? (normal * Main.rand.NextFloat(-2.8f, 2.8f) - direction * Main.rand.NextFloat(0.2f, 1.8f)) : (normal * Main.rand.NextFloat(-1.8f, 1.8f) - direction * Main.rand.NextFloat(0.3f, 1.4f)))), 70, useGreatswordEffects ? new Color(235, 125, 255) : new Color(210, 95, 255), Main.rand.NextFloat(useGreatswordEffects ? 1.2f : 1.05f, (useGreatswordEffects ? 1.95f : 1.55f) + activePower * (useGreatswordEffects ? 1.1f : 0.85f)));
				dust.noGravity = true;
				dust.fadeIn = Main.rand.NextFloat(0.35f, 0.8f);
			}
			MoDaoQianRenWarmupSystem.AddLight(base.Projectile.Center + direction * MathHelper.Lerp(80f, length, 0.72f), 0.55f + distanceFactor * 0.35f, 0.08f, 0.95f + distanceFactor * 0.35f);
			EmitBladeEnchantmentVisuals(direction, normal, length, activePower);
		}
	}

	private void EmitBladeEnchantmentVisuals(Vector2 direction, Vector2 normal, float length, float activePower)
	{
		if (!base.Projectile.noEnchantments && !(activePower <= 0.08f))
		{
			int sampleCount = ((!(TargetBladeLength > 150f)) ? 1 : 2);
			for (int i = 0; i < sampleCount; i++)
			{
				float progress = Main.rand.NextFloat(0.32f, 0.95f);
				Vector2 position = base.Projectile.Center + direction * MathHelper.Lerp(40f, length, progress) + normal * Main.rand.NextFloat(-10f, 10f);
				base.Projectile.EmitEnchantmentVisualsAt(position - new Vector2(6f), 12, 12);
			}
		}
	}

	private float GetVisualBladeLength()
	{
		if (IsGreatswordComboDash)
		{
			if (IsGreatswordComboRingSlash)
			{
				return GetGreatswordComboRingSlashRadius();
			}
			return GetGreatswordComboDashVisibleLength();
		}
		if (IsGreatswordBurst && base.Projectile.owner >= 0 && base.Projectile.owner < 255)
		{
			int duration = GetAttackDuration(Main.player[base.Projectile.owner]);
			float progress = Utils.GetLerpValue(0f, duration, Timer, clamped: true);
			float extension = SmoothStep(Utils.GetLerpValue(0.07f, 0.24f, progress, clamped: true));
			float overshoot = MathF.Sin(MathHelper.Clamp(Utils.GetLerpValue(0.12f, 0.34f, progress, clamped: true), 0f, 1f) * (float)Math.PI) * 0.14f;
			float targetLength = MathF.Max(120f, TargetBladeLength);
			float value = MathHelper.Lerp(150f, targetLength, extension) * (1f + overshoot);
			float retract = SmoothStep(Utils.GetLerpValue(0.66f, 0.96f, progress, clamped: true));
			float retractedLength = MathF.Max(150f, targetLength * 0.38f);
			return MathHelper.Lerp(value, retractedLength, retract);
		}
		return MathF.Max(120f, TargetBladeLength);
	}

	private static int GetCurrentGrowthStage(Player player)
	{
		if (!MoDaoQianRen.TryGetGrowthStage(player.HeldItem, out var growthStage))
		{
			return 0;
		}
		return growthStage;
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

	private void DrawCompleteBlade(Texture2D texture, Vector2 drawPosition, float rotation, SpriteEffects effects)
	{
		Main.EntitySpriteDraw(origin: new Vector2(156f, 17.5f), texture: texture, position: drawPosition, sourceRectangle: GetWeaponOutPulseFrame(texture), color: Color.White, rotation: rotation, scale: 0.99863356f * base.Projectile.scale, effects: effects);
	}

	private void DrawSplitBlade(Texture2D hiltTexture, Vector2 drawPosition, float rotation, SpriteEffects effects)
	{
		Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
		Vector2 normal = direction.RotatedBy(1.5707963705062866);
		float length = GetVisualBladeLength();
		float distanceFactor = GetDistanceFactor(IsGreatswordComboDash ? length : TargetBladeLength);
		bool greatsword = IsGreatswordBladeForm;
		if (greatsword)
		{
			if (IsGreatswordBurstCharge)
			{
				Texture2D completeBladeTexture = ModContent.Request<Texture2D>("魔刀千刃/Content/Items/Weapons/MoDaoQianRen_weaponout_pulse").Value;
				DrawCompleteBlade(completeBladeTexture, drawPosition + Vector2.UnitY * 6f, rotation, effects);
			}
			else if (IsGreatswordComboDash)
			{
				if (IsGreatswordComboRingSlash)
				{
					DrawGreatswordMicroShardBlade(hiltTexture, drawPosition, direction, normal, length, distanceFactor, rotation, effects);
				}
				else
				{
					DrawGreatswordPoleVaultBlade(hiltTexture, drawPosition, direction, normal, length, distanceFactor, rotation, effects);
				}
			}
			else
			{
				DrawGreatswordMicroShardBlade(hiltTexture, drawPosition, direction, normal, length, distanceFactor, rotation, effects);
			}
			return;
		}
		int shardCount = (greatsword ? ((int)MathHelper.Clamp(length / 2.35f, 170f, 430f)) : ((int)MathHelper.Clamp(length / 5.6f, 62f, 188f)));
		int microShardCount = (greatsword ? ((int)MathHelper.Clamp(length / 2.55f, 160f, 380f)) : ((int)MathHelper.Clamp(length / 5.4f, 54f, 168f)));
		Texture2D shardTexture = MoDaoQianRenShardVisuals.Texture;
		Texture2D pixel = TextureAssets.MagicPixel.Value;
		for (int i = 0; i < shardCount; i++)
		{
			float progress = (greatsword ? MathF.Pow(((float)i + 0.5f) / (float)shardCount, 1.28f) : (((float)i + 0.5f) / (float)shardCount));
			Vector2 vector = base.Projectile.Center + direction * MathHelper.Lerp(46f, length, progress);
			float shardSeed = (float)i + 131f;
			float breathRate = MathHelper.Lerp(0.045f, 0.135f, MoDaoQianRenShardVisuals.Random01(i * 31 + 5));
			float shimmerRate = MathHelper.Lerp(0.22f, 0.68f, MoDaoQianRenShardVisuals.Random01(i * 47 + 17));
			float breath = 0.5f + MathF.Sin(Timer * breathRate + (float)i * 0.61f) * 0.5f;
			float shimmer = MoDaoQianRenShardVisuals.Flicker(Timer, i + 37, shimmerRate * 0.45f, shimmerRate);
			float scatter = (greatsword ? MathHelper.Lerp(0.25f, 2.6f + distanceFactor * 1.8f, MathF.Pow(progress, 0.7f)) : MathHelper.Lerp(1.4f, 10f + distanceFactor * 5.4f, MathF.Pow(progress, 0.78f)));
			float sideDrift = MathF.Sin(Timer * MathHelper.Lerp(0.06f, 0.18f, MoDaoQianRenShardVisuals.Random01(i * 23 + 3)) + (float)i * 2.17f) * scatter;
			if (greatsword)
			{
				float sideSeed = MoDaoQianRenShardVisuals.Random01(i * 83 + 13) * 2f - 1f;
				float widthSeed = MathHelper.Lerp(0.25f, 1f, MoDaoQianRenShardVisuals.Random01(i * 97 + 21));
				sideDrift = sideDrift * 0.45f + sideSeed * GetGreatswordBladeHalfWidth(progress, distanceFactor) * widthSeed;
			}
			float lengthDrift = MathF.Cos(Timer * MathHelper.Lerp(0.05f, 0.14f, MoDaoQianRenShardVisuals.Random01(i * 29 + 11)) + (float)i * 1.11f) * MathHelper.Lerp(0.35f, 4.6f, progress);
			Vector2 shardPosition = vector + normal * sideDrift + direction * lengthDrift;
			float shardScale = (greatsword ? MathHelper.Lerp(0.026f, 0.011f, progress) : MathHelper.Lerp(0.082f, 0.05f, progress)) * base.Projectile.scale;
			shardScale *= MathHelper.Lerp(0.86f, 1.18f, breath) * MathHelper.Lerp(0.92f, 1.14f, shimmer);
			if (greatsword)
			{
				shardScale *= MathHelper.Lerp(1f, 0.58f, progress);
			}
			float shardRotation = rotation + MathF.Sin(Timer * MathHelper.Lerp(0.09f, 0.21f, MoDaoQianRenShardVisuals.Random01(i * 59 + 7)) + (float)i * 0.83f) * MathHelper.Lerp(0.18f, 0.62f, progress) + MathF.Sin((float)i * 1.37f) * 0.26f;
			float glowPower = MathHelper.Clamp(0.22f + shimmer * 0.62f + distanceFactor * 0.28f, 0f, 1f);
			Color outline = Color.Lerp(greatsword ? new Color(134, 16, 245) : new Color(104, 24, 230), new Color(224, 58, 255), shimmer) * (0.45f + glowPower * 0.22f);
			Color shardColor = Color.Lerp(greatsword ? new Color(215, 86, 255) : new Color(190, 92, 255), new Color(255, 226, 255), 0.18f + shimmer * 0.48f);
			shardColor = Color.Lerp(shardColor, new Color(170, 80, 255), progress * 0.24f);
			Color flashColor = Color.Lerp(new Color(255, 150, 255), Color.White, shimmer * 0.75f);
			MoDaoQianRenShardVisuals.DrawOutlinedShard(shardTexture, shardPosition - Main.screenPosition, (int)shardSeed, outline, shardColor, flashColor, shardRotation, shardScale, shimmer);
			if (!greatsword && shimmer > 0.9f && i % 6 == (int)(Timer / 4f) % 6)
			{
				DrawShardSpark(pixel, shardPosition, shardRotation, progress, shimmer);
			}
			if (i % 9 == 0)
			{
				MoDaoQianRenWarmupSystem.AddLight(shardPosition, 0.12f + glowPower * 0.12f, 0.02f, 0.26f + glowPower * 0.18f);
			}
		}
		for (int j = 0; j < microShardCount; j++)
		{
			float progress2 = (greatsword ? MathF.Pow(((float)j + MoDaoQianRenShardVisuals.Random01(j * 17 + 5)) / (float)microShardCount, 1.38f) : (((float)j + MoDaoQianRenShardVisuals.Random01(j * 17 + 5)) / (float)microShardCount));
			progress2 = MathHelper.Clamp(progress2, 0.02f, 0.995f);
			float lateralSeed = MoDaoQianRenShardVisuals.Random01(j * 41 + 19) * 2f - 1f;
			float edgeSide = ((j % 3 == 0) ? 0f : ((j % 2 == 0) ? 1f : (-1f)));
			bool innerShard = edgeSide == 0f;
			float edgeWidth = MathHelper.Lerp(3.5f, 15f + distanceFactor * 6f, MathF.Pow(progress2, 0.72f));
			float greatswordHalfWidth = 0f;
			if (greatsword)
			{
				greatswordHalfWidth = GetGreatswordBladeHalfWidth(progress2, distanceFactor);
				edgeWidth = (innerShard ? (greatswordHalfWidth * MathHelper.Lerp(0.28f, 0.66f, MathF.Abs(lateralSeed))) : (greatswordHalfWidth * MathHelper.Lerp(0.86f, 1.08f, MoDaoQianRenShardVisuals.Random01(j * 61 + 7))));
			}
			float sideOffset = (greatsword ? (innerShard ? (lateralSeed * edgeWidth) : (edgeSide * edgeWidth + lateralSeed * MathHelper.Lerp(0.6f, 3.4f, 1f - progress2))) : (innerShard ? (lateralSeed * edgeWidth * 0.36f) : (edgeSide * edgeWidth + lateralSeed * MathHelper.Lerp(1.1f, 4.2f, progress2))));
			Vector2 shardPosition2 = base.Projectile.Center + direction * MathHelper.Lerp(58f, length, progress2) + normal * (sideOffset + MathF.Sin(Timer * 0.11f + (float)j * 1.9f) * MathHelper.Lerp(0.35f, 1.75f, progress2)) + direction * ((MoDaoQianRenShardVisuals.Random01(j * 73 + 29) - 0.5f) * MathHelper.Lerp(0.8f, 5.4f, progress2) + MathF.Sin(Timer * 0.07f + (float)j * 1.27f) * MathHelper.Lerp(0.4f, 2.6f, progress2));
			float flicker = MoDaoQianRenShardVisuals.Flicker(Timer, j + 509, 0.09f, 0.82f);
			float sizeSeed = MoDaoQianRenShardVisuals.Random01(j * 67 + 31);
			float scale = MathHelper.Lerp(0.037f, 0.019f, progress2) * MathHelper.Lerp(0.72f, 1.18f, sizeSeed) * MathHelper.Lerp(0.82f, 1.34f, flicker) * base.Projectile.scale * (greatsword ? MathHelper.Lerp(0.58f, 0.25f, progress2) : 1f);
			float shardRotation2 = rotation + edgeSide * MathHelper.Lerp(0.22f, 0.72f, progress2) + lateralSeed * MathHelper.Lerp(0.18f, 0.56f, progress2) + MathF.Sin(Timer * 0.2f + (float)j) * 0.18f;
			Color outline2 = new Color(146, 34, 255) * (0.22f + flicker * 0.2f);
			Color core = Color.Lerp(new Color(190, 76, 255), Color.White, flicker * 0.62f) * MathHelper.Lerp(0.54f, 0.92f, progress2);
			Color flash = Color.Lerp(new Color(250, 145, 255), Color.White, flicker * 0.78f);
			MoDaoQianRenShardVisuals.DrawOutlinedShard(shardTexture, shardPosition2 - Main.screenPosition, j + 701, outline2, core, flash, shardRotation2, scale, flicker);
		}
		DrawHandle(hiltTexture, drawPosition, rotation, effects, base.Projectile.scale);
	}

	private void ProduceCrimsonRiftHitEffects(NPC target, Player player, int damageDone)
	{
		ProduceHitEffects(target, damageDone);
		TryAddSwordScreenShake(player, (target.Center - player.Center).SafeNormalize(base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction)) * 2.15f);
		if (Main.dedServ)
		{
			return;
		}
		Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
		Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
		for (int i = 0; i < 34; i++)
		{
			float side = Main.rand.NextFloat(-1f, 1f);
			Vector2 velocity = (direction * Main.rand.NextFloat(0.55f, 1.35f) + normal * side * Main.rand.NextFloat(0.45f, 1.9f)).SafeNormalize(direction) * Main.rand.NextFloat(5.8f, 14.5f);
			Dust dust = Dust.NewDustPerfect(target.Center + normal * Main.rand.NextFloat(-28f, 28f) - direction * Main.rand.NextFloat(4f, 22f), Main.rand.NextBool(3) ? DustID.Blood : DustID.RedTorch, velocity, 30, new Color(255, 46, 74), Main.rand.NextFloat(1.1f, 2.25f));
			dust.noGravity = true;
			dust.fadeIn = Main.rand.NextFloat(0.05f, 0.28f);
		}
		MoDaoQianRenWarmupSystem.AddLight(target.Center, 1.35f, 0.08f, 0.12f);
	}

	private void DrawGreatswordDevilsFogBlade(Texture2D hiltTexture, Vector2 drawPosition, Vector2 direction, float rotation, SpriteEffects effects)
	{
		direction = direction.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
		Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
		float length = MathF.Max(120f, TargetBladeLength);
		float distanceFactor = GetDistanceFactor(length);
		float scale = MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f);
		float rootDistance = GreatswordDevilsFogBladeRootDistance * scale;
		float bladeLength = MathF.Max(36f, length - rootDistance);
		float swingPower = GetCurrentAttackPower(0f);
		float auraPower = MathHelper.Lerp(0.38f, 0.82f, MathHelper.Clamp(swingPower, 0f, 1f));
		Vector2 rootPosition = base.Projectile.Center + direction * rootDistance - Main.screenPosition;
		MoDaoQianRenGreatswordFogVisuals.Draw(rootPosition, direction, normal, bladeLength, (float progress) => GetGreatswordDevilsFogBladeHalfWidth(progress, distanceFactor) * scale, distanceFactor, auraPower, Timer * 0.35f, 11800);
		DrawCrimsonRiftPressureLine(rootPosition, direction, normal, bladeLength, distanceFactor, swingPower, scale);
		DrawHandle(hiltTexture, drawPosition, rotation, effects, scale);
	}

	private void DrawCrimsonRiftPressureLine(Vector2 rootPosition, Vector2 direction, Vector2 normal, float bladeLength, float distanceFactor, float swingPower, float scale)
	{
		float pulse = MathHelper.Clamp(swingPower, 0.18f, 1f);
		float alpha = MathHelper.Lerp(0.26f, 0.9f, pulse);
		Texture2D texture = TextureAssets.Extra[98].Value;
		Vector2 origin = texture.Size() * 0.5f;
		Vector2 center = rootPosition + direction * (bladeLength * 0.52f) + normal * MathF.Sin(Timer * 0.42f) * MathHelper.Lerp(2f, 7f, distanceFactor);
		float width = MathHelper.Lerp(26f, 58f, distanceFactor) * MathHelper.Lerp(0.72f, 1.18f, pulse) * scale;
		Vector2 drawScale = new Vector2(MathF.Max(0.01f, bladeLength / texture.Width), MathF.Max(0.01f, width / texture.Height));
		Color shadow = new Color(120, 0, 24) * (alpha * 0.55f);
		Color body = new Color(255, 34, 66) * (alpha * 0.72f);
		Color core = new Color(255, 214, 220) * (alpha * 0.32f);
		Main.EntitySpriteDraw(texture, center, null, shadow, direction.ToRotation(), origin, drawScale * new Vector2(1.04f, 1.6f), SpriteEffects.None);
		Main.EntitySpriteDraw(texture, center, null, body, direction.ToRotation(), origin, drawScale * new Vector2(1f, 0.78f), SpriteEffects.None);
		Main.EntitySpriteDraw(texture, center + normal * MathF.Sin(Timer * 0.66f) * 2.5f, null, core, direction.ToRotation(), origin, drawScale * new Vector2(0.92f, 0.34f), SpriteEffects.None);
	}

	private static float GetGreatswordDevilsFogBladeHalfWidth(float progress, float distanceFactor)
	{
		progress = MathHelper.Clamp(progress, 0f, 1f);
		float body = MathF.Sin(progress * MathHelper.Pi);
		float root = SmoothStep(Utils.GetLerpValue(0f, 0.16f, progress, clamped: true));
		float tip = 1f - SmoothStep(Utils.GetLerpValue(0.78f, 1f, progress, clamped: true));
		float profile = MathHelper.Clamp(MathF.Max(body, 0.34f) * root * MathHelper.Lerp(0.48f, 1f, tip), 0.18f, 1f);
		return GreatswordDevilsFogBladeWidth * MathHelper.Lerp(0.72f, 1.18f, distanceFactor) * profile;
	}

	private void DrawGreatswordPoleVaultBlade(Texture2D hiltTexture, Vector2 drawPosition, Vector2 direction, Vector2 normal, float length, float distanceFactor, float rotation, SpriteEffects effects)
	{
		Texture2D shardTexture = MoDaoQianRenShardVisuals.Texture;
		float extensionProgress = GetGreatswordComboDashExtensionProgress();
		float visibleLength = MathF.Max(28f, length);
		float rootDistance = MathF.Min(42f, visibleLength * 0.56f);
		int shardCount = (int)MathHelper.Clamp(visibleLength / 5.6f, 10f, 188f);
		int microShardCount = (int)MathHelper.Clamp(visibleLength / 5.4f, 8f, 168f);
		Texture2D pixel = TextureAssets.MagicPixel.Value;
		for (int i = 0; i < shardCount; i++)
		{
			float progress = ((float)i + 0.5f) / (float)shardCount;
			Vector2 vector = base.Projectile.Center + direction * MathHelper.Lerp(rootDistance, visibleLength, progress);
			float breathRate = MathHelper.Lerp(0.045f, 0.135f, MoDaoQianRenShardVisuals.Random01(i * 31 + 5));
			float shimmerRate = MathHelper.Lerp(0.22f, 0.68f, MoDaoQianRenShardVisuals.Random01(i * 47 + 17));
			float breath = 0.5f + MathF.Sin(Timer * breathRate + (float)i * 0.61f) * 0.5f;
			float shimmer = MoDaoQianRenShardVisuals.Flicker(Timer, i + 37, shimmerRate * 0.45f, shimmerRate);
			float scatter = MathHelper.Lerp(1.4f, 10f + distanceFactor * 5.4f, MathF.Pow(progress, 0.78f));
			float extensionSnap = MathHelper.Lerp(10f, 0f, extensionProgress) * MathHelper.Lerp(0.25f, 1f, progress);
			float sideDrift = MathF.Sin(Timer * MathHelper.Lerp(0.06f, 0.18f, MoDaoQianRenShardVisuals.Random01(i * 23 + 3)) + (float)i * 2.17f) * scatter;
			float lengthDrift = MathF.Cos(Timer * MathHelper.Lerp(0.05f, 0.14f, MoDaoQianRenShardVisuals.Random01(i * 29 + 11)) + (float)i * 1.11f) * MathHelper.Lerp(0.35f, 4.6f, progress);
			Vector2 shardPosition = vector + normal * sideDrift + direction * (lengthDrift - extensionSnap);
			float shardScale = MathHelper.Lerp(0.082f, 0.05f, progress) * base.Projectile.scale * MathHelper.Lerp(0.86f, 1.18f, breath) * MathHelper.Lerp(0.92f, 1.14f, shimmer);
			float shardRotation = rotation + MathF.Sin(Timer * MathHelper.Lerp(0.09f, 0.21f, MoDaoQianRenShardVisuals.Random01(i * 59 + 7)) + (float)i * 0.83f) * MathHelper.Lerp(0.18f, 0.62f, progress) + MathF.Sin((float)i * 1.37f) * 0.26f;
			float glowPower = MathHelper.Clamp(0.22f + shimmer * 0.62f + distanceFactor * 0.28f, 0f, 1f);
			Color outline = Color.Lerp(new Color(104, 24, 230), new Color(224, 58, 255), shimmer) * (0.45f + glowPower * 0.22f);
			Color shardColor = Color.Lerp(new Color(190, 92, 255), new Color(255, 226, 255), 0.18f + shimmer * 0.48f);
			shardColor = Color.Lerp(shardColor, new Color(170, 80, 255), progress * 0.24f);
			Color flashColor = Color.Lerp(new Color(255, 150, 255), Color.White, shimmer * 0.75f);
			MoDaoQianRenShardVisuals.DrawOutlinedShard(shardTexture, shardPosition - Main.screenPosition, i + 131, outline, shardColor, flashColor, shardRotation, shardScale, shimmer);
			if (shimmer > 0.9f && i % 6 == (int)(Timer / 4f) % 6)
			{
				DrawShardSpark(pixel, shardPosition, shardRotation, progress, shimmer);
			}
			if (i % 9 == 0)
			{
				MoDaoQianRenWarmupSystem.AddLight(shardPosition, 0.12f + glowPower * 0.12f, 0.02f, 0.26f + glowPower * 0.18f);
			}
		}
		for (int j = 0; j < microShardCount; j++)
		{
			float progress2 = ((float)j + MoDaoQianRenShardVisuals.Random01(j * 17 + 5)) / (float)microShardCount;
			progress2 = MathHelper.Clamp(progress2, 0.02f, 0.995f);
			float lateralSeed = MoDaoQianRenShardVisuals.Random01(j * 41 + 19) * 2f - 1f;
			float edgeSide = ((j % 3 == 0) ? 0f : ((j % 2 == 0) ? 1f : (-1f)));
			bool num = edgeSide == 0f;
			float edgeWidth = MathHelper.Lerp(3.5f, 15f + distanceFactor * 6f, MathF.Pow(progress2, 0.72f));
			float sideOffset = (num ? (lateralSeed * edgeWidth * 0.36f) : (edgeSide * edgeWidth + lateralSeed * MathHelper.Lerp(1.1f, 4.2f, progress2)));
			Vector2 shardPosition2 = base.Projectile.Center + direction * MathHelper.Lerp(rootDistance + 8f, visibleLength, progress2) + normal * (sideOffset + MathF.Sin(Timer * 0.11f + (float)j * 1.9f) * MathHelper.Lerp(0.35f, 1.75f, progress2)) + direction * ((MoDaoQianRenShardVisuals.Random01(j * 73 + 29) - 0.5f) * MathHelper.Lerp(0.8f, 5.4f, progress2) + MathF.Sin(Timer * 0.07f + (float)j * 1.27f) * MathHelper.Lerp(0.4f, 2.6f, progress2) - MathHelper.Lerp(12f, 0f, extensionProgress) * MathHelper.Lerp(0.2f, 1f, progress2));
			float flicker = MoDaoQianRenShardVisuals.Flicker(Timer, j + 509, 0.09f, 0.82f);
			float sizeSeed = MoDaoQianRenShardVisuals.Random01(j * 67 + 31);
			float scale = MathHelper.Lerp(0.037f, 0.019f, progress2) * MathHelper.Lerp(0.72f, 1.18f, sizeSeed) * MathHelper.Lerp(0.82f, 1.34f, flicker) * base.Projectile.scale;
			float shardRotation2 = rotation + edgeSide * MathHelper.Lerp(0.22f, 0.72f, progress2) + lateralSeed * MathHelper.Lerp(0.18f, 0.56f, progress2) + MathF.Sin(Timer * 0.2f + (float)j) * 0.18f;
			Color outline2 = new Color(146, 34, 255) * (0.22f + flicker * 0.2f);
			Color core = Color.Lerp(new Color(190, 76, 255), Color.White, flicker * 0.62f) * MathHelper.Lerp(0.54f, 0.92f, progress2);
			Color flash = Color.Lerp(new Color(250, 145, 255), Color.White, flicker * 0.78f);
			MoDaoQianRenShardVisuals.DrawOutlinedShard(shardTexture, shardPosition2 - Main.screenPosition, j + 701, outline2, core, flash, shardRotation2, scale, flicker);
		}
		DrawHandle(hiltTexture, drawPosition, rotation, effects, base.Projectile.scale);
	}

	private void DrawGreatswordMicroShardBlade(Texture2D hiltTexture, Vector2 drawPosition, Vector2 direction, Vector2 normal, float length, float distanceFactor, float rotation, SpriteEffects effects)
	{
		Texture2D shardTexture = MoDaoQianRenShardVisuals.Texture;
		float rootDistance = 45.937145f * base.Projectile.scale;
		float bladeLength = MathF.Max(36f, length - rootDistance);
		float chargePower = ((IsGreatswordBurstCharge && base.Projectile.owner >= 0 && base.Projectile.owner < 255) ? GetGreatswordBurstChargePower(Main.player[base.Projectile.owner].GetModPlayer<MoDaoQianRenPlayer>()) : 0f);
		float chargeDensity = MathHelper.Lerp(1f, 1.28f, chargePower);
		int coreShardCount = (int)MathHelper.Clamp(bladeLength / 3.1f * chargeDensity, 130f, 430f);
		int looseShardCount = (int)MathHelper.Clamp(bladeLength / 6.2f * chargeDensity, 45f, 155f);
		DrawGreatswordRangeAura(direction, normal, rootDistance, bladeLength, distanceFactor);
		for (int i = 0; i < coreShardCount; i++)
		{
			float progress = MathF.Pow(((float)i + MoDaoQianRenShardVisuals.Random01(i * 101 + 1701)) / (float)coreShardCount, 1.68f);
			progress = MathHelper.Clamp(progress, 0.01f, 0.995f);
			float rootAuraFade = SmoothStep(Utils.GetLerpValue(0.14f, 0.28f, progress, clamped: true));
			float halfWidth = GetGreatswordBladeHalfWidth(progress, distanceFactor);
			float sideSeed = MoDaoQianRenShardVisuals.Random01(i * 73 + 19) * 2f - 1f;
			float sideOffset = (float)MathF.Sign(sideSeed) * MathF.Pow(MathF.Abs(sideSeed), 0.72f) * halfWidth * MathF.Sqrt(MoDaoQianRenShardVisuals.Random01(i * 59 + 29));
			float alongJitter = MathHelper.Lerp(-5.2f, 5.2f, MoDaoQianRenShardVisuals.Random01(i * 43 + 13)) * MathHelper.Lerp(0.45f, 1f, progress);
			float ripple = MathF.Sin(Timer * MathHelper.Lerp(0.075f, 0.18f + chargePower * 0.08f, MoDaoQianRenShardVisuals.Random01(i * 31 + 7)) + (float)i * 1.83f) * MathHelper.Lerp(0.45f, 2.8f + chargePower * 2.6f, progress);
			float flicker = MoDaoQianRenShardVisuals.Flicker(Timer, i + 2500, 0.06f, 0.78f);
			Vector2 shardPosition = base.Projectile.Center + direction * (rootDistance + bladeLength * progress + alongJitter) + normal * (sideOffset + ripple);
			float densityScale = MathHelper.Lerp(1.08f, 0.48f, progress);
			float shardScale = MathHelper.Lerp(0.064f, 0.022f, progress) * MathHelper.Lerp(0.72f, 1.16f, MoDaoQianRenShardVisuals.Random01(i * 47 + 5)) * MathHelper.Lerp(0.82f, 1.24f, flicker) * densityScale * base.Projectile.scale;
			float shardRotation = rotation + (MoDaoQianRenShardVisuals.Random01(i * 37 + 31) - 0.5f) * MathHelper.Lerp(0.42f, 1.1f, progress) + MathF.Sin(Timer * 0.14f + (float)i) * 0.16f;
			float auraStrength = MathHelper.Lerp(0.025f, MathHelper.Lerp(0.2f, 0.07f, progress), rootAuraFade);
			Color aura = new Color(126, 18, 255) * (auraStrength * MathHelper.Lerp(1f, 1.7f, chargePower));
			Color core = Color.Lerp(new Color(198, 76, 255), Color.White, 0.2f + flicker * 0.45f) * MathHelper.Lerp(0.78f, 0.38f, progress) * MathHelper.Lerp(1f, 1.34f, chargePower);
			Color flash = Color.Lerp(new Color(255, 145, 255), Color.White, flicker * 0.72f);
			DrawMicroShard(shardTexture, shardPosition - Main.screenPosition, i + 2600, aura, core, flash, shardRotation, shardScale, flicker);
			if (IsGreatswordBurstCharge && i % 12 == 0 && chargePower > 0.2f)
			{
				DrawPrettyStarSparkle(chargePower * (0.16f + flicker * 0.1f), SpriteEffects.None, shardPosition - Main.screenPosition, Color.White * chargePower, new Color(248, 126, 255), flicker, 0f, 0.25f, 0.82f, 1f, shardRotation, new Vector2(0.28f, 0.9f + chargePower * 0.9f) * base.Projectile.scale, new Vector2(0.5f, 0.24f) * base.Projectile.scale);
			}
		}
		for (int j = 0; j < looseShardCount; j++)
		{
			float progress2 = MathF.Pow(((float)j + MoDaoQianRenShardVisuals.Random01(j * 29 + 503)) / (float)looseShardCount, 1.92f);
			progress2 = MathHelper.Clamp(progress2, 0.015f, 0.995f);
			float halfWidth2 = GetGreatswordBladeHalfWidth(progress2, distanceFactor);
			float sideSeed2 = MoDaoQianRenShardVisuals.Random01(j * 53 + 11) * 2f - 1f;
			float sideOffset2 = sideSeed2 * halfWidth2 * MathHelper.Lerp(0.92f, 1.42f, MoDaoQianRenShardVisuals.Random01(j * 71 + 13));
			float trailBack = MathHelper.Lerp(-10f, 5.5f, MoDaoQianRenShardVisuals.Random01(j * 89 + 43)) * MathHelper.Lerp(0.3f, 1f, progress2);
			Vector2 shardPosition2 = base.Projectile.Center + direction * (rootDistance + bladeLength * progress2 + trailBack) + normal * (sideOffset2 + MathF.Sin(Timer * (0.1f + chargePower * 0.07f) + (float)j * 1.31f) * MathHelper.Lerp(0.6f, 3.8f + chargePower * 3f, progress2));
			float flicker2 = MoDaoQianRenShardVisuals.Flicker(Timer, j + 4300, 0.08f, 0.72f);
			float shardScale2 = MathHelper.Lerp(0.044f, 0.014f, progress2) * MathHelper.Lerp(0.72f, 1.08f, MoDaoQianRenShardVisuals.Random01(j * 37 + 19)) * MathHelper.Lerp(0.72f, 1.18f, flicker2) * base.Projectile.scale;
			float shardRotation2 = rotation + sideSeed2 * MathHelper.Lerp(0.3f, 0.95f, progress2) + MathF.Sin(Timer * 0.17f + (float)j * 0.7f) * 0.2f;
			DrawMicroShard(shardTexture, shardPosition2 - Main.screenPosition, j + 4200, new Color(104, 10, 235) * (MathHelper.Lerp(0.14f, 0.04f, progress2) * MathHelper.Lerp(1f, 1.55f, chargePower)), Color.Lerp(new Color(174, 58, 255), new Color(255, 210, 255), flicker2 * 0.44f) * MathHelper.Lerp(0.46f, 0.18f, progress2) * MathHelper.Lerp(1f, 1.28f, chargePower), Color.Lerp(new Color(255, 124, 255), Color.White, flicker2 * 0.72f), shardRotation2, shardScale2, flicker2);
		}
		DrawHandle(hiltTexture, drawPosition, rotation, effects, base.Projectile.scale);
	}

	private void DrawGreatswordRangeAura(Vector2 direction, Vector2 normal, float rootDistance, float bladeLength, float distanceFactor)
	{
		if (!IsGreatswordBladeForm || base.Projectile.owner < 0 || base.Projectile.owner >= 255)
		{
			return;
		}
		Player player = Main.player[base.Projectile.owner];
		int duration = GetAttackDuration(player);
		float attackProgress = Utils.GetLerpValue(0f, duration, Timer, clamped: true);
		float num = (IsGreatswordBurstCharge ? GetGreatswordBurstChargePower(player.GetModPlayer<MoDaoQianRenPlayer>()) : (IsGreatswordBurst ? GetGreatswordBurstPower(attackProgress) : GetGreatswordSlashPower(attackProgress)));
		float earlyReadability = Utils.GetLerpValue(0.12f, 0.28f, attackProgress, clamped: true) * (1f - Utils.GetLerpValue(0.82f, 0.96f, attackProgress, clamped: true));
		float readabilityScale = (IsGreatswordBurstCharge ? 0.5f : (IsGreatswordBurst ? 0.58f : 0.38f));
		float auraPower = MathHelper.Clamp(MathF.Max(num, earlyReadability * readabilityScale), 0f, 1f);
		if (!(auraPower <= 0.018f))
		{
			Vector2 rootPosition = base.Projectile.Center + direction * rootDistance - Main.screenPosition;
			float auraLow = (IsGreatswordBurstCharge ? 0.46f : (IsGreatswordBurst ? 0.5f : 0.38f));
			float auraHigh = (IsGreatswordBurstCharge ? 0.74f : (IsGreatswordBurst ? 0.72f : 0.52f));
			MoDaoQianRenGreatswordFogVisuals.Draw(auraPower: auraPower * MathHelper.Lerp(auraLow, auraHigh, distanceFactor), rootPosition: rootPosition, direction: direction, normal: normal, bladeLength: bladeLength, halfWidthAt: (float progress) => GetGreatswordBladeHalfWidth(progress, distanceFactor), distanceFactor: distanceFactor, timer: Timer, seedBase: 8800);
		}
	}

	private void UpdateGreatswordSlashArcLock(Player player)
	{
		if (!IsGreatswordSwing)
		{
			greatswordSlashArcLocked = false;
		}
		else if (!greatswordSlashArcLocked)
		{
			int duration = GetAttackDuration(player);
			float lerpValue = Utils.GetLerpValue(0f, duration, Timer, clamped: true);
			float releaseProgress = (IsGreatswordHeavySlash ? 0.39f : 0.36f);
			if (!(lerpValue < releaseProgress))
			{
				float distanceFactor = GetDistanceFactor(TargetBladeLength);
				Vector2 aimDirection = BaseRotation.ToRotationVector2().SafeNormalize(Vector2.UnitX * player.direction);
				GetGreatswordSlashArcGeometry(player, aimDirection, out var circleCenter, out var outerRadius);
				greatswordSlashArcCenter = circleCenter;
				greatswordSlashArcRotation = aimDirection.ToRotation();
				greatswordSlashArcOuterRadius = outerRadius;
				greatswordSlashArcDistanceFactor = distanceFactor;
				greatswordSlashArcLockTimer = Timer;
				greatswordSlashArcLocked = true;
			}
		}
	}

	private void GetGreatswordSlashArcGeometry(Player player, Vector2 aimDirection, out Vector2 circleCenter, out float outerRadius)
	{
		int samples = (IsGreatswordHeavySlash ? 29 : 25);
		Vector2 previousTip = Vector2.Zero;
		Vector2 midpoint = base.Projectile.Center + aimDirection * GetVisualBladeLength();
		float sweptLength = 0f;
		float halfLength = 0f;
		bool midpointFound = false;
		Vector2 start;
		for (int i = 0; i < samples; i++)
		{
			float sampleT = ((samples <= 1) ? 1f : ((float)i / (float)(samples - 1)));
			float sampleProgress = MathHelper.Lerp(0.2f, 0.78f, sampleT);
			GetGreatswordSlashArcBladeLineAtProgress(player, sampleProgress, aimDirection.ToRotation(), out start, out var tip);
			if (i > 0)
			{
				sweptLength += Vector2.Distance(previousTip, tip);
			}
			previousTip = tip;
		}
		halfLength = sweptLength * 0.5f;
		float traversed = 0f;
		previousTip = Vector2.Zero;
		for (int j = 0; j < samples; j++)
		{
			float sampleT2 = ((samples <= 1) ? 1f : ((float)j / (float)(samples - 1)));
			float sampleProgress2 = MathHelper.Lerp(0.2f, 0.78f, sampleT2);
			GetGreatswordSlashArcBladeLineAtProgress(player, sampleProgress2, aimDirection.ToRotation(), out start, out var tip2);
			if (j > 0)
			{
				float segmentLength = Vector2.Distance(previousTip, tip2);
				if (!midpointFound && traversed + segmentLength >= halfLength)
				{
					float segmentT = ((segmentLength <= 0.001f) ? 0f : ((halfLength - traversed) / segmentLength));
					midpoint = Vector2.Lerp(previousTip, tip2, MathHelper.Clamp(segmentT, 0f, 1f));
					midpointFound = true;
					break;
				}
				traversed += segmentLength;
			}
			previousTip = tip2;
		}
		outerRadius = MathF.Max(72f, sweptLength / 2.48f);
		circleCenter = midpoint - aimDirection * outerRadius;
	}

	private void DrawGreatswordSlashArc(Player player, bool foreground)
	{
		if (!IsGreatswordSwing || Main.dedServ || !greatswordSlashArcLocked)
		{
			return;
		}
		float arcAge = MathF.Max(0f, Timer - greatswordSlashArcLockTimer);
		float arcLife = (IsGreatswordHeavySlash ? 18f : 14f);
		float num = SmoothStep(Utils.GetLerpValue(0f, 2.4f, arcAge + 1f, clamped: true));
		float fadeOut = 1f - SmoothStep(Utils.GetLerpValue(arcLife * 0.5f, arcLife, arcAge, clamped: true));
		float slashPower = MathHelper.Clamp(num * fadeOut * (IsGreatswordHeavySlash ? 1.08f : 0.88f), 0f, 1f);
		if (!(slashPower <= (foreground ? 0.035f : 0.02f)))
		{
			MoDaoQianRenGreatswordSlashVisuals.Draw(greatswordSlashArcCenter - Main.screenPosition, greatswordSlashArcRotation, greatswordSlashArcOuterRadius, greatswordSlashArcDistanceFactor, slashPower, arcAge, IsGreatswordHeavySlash, IsGreatswordReverseSwing, foreground);
			if (foreground)
			{
				DrawGreatswordSlashArcSparks(greatswordSlashArcCenter, greatswordSlashArcRotation, greatswordSlashArcOuterRadius, greatswordSlashArcDistanceFactor, slashPower, arcAge / arcLife);
			}
		}
	}

	private void DrawGreatswordDevilsSlashArc(Player player, bool foreground)
	{
		return;
	}

	private void GetGreatswordDevilsSlashArcGeometry(Player player, Vector2 aimDirection, out Vector2 circleCenter, out float outerRadius)
	{
		int samples = 24;
		Vector2 min = new Vector2(float.MaxValue);
		Vector2 max = new Vector2(float.MinValue);
		for (int i = 0; i < samples; i++)
		{
			float sample = (samples <= 1) ? 1f : (float)i / (float)(samples - 1);
			GetGreatswordDevilsBladeLine(player, out var start, out var tip);
			min = Vector2.Min(min, Vector2.Min(start, tip));
			max = Vector2.Max(max, Vector2.Max(start, tip));
		}
		circleCenter = (min + max) * 0.5f;
		outerRadius = MathF.Max(TargetBladeLength * 0.62f, (max - min).Length() * 0.5f);
	}

	private void DrawGreatswordSlashArcSparks(Vector2 centerWorld, float rotation, float bladeLength, float distanceFactor, float slashPower, float attackProgress)
	{
		int sparkleCount = (IsGreatswordHeavySlash ? Math.Max(6, 7) : Math.Max(4, 5));
		float swingSign = (IsGreatswordReverseSwing ? (-1f) : 1f);
		float scale = MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f);
		for (int i = 0; i < sparkleCount; i++)
		{
			float factor = ((sparkleCount <= 1) ? 1f : ((float)i / (float)(sparkleCount - 1)));
			float seed = MoDaoQianRenShardVisuals.Random01(i * 739 + (IsGreatswordHeavySlash ? 9101 : 8501));
			float angle = MathHelper.Lerp(-0.76f, 0.92f, factor) * swingSign;
			float radius = bladeLength * MathHelper.Lerp(0.62f, 0.98f, MathF.Pow(factor, 0.72f));
			float flicker = MoDaoQianRenShardVisuals.Flicker(Timer, i + 9300, 0.08f, 0.62f);
			float alpha = slashPower * MathHelper.Lerp(0.18f, IsGreatswordHeavySlash ? 0.58f : 0.42f, factor) * MathHelper.Lerp(0.72f, 1.12f, flicker);
			if (!(alpha <= 0.055f))
			{
				Vector2 radial = (rotation + angle).ToRotationVector2();
				Vector2 tangent = radial.RotatedBy(swingSign * ((float)Math.PI / 2f));
				Vector2 position = centerWorld + radial * radius + tangent * MathHelper.Lerp(-10f, 10f, seed) * MathHelper.Lerp(0.4f, 1.2f, distanceFactor);
				Color shine = Color.Lerp(new Color(225, 104, 255), Color.White, 0.3f + flicker * 0.48f);
				Color glow = (IsGreatswordHeavySlash ? new Color(255, 74, 232) : new Color(190, 78, 255));
				DrawPrettyStarSparkle(alpha, SpriteEffects.None, position - Main.screenPosition, shine * (alpha * 0.82f), glow, attackProgress, 0.08f, 0.32f, 0.78f, 1f, radial.ToRotation() + (float)Math.PI / 2f, new Vector2(MathHelper.Lerp(0.62f, IsGreatswordHeavySlash ? 1.24f : 0.92f, factor), MathHelper.Lerp(1.8f, IsGreatswordHeavySlash ? 4.8f : 3.3f, factor)) * scale, new Vector2(MathHelper.Lerp(0.82f, 1.35f, factor), 0.58f) * scale);
			}
		}
	}

	private void DrawGreatswordComboRingSlash(Player player, bool foreground)
	{
		if (!IsGreatswordComboRingSlash || Main.dedServ)
		{
			return;
		}
		Texture2D pixel = TextureAssets.MagicPixel.Value;
		Vector2 center = player.Center;
		float radius = GetGreatswordComboRingSlashRadius();
		float distanceFactor = GetDistanceFactor(radius);
		float progress = GetGreatswordComboRingSlashProgress();
		float sweptAngle = (float)Math.PI * 2f * progress;
		float num = SmoothStep(Utils.GetLerpValue(0f, 0.18f, progress, clamped: true));
		float fadeOut = 1f - SmoothStep(Utils.GetLerpValue(0.82f, 1f, progress, clamped: true));
		float power = MathHelper.Clamp(MathF.Max(num * fadeOut, 0.16f * (1f - progress)), 0f, 1f);
		if (power <= 0.02f)
		{
			return;
		}
		float startRotation = GreatswordComboRingSlashRotation;
		float currentRotation = startRotation + sweptAngle * GreatswordComboRingSlashSpinDirection;
		float scale = MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f);
		if (!foreground)
		{
			int afterimageCount = 7;
			for (int i = 1; i <= afterimageCount; i++)
			{
				float echoT = (float)i / (float)afterimageCount;
				float echoProgress = MathHelper.Clamp(progress - echoT * MathHelper.Lerp(0.045f, 0.12f, distanceFactor), 0f, progress);
				Vector2 echoDirection = (startRotation + (float)Math.PI * 2f * echoProgress * GreatswordComboRingSlashSpinDirection).ToRotationVector2();
				Vector2 echoNormal = echoDirection.RotatedBy(1.5707963705062866);
				float echoPower = power * MathHelper.Lerp(0.28f, 0.06f, echoT);
				MoDaoQianRenGreatswordFogVisuals.Draw(center + echoDirection * 46f * 0.99863356f * scale - Main.screenPosition, echoDirection, echoNormal, radius, (float sample) => GetGreatswordBladeHalfWidth(sample, distanceFactor), distanceFactor, echoPower, Timer + (float)i * 3f, 9700 + i * 113);
			}
			return;
		}
		Vector2 tipRadial = currentRotation.ToRotationVector2();
		Vector2 tangent = tipRadial.RotatedBy(GreatswordComboRingSlashSpinDirection * ((float)Math.PI / 2f));
		Vector2 tipPosition = center + tipRadial * radius;
		Color shine = Color.Lerp(new Color(232, 96, 255), Color.White, 0.54f);
		DrawPrettyStarSparkle(power, SpriteEffects.None, tipPosition - Main.screenPosition, shine * (0.9f * power), new Color(220, 76, 255), progress, 0f, 0.16f, 0.86f, 1f, tangent.ToRotation(), new Vector2(1.15f, 5.2f) * scale, new Vector2(1.45f, 0.72f) * scale);
		for (int i2 = 0; i2 < 12; i2++)
		{
			float factor = Main.rand.NextFloat(MathF.Max(0f, progress - 0.18f), MathF.Max(0.02f, progress));
			Vector2 radial = (startRotation + (float)Math.PI * 2f * factor * GreatswordComboRingSlashSpinDirection).ToRotationVector2();
			Vector2 dustTangent = radial.RotatedBy(GreatswordComboRingSlashSpinDirection * ((float)Math.PI / 2f));
			DrawShardSpark(pixel, center + radial * Main.rand.NextFloat(radius * 0.84f, radius * 1.04f) + dustTangent * Main.rand.NextFloat(-18f, 18f), dustTangent.ToRotation(), factor, power);
		}
	}

	private void DrawModeOneBladeSlash(Player player, bool foreground)
	{
		if (IsShardStream || IsGreatswordSwing || Main.dedServ)
		{
			return;
		}
		int duration = GetAttackDuration(player);
		float progress = Utils.GetLerpValue(0f, duration, Timer, clamped: true);
		float currentAttackPower = GetCurrentAttackPower(progress);
		float earlyGlow = MathHelper.Clamp(Utils.GetLerpValue(0.08f, IsModeOneFinisher ? 0.24f : 0.18f, progress, clamped: true) * (1f - Utils.GetLerpValue(IsModeOneFinisher ? 0.42f : 0.34f, 0.98f, progress, clamped: true)), 0f, 1f);
		float slashPower = MathF.Max(currentAttackPower, earlyGlow * 0.58f);
		if (slashPower <= 0.025f)
		{
			return;
		}
		float distanceFactor = GetDistanceFactor(TargetBladeLength);
		float scale = MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f);
		int samples = (IsModeOneFinisher ? 31 : 22);
		float sweepBack = (IsModeOneFinisher ? MathHelper.Lerp(0.26f, 0.36f, distanceFactor) : MathHelper.Lerp(0.18f, 0.25f, distanceFactor));
		float sweepStart = MathF.Max(0f, progress - sweepBack);
		GetModeOneSlashPalette(out var auraColor, out var bodyColor, out var edgeColor, out var coreColor);
		Color shadowColor = GetModeOneSlashShadowColor();
		Vector2[] innerPoints = new Vector2[samples];
		Vector2[] bodyPoints = new Vector2[samples];
		Vector2[] outerPoints = new Vector2[samples];
		float[] alphas = new float[samples];
		int pointCount = BuildModeOneSlashSamples(player, sweepStart, progress, samples, slashPower, innerPoints, bodyPoints, outerPoints, alphas);
		if (pointCount < 3)
		{
			return;
		}
		if (foreground)
		{
			DrawModeOneOuterRibbon(outerPoints, alphas, pointCount, edgeColor, coreColor, scale);
			DrawModeOneOuterRibbon(bodyPoints, alphas, pointCount, bodyColor, coreColor, scale * 0.54f);
			DrawModeOneTipComet(outerPoints, alphas, pointCount, progress, edgeColor, coreColor, scale);
			DrawModeOneBladeSparkles(outerPoints, alphas, pointCount, progress, edgeColor, coreColor, scale);
			DrawModeOneFractureSparkles(bodyPoints, outerPoints, alphas, pointCount, progress, edgeColor, coreColor, scale);
			return;
		}
		DrawModeOneEchoRibbons(bodyPoints, outerPoints, alphas, pointCount, auraColor, edgeColor, scale);
		DrawModeOneArcBand(innerPoints, LerpModeOnePointArrays(bodyPoints, outerPoints, pointCount, IsModeOneFinisher ? 0.4f : 0.32f), alphas, pointCount, shadowColor, auraColor, IsModeOneFinisher ? 0.18f : 0.12f, IsModeOneFinisher ? 0.36f : 0.26f, additive: false);
		DrawModeOneArcBand(innerPoints, outerPoints, alphas, pointCount, auraColor, bodyColor, IsModeOneFinisher ? 0.12f : 0.08f, IsModeOneFinisher ? 0.46f : 0.32f, additive: true);
		DrawModeOneArcBand(bodyPoints, outerPoints, alphas, pointCount, bodyColor, edgeColor, IsModeOneFinisher ? 0.21f : 0.15f, IsModeOneFinisher ? 0.72f : 0.52f, additive: false);
		DrawModeOneArcBand(LerpModeOnePointArrays(bodyPoints, outerPoints, pointCount, IsModeOneFinisher ? 0.48f : 0.58f), outerPoints, alphas, pointCount, edgeColor, coreColor, IsModeOneFinisher ? 0.35f : 0.24f, IsModeOneFinisher ? 0.96f : 0.76f, additive: true);
		if (IsModeOneFinisher)
		{
			DrawModeOneFinisherBloom(innerPoints, outerPoints, alphas, pointCount, bodyColor, coreColor);
			DrawModeOneFinisherShock(innerPoints, outerPoints, alphas, pointCount, progress, bodyColor, coreColor, scale);
		}
	}

	private int BuildModeOneSlashSamples(Player player, float sweepStart, float progress, int samples, float slashPower, Vector2[] innerPoints, Vector2[] bodyPoints, Vector2[] outerPoints, float[] alphas)
	{
		int count = 0;
		float innerFactor = (IsModeOneFinisher ? 0.18f : 0.28f);
		float bodyFactor = (IsModeOneFinisher ? 0.54f : 0.6f);
		float outerFactor = (IsModeOneFinisher ? 1.07f : 1.01f);
		for (int i = 0; i < samples; i++)
		{
			float sampleT = ((samples <= 1) ? 1f : ((float)i / (float)(samples - 1)));
			float sampleProgress = MathHelper.Lerp(sweepStart, progress, sampleT);
			if (IsDamageWindowActive(sampleProgress) || !(sampleProgress < 0.14f))
			{
				GetBladeLineAtProgress(player, sampleProgress, out var start, out var end);
				Vector2 direction = (end - start).SafeNormalize(base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction));
				float distanceFactor = GetDistanceFactor(TargetBladeLength);
				float leadingPush = MathHelper.Lerp(3f, IsModeOneFinisher ? 22f : 12f, distanceFactor) * MathF.Pow(sampleT, 1.7f) * base.Projectile.scale;
				Vector2 outer = Vector2.Lerp(start, end, outerFactor) + direction * leadingPush;
				Vector2 body = Vector2.Lerp(start, end, bodyFactor);
				Vector2 inner = Vector2.Lerp(start, end, innerFactor);
				float tailFade = SmoothStep(sampleT);
				float frontTaper = MathHelper.Lerp(1f, 0.84f, SmoothStep(Utils.GetLerpValue(0.86f, 1f, sampleT, clamped: true)));
				float breathing = 0.94f + MathF.Sin(Timer * 0.21f + sampleT * ((float)Math.PI * 2f)) * 0.06f;
				float damageWindow = (IsDamageWindowActive(sampleProgress) ? 1f : 0.58f);
				float ultimateWeight = (IsModeOneFinisher ? 1.18f : 1.08f);
				float alpha = MathHelper.Clamp(slashPower * tailFade * frontTaper * breathing * damageWindow * ultimateWeight, 0f, 1f);
				innerPoints[count] = inner;
				bodyPoints[count] = body;
				outerPoints[count] = outer;
				alphas[count] = alpha;
				count++;
			}
		}
		return count;
	}

	private static Vector2[] LerpModeOnePointArrays(Vector2[] from, Vector2[] to, int count, float amount)
	{
		Vector2[] result = new Vector2[count];
		for (int i = 0; i < count; i++)
		{
			result[i] = Vector2.Lerp(from[i], to[i], amount);
		}
		return result;
	}

	private static void DrawModeOneArcBand(Vector2[] innerPoints, Vector2[] outerPoints, float[] alphas, int count, Color innerColor, Color outerColor, float innerOpacity, float outerOpacity, bool additive)
	{
		if (count < 2)
		{
			return;
		}
		Texture2D slashTexture = TextureAssets.Extra[98].Value;
		int railCount = (additive ? 6 : 5);
		for (int rail = 0; rail < railCount; rail++)
		{
			float railT = ((railCount <= 1) ? 1f : ((float)rail / (float)(railCount - 1)));
			float edgeBias = SmoothStep(railT);
			float opacity = MathHelper.Lerp(innerOpacity, outerOpacity, edgeBias);
			Color railColor = Color.Lerp(innerColor, outerColor, edgeBias);
			for (int i = 1; i < count; i++)
			{
				float progress = ((float)i - 0.5f) / (float)(count - 1);
				float alpha = (alphas[i - 1] + alphas[i]) * 0.5f;
				if (!(alpha <= 0.015f))
				{
					Vector2 start = Vector2.Lerp(innerPoints[i - 1], outerPoints[i - 1], railT);
					Vector2 end = Vector2.Lerp(innerPoints[i], outerPoints[i], railT);
					float bandWidth = (Vector2.Distance(innerPoints[i - 1], outerPoints[i - 1]) + Vector2.Distance(innerPoints[i], outerPoints[i])) * 0.5f;
					float width = MathHelper.Lerp(MathHelper.Clamp(bandWidth * 0.09f, 10f, 26f), MathHelper.Clamp(bandWidth * 0.032f, 4f, 11f), edgeBias);
					width *= MathHelper.Lerp(0.82f, 1.28f, SmoothStep(progress));
					DrawModeOneSoftTrailSegment(slashTexture, start, end, railColor, width, alpha * opacity, additive ? 0f : 0.34f);
				}
			}
		}
	}

	private void DrawModeOneOuterRibbon(Vector2[] points, float[] alphas, int count, Color edgeColor, Color coreColor, float scale)
	{
		if (count < 2)
		{
			return;
		}
		Texture2D slashTexture = TextureAssets.Extra[98].Value;
		float baseWidth = (IsModeOneFinisher ? 9.4f : 6.2f) * scale;
		for (int i = 1; i < count; i++)
		{
			float progress = ((float)i - 0.5f) / (float)(count - 1);
			float alpha = (alphas[i - 1] + alphas[i]) * 0.5f * MathHelper.Lerp(0.24f, 0.94f, SmoothStep(progress));
			if (!(alpha <= 0.02f))
			{
				float widthPulse = MathHelper.Lerp(0.72f, 1.18f, SmoothStep(progress));
				Color edge = Color.Lerp(edgeColor, coreColor, MathF.Pow(progress, 1.85f));
				DrawModeOneSoftTrailSegment(slashTexture, points[i - 1], points[i], edgeColor, baseWidth * 4.75f * widthPulse, alpha * 0.34f, 0f);
				DrawModeOneSoftTrailSegment(slashTexture, points[i - 1], points[i], edge, baseWidth * 2.35f * widthPulse, alpha * 0.88f, 0f);
				DrawModeOneSoftTrailSegment(slashTexture, points[i - 1], points[i], coreColor, baseWidth * 0.86f * widthPulse, alpha, 0f);
			}
		}
	}

	private void DrawModeOneFinisherBloom(Vector2[] innerPoints, Vector2[] outerPoints, float[] alphas, int count, Color bodyColor, Color coreColor)
	{
		Vector2[] innerPoints2 = LerpModeOnePointArrays(innerPoints, outerPoints, count, 0.34f);
		Vector2[] bloomOuter = LerpModeOnePointArrays(innerPoints, outerPoints, count, 0.94f);
		DrawModeOneArcBand(innerPoints2, bloomOuter, alphas, count, bodyColor, coreColor, 0.2f, 0.66f, additive: true);
	}

	private void DrawModeOneEchoRibbons(Vector2[] bodyPoints, Vector2[] outerPoints, float[] alphas, int count, Color auraColor, Color edgeColor, float scale)
	{
		if (count < 2)
		{
			return;
		}
		Texture2D slashTexture = TextureAssets.Extra[98].Value;
		int echoCount = (IsModeOneFinisher ? 4 : 3);
		for (int echo = 0; echo < echoCount; echo++)
		{
			float echoT = ((echoCount <= 1) ? 0f : ((float)echo / (float)(echoCount - 1)));
			float railT = MathHelper.Lerp(0.34f, 0.86f, echoT);
			Color echoColor = Color.Lerp(auraColor, edgeColor, MathHelper.Lerp(0.26f, 0.58f, echoT));
			float echoOpacity = (IsModeOneFinisher ? 0.26f : 0.17f) * MathHelper.Lerp(1f, 0.58f, echoT);
			for (int i = 1; i < count; i++)
			{
				float progress = ((float)i - 0.5f) / (float)(count - 1);
				float alpha = (alphas[i - 1] + alphas[i]) * 0.5f;
				if (!(alpha <= 0.025f))
				{
					Vector2 start = Vector2.Lerp(bodyPoints[i - 1], outerPoints[i - 1], railT);
					Vector2 end = Vector2.Lerp(bodyPoints[i], outerPoints[i], railT);
					Vector2 vector = (end - start).SafeNormalize(GetModeOneTrailTangent(outerPoints, count, i));
					Vector2 normal = vector.RotatedBy(1.5707963705062866);
					float drift = MathF.Sin(Timer * 0.16f + (float)i * 0.71f + (float)echo * 1.9f) * MathHelper.Lerp(2.5f, IsModeOneFinisher ? 9f : 6f, progress) * scale;
					Vector2 offset = -vector * MathHelper.Lerp(7f, IsModeOneFinisher ? 24f : 16f, progress) * ((float)echo + 1f) * 0.72f * scale + normal * drift;
					float width = MathHelper.Lerp(IsModeOneFinisher ? 20f : 14f, IsModeOneFinisher ? 9f : 6f, echoT) * MathHelper.Lerp(0.76f, 1.18f, SmoothStep(progress)) * scale;
					DrawModeOneSoftTrailSegment(slashTexture, start + offset, end + offset, echoColor, width, alpha * echoOpacity, 0f);
				}
			}
		}
	}

	private void DrawModeOneTipComet(Vector2[] outerPoints, float[] alphas, int count, float progress, Color edgeColor, Color coreColor, float scale)
	{
		if (count >= 2)
		{
			int tipIndex = count - 1;
			float alpha = alphas[tipIndex];
			if (!(alpha <= 0.035f))
			{
				Texture2D value = TextureAssets.Extra[98].Value;
				Vector2 tip = outerPoints[tipIndex];
				Vector2 tangent = GetModeOneTrailTangent(outerPoints, count, tipIndex);
				float cometLength = MathHelper.Lerp(amount: GetDistanceFactor(TargetBladeLength), value1: IsModeOneFinisher ? 42f : 28f, value2: IsModeOneFinisher ? 98f : 62f) * scale;
				Vector2 nose = tip + tangent * cometLength * 0.16f;
				Vector2 tail = tip - tangent * cometLength;
				DrawModeOneSoftTrailSegment(value, tail, nose, edgeColor, (IsModeOneFinisher ? 18f : 12f) * scale, alpha * (IsModeOneFinisher ? 0.86f : 0.62f), 0f);
				DrawModeOneSoftTrailSegment(value, Vector2.Lerp(tail, tip, 0.42f), nose, coreColor, (IsModeOneFinisher ? 8.8f : 5.8f) * scale, alpha, 0f);
				DrawPrettyStarSparkle(alpha * (IsModeOneFinisher ? 1f : 0.9f), SpriteEffects.None, tip - Main.screenPosition, Color.White * (alpha * 0.92f), edgeColor, progress, 0f, 0.3f, 0.86f, 1f, tangent.ToRotation(), new Vector2(IsModeOneFinisher ? 1.18f : 0.82f, IsModeOneFinisher ? 3.8f : 2.45f) * scale, new Vector2(IsModeOneFinisher ? 1.42f : 1.02f, 0.66f) * scale);
			}
		}
	}

	private void DrawModeOneFractureSparkles(Vector2[] bodyPoints, Vector2[] outerPoints, float[] alphas, int count, float progress, Color edgeColor, Color coreColor, float scale)
	{
		if (count < 3)
		{
			return;
		}
		int sparkleCount = (IsModeOneFinisher ? 8 : 5);
		for (int i = 0; i < sparkleCount; i++)
		{
			float factor = ((float)i + 0.5f) / (float)sparkleCount;
			int index = Math.Clamp((int)MathF.Round(MathHelper.Lerp((float)count * 0.18f, (float)count - 1f, factor)), 0, count - 1);
			float alpha = alphas[index] * MathHelper.Lerp(0.24f, IsModeOneFinisher ? 0.68f : 0.48f, factor);
			if (!(alpha <= 0.045f))
			{
				Vector2 tangent = GetModeOneTrailTangent(outerPoints, count, index);
				Vector2 normal = tangent.RotatedBy(1.5707963705062866);
				float side = ((i % 2 == 0) ? 1f : (-1f));
				float drift = side * MathHelper.Lerp(7f, IsModeOneFinisher ? 30f : 20f, factor) * scale;
				Vector2 position = Vector2.Lerp(bodyPoints[index], outerPoints[index], MathHelper.Lerp(0.48f, 0.93f, factor)) + normal * drift - tangent * MathF.Sin(Timer * 0.2f + (float)i * 1.37f) * MathHelper.Lerp(1.5f, 8f, factor) * scale;
				DrawPrettyStarSparkle(alpha, SpriteEffects.None, position - Main.screenPosition, coreColor * (alpha * 0.56f), Color.Lerp(edgeColor, coreColor, 0.1f + factor * 0.24f), progress, 0f, 0.36f, 0.88f, 1f, tangent.ToRotation() + side * MathHelper.Lerp(0.32f, 0.82f, factor), new Vector2(MathHelper.Lerp(0.28f, IsModeOneFinisher ? 0.72f : 0.5f, factor), MathHelper.Lerp(0.78f, IsModeOneFinisher ? 2.05f : 1.38f, factor)) * scale, new Vector2(0.72f, 0.42f) * scale);
			}
		}
	}

	private void DrawModeOneFinisherShock(Vector2[] innerPoints, Vector2[] outerPoints, float[] alphas, int count, float progress, Color bodyColor, Color coreColor, float scale)
	{
		if (count >= 3)
		{
			int index = Math.Clamp((int)MathF.Round((float)count * 0.74f), 1, count - 1);
			float alpha = alphas[index] * Utils.GetLerpValue(0.2f, 0.72f, progress, clamped: true);
			if (!(alpha <= 0.04f))
			{
				Vector2 tangent = GetModeOneTrailTangent(outerPoints, count, index);
				Vector2 position = Vector2.Lerp(innerPoints[index], outerPoints[index], 0.78f) - Main.screenPosition;
				DrawPrettyStarSparkle(alpha * 0.92f, SpriteEffects.None, position, Color.White * (alpha * 0.7f), Color.Lerp(bodyColor, coreColor, 0.28f), progress, 0f, 0.28f, 0.72f, 1f, tangent.ToRotation(), new Vector2(1.38f, 5.15f) * scale, new Vector2(1.82f, 1f) * scale);
			}
		}
	}

	private static void DrawModeOneSoftTrailSegment(Texture2D texture, Vector2 start, Vector2 end, Color color, float width, float opacity, float alphaRetention)
	{
		if (!(opacity <= 0f) && !(width <= 0f))
		{
			Vector2 delta = end - start;
			float length = delta.Length();
			if (!(length <= 0.5f) && !float.IsNaN(length) && !float.IsInfinity(length))
			{
				Vector2 origin = texture.Size() * 0.5f;
				Vector2 drawPosition = (start + end) * 0.5f - Main.screenPosition;
				Vector2 scale = new Vector2(MathF.Max(0.01f, (length + width * 0.8f) / (float)texture.Width), MathF.Max(0.01f, width / (float)texture.Height));
				Color drawColor = color * MathHelper.Clamp(opacity, 0f, 1f);
				drawColor.A = (byte)((float)(int)drawColor.A * MathHelper.Clamp(alphaRetention, 0f, 1f));
				Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, delta.ToRotation(), origin, scale, SpriteEffects.None);
			}
		}
	}

	private void DrawModeOneBladeSparkles(Vector2[] outerPoints, float[] alphas, int count, float progress, Color edgeColor, Color coreColor, float scale)
	{
		if (count < 3)
		{
			return;
		}
		int sparkleCount = (IsModeOneFinisher ? 7 : 5);
		for (int i = 0; i < sparkleCount; i++)
		{
			float factor = ((sparkleCount <= 1) ? 1f : ((float)i / (float)(sparkleCount - 1)));
			int index = Math.Clamp((int)MathF.Round(MathHelper.Lerp((float)count * 0.34f, (float)count - 1f, factor)), 0, count - 1);
			float alpha = alphas[index] * MathHelper.Lerp(0.34f, 0.88f, factor);
			if (!(alpha <= 0.06f))
			{
				Vector2 tangent = GetModeOneTrailTangent(outerPoints, count, index);
				Vector2 drawPosition = outerPoints[index] - Main.screenPosition;
				Vector2 sparkleScale = new Vector2(MathHelper.Lerp(0.52f, IsModeOneFinisher ? 1.14f : 0.8f, factor) * scale, MathHelper.Lerp(1.68f, IsModeOneFinisher ? 3.7f : 2.62f, factor) * scale);
				DrawPrettyStarSparkle(alpha, SpriteEffects.None, drawPosition, coreColor * (alpha * 0.9f), edgeColor, progress, 0f, 0.38f, 0.82f, 1f, tangent.ToRotation(), sparkleScale, Vector2.One * scale);
			}
		}
	}

	private void DrawModeOneImpactFlash()
	{
		if (!(bladeImpactFlashTimer <= 0f) && !Main.dedServ)
		{
			float fade = MathHelper.Clamp(bladeImpactFlashTimer / 9f, 0f, 1f);
			float alpha = SmoothStep(fade);
			float scale = MathHelper.Clamp(base.Projectile.scale, 0.25f, 3f);
			Vector2 direction = base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction);
			GetModeOneSlashPalette(out var _, out var bodyColor, out var edgeColor, out var coreColor);
			Color shine = Color.Lerp(bodyColor, edgeColor, IsModeOneFinisher ? 0.72f : 0.62f);
			Color flashCore = Color.Lerp(coreColor, shine, 0.12f);
			Vector2 drawPosition = bladeImpactFlashCenter - Main.screenPosition;
			DrawPrettyStarSparkle(alpha * (IsModeOneFinisher ? 1f : 0.94f), SpriteEffects.None, drawPosition, flashCore * (alpha * 0.98f), shine, 1f - fade, 0f, 0.24f, 0.86f, 1f, direction.ToRotation(), new Vector2(IsModeOneFinisher ? 1.6f : 1.12f, IsModeOneFinisher ? 4.95f : 3.35f) * scale, new Vector2(IsModeOneFinisher ? 2f : 1.42f, 1f) * scale);
			DrawPrettyStarSparkle(alpha * (IsModeOneFinisher ? 0.68f : 0.48f), SpriteEffects.None, drawPosition, shine * (alpha * 0.52f), Color.Lerp(shine, coreColor, 0.28f), 1f - fade, 0f, 0.18f, 0.76f, 1f, direction.ToRotation() + (float)Math.PI / 2f, new Vector2(IsModeOneFinisher ? 1.05f : 0.76f, IsModeOneFinisher ? 3.2f : 2.2f) * scale, new Vector2(IsModeOneFinisher ? 1.35f : 0.96f, 0.74f) * scale);
		}
	}

	private void ProduceModeOneReleaseBurst(Player player)
	{
		if (!Main.dedServ && IsModeOneBladeSwing)
		{
			int duration = GetAttackDuration(player);
			float progress = Utils.GetLerpValue(0f, duration, Timer, clamped: true);
			float distanceFactor = GetDistanceFactor(TargetBladeLength);
			int burstCount = (IsModeOneFinisher ? ((int)MathHelper.Lerp(30f, 50f, distanceFactor)) : ((int)MathHelper.Lerp(15f, 30f, distanceFactor)));
			float sweepBack = (IsModeOneFinisher ? 0.22f : 0.15f);
			for (int i = 0; i < burstCount; i++)
			{
				float sampleProgress = MathHelper.Clamp(progress - Main.rand.NextFloat(0f, sweepBack), 0f, 1f);
				GetBladeLineAtProgress(player, sampleProgress, out var start, out var end);
				Vector2 spinningpoint = (end - start).SafeNormalize(base.Projectile.velocity.SafeNormalize(Vector2.UnitX * base.Projectile.direction));
				Vector2 normal = spinningpoint.RotatedBy(1.5707963705062866);
				Vector2 position = Vector2.Lerp(amount: MathF.Pow(Main.rand.NextFloat(0.16f, 1f), IsModeOneFinisher ? 0.72f : 0.92f), value1: start, value2: end) + normal * Main.rand.NextFloat(IsModeOneFinisher ? (-28f) : (-16f), IsModeOneFinisher ? 28f : 16f);
				Vector2 velocity = spinningpoint.RotatedBy(Main.rand.NextFloat(-0.18f, 0.18f)) * Main.rand.NextFloat(IsModeOneFinisher ? 2.2f : 1.4f, IsModeOneFinisher ? 7.4f : 4.8f) + normal * Main.rand.NextFloat(IsModeOneFinisher ? (-3.6f) : (-2.4f), IsModeOneFinisher ? 3.6f : 2.4f);
				int dustType = (Main.rand.NextBool(IsModeOneFinisher ? 2 : 4) ? 27 : (Main.rand.NextBool() ? 62 : 242));
				Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 58, IsModeOneFinisher ? new Color(255, 72, 216) : new Color(210, 78, 255), Main.rand.NextFloat(IsModeOneFinisher ? 1.35f : 1.02f, IsModeOneFinisher ? 2.48f : 1.78f));
				dust.noGravity = true;
				dust.fadeIn = Main.rand.NextFloat(0.2f, 0.62f);
			}
		}
	}

	private void ProduceModeOneHitSparks(Vector2 center, Vector2 direction, Vector2 normal, float impactScale)
	{
		if (Main.dedServ)
		{
			return;
		}
		int sparkCount = (IsModeOneFinisher ? 28 : 16);
		float speedScale = (IsModeOneFinisher ? 1.42f : 1.08f);
		for (int i = 0; i < sparkCount; i++)
		{
			float side = Main.rand.NextFloat(-1f, 1f);
			Vector2 sparkDirection = (direction * Main.rand.NextFloat(0.35f, 1.15f) + normal * side * Main.rand.NextFloat(0.45f, IsModeOneFinisher ? 1.75f : 1.25f)).SafeNormalize(direction);
			Dust dust = Dust.NewDustPerfect(center - direction * Main.rand.NextFloat(4f, 18f) + normal * Main.rand.NextFloat(IsModeOneFinisher ? (-24f) : (-14f), IsModeOneFinisher ? 24f : 14f), Main.rand.NextBool(IsModeOneFinisher ? 2 : 4) ? 242 : (Main.rand.NextBool(3) ? 27 : 62), sparkDirection * Main.rand.NextFloat(5.4f, IsModeOneFinisher ? 12.8f : 9.4f) * impactScale * speedScale, 35, IsModeOneFinisher ? new Color(255, 92, 218) : new Color(220, 82, 255), Main.rand.NextFloat(IsModeOneFinisher ? 1.18f : 0.96f, IsModeOneFinisher ? 2.06f : 1.56f) * impactScale);
			dust.noGravity = true;
			dust.fadeIn = Main.rand.NextFloat(0.08f, 0.32f);
		}
		if (IsModeOneFinisher)
		{
			for (int j = 0; j < 7; j++)
			{
				Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(18f, 18f), 27, (direction * Main.rand.NextFloat(1.6f, 4.8f) + normal * Main.rand.NextFloat(-2.8f, 2.8f)) * impactScale, 70, new Color(255, 54, 204), Main.rand.NextFloat(1.36f, 2.32f) * impactScale).noGravity = true;
			}
		}
		MoDaoQianRenWarmupSystem.AddLight(center, IsModeOneFinisher ? 1.28f : 0.9f, 0.16f, IsModeOneFinisher ? 1.72f : 1.3f);
	}

	private void GetModeOneSlashPalette(out Color auraColor, out Color bodyColor, out Color edgeColor, out Color coreColor)
	{
		if (base.Projectile.ai[0] == 2f)
		{
			auraColor = new Color(0, 46, 255);
			bodyColor = new Color(0, 224, 255);
			edgeColor = new Color(112, 248, 255);
			coreColor = Color.White;
		}
		else if (IsModeOneFinisher)
		{
			auraColor = new Color(128, 0, 255);
			bodyColor = new Color(255, 0, 176);
			edgeColor = new Color(255, 78, 226);
			coreColor = Color.White;
		}
		else
		{
			auraColor = new Color(84, 0, 255);
			bodyColor = new Color(226, 24, 255);
			edgeColor = new Color(255, 74, 246);
			coreColor = Color.White;
		}
	}

	private Color GetModeOneSlashShadowColor()
	{
		if (base.Projectile.ai[0] == 2f)
		{
			return new Color(0, 12, 108);
		}
		if (IsModeOneFinisher)
		{
			return new Color(58, 0, 112);
		}
		return new Color(42, 0, 128);
	}

	private static Vector2 GetModeOneTrailTangent(Vector2[] points, int count, int index)
	{
		if (count <= 1)
		{
			return Vector2.UnitX;
		}
		return ((index <= 0) ? (points[1] - points[0]) : ((index >= count - 1) ? (points[count - 1] - points[count - 2]) : (points[index + 1] - points[index - 1]))).SafeNormalize(Vector2.UnitX);
	}

	private static void DrawPrettyStarSparkle(float opacity, SpriteEffects dir, Vector2 drawPos, Color drawColor, Color shineColor, float flareCounter, float fadeInStart, float fadeInEnd, float fadeOutStart, float fadeOutEnd, float rotation, Vector2 scale, Vector2 fatness)
	{
		Texture2D value = TextureAssets.Extra[98].Value;
		Vector2 origin = value.Size() / 2f;
		float fade = Utils.GetLerpValue(fadeInStart, fadeInEnd, flareCounter, clamped: true) * Utils.GetLerpValue(fadeOutEnd, fadeOutStart, flareCounter, clamped: true);
		Color bigColor = shineColor * opacity * 0.5f * fade;
		Color smallColor = drawColor * opacity * 0.5f * fade;
		bigColor.A = 0;
		smallColor.A = 0;
		Vector2 scaleLeftRight = new Vector2(fatness.X * 0.5f, scale.X);
		Vector2 scaleUpDown = new Vector2(fatness.Y * 0.5f, scale.Y);
		Main.EntitySpriteDraw(value, drawPos, null, bigColor, (float)Math.PI / 2f + rotation, origin, scaleLeftRight, dir);
		Main.EntitySpriteDraw(value, drawPos, null, bigColor, rotation, origin, scaleUpDown, dir);
		Main.EntitySpriteDraw(value, drawPos, null, smallColor, (float)Math.PI / 2f + rotation, origin, scaleLeftRight * 0.58f, dir);
		Main.EntitySpriteDraw(value, drawPos, null, smallColor, rotation, origin, scaleUpDown * 0.58f, dir);
	}

	private static float GetGreatswordBladeHalfWidth(float progress, float distanceFactor)
	{
		progress = MathHelper.Clamp(progress, 0f, 1f);
		float rootGather = MathHelper.Lerp(0.34f, 1f, SmoothStep(Utils.GetLerpValue(0f, 0.22f, progress, clamped: true)));
		float tipFade = MathHelper.Lerp(1f, 0.46f, SmoothStep(Utils.GetLerpValue(0.78f, 1f, progress, clamped: true)));
		float chip = (MoDaoQianRenShardVisuals.Random01((int)(progress * 997f) + 811) - 0.5f) * 0.09f;
		return MathHelper.Lerp(18f, 42f, distanceFactor) * MathHelper.Clamp(rootGather * tipFade + chip, 0.28f, 1.08f);
	}

	private static void DrawMicroShard(Texture2D texture, Vector2 drawPosition, int seed, Color auraColor, Color coreColor, Color flashColor, float rotation, float scale, float flicker)
	{
		Rectangle source = MoDaoQianRenShardVisuals.GetFrame(seed);
		Vector2 origin = source.Size() * 0.5f;
		Main.EntitySpriteDraw(texture, drawPosition, source, auraColor * (0.18f + flicker * 0.12f), rotation, origin, scale * (1.7f + flicker * 0.16f), SpriteEffects.None);
		Main.EntitySpriteDraw(texture, drawPosition, source, coreColor, rotation, origin, scale, SpriteEffects.None);
		if (flicker > 0.78f)
		{
			Main.EntitySpriteDraw(texture, drawPosition, source, flashColor * MathHelper.Clamp((flicker - 0.78f) / 0.4f, 0f, 0.42f), rotation, origin, scale * 0.62f, SpriteEffects.None);
		}
	}

	private static void DrawShardSpark(Texture2D pixel, Vector2 position, float rotation, float progress, float shimmer)
	{
		float alpha = MathHelper.Clamp((shimmer - 0.88f) / 0.12f, 0f, 1f);
		Vector2 scale = new Vector2(MathHelper.Lerp(8f, 16f, progress) * alpha, MathHelper.Lerp(1.1f, 2.2f, progress));
		Color core = new Color(255, 225, 255) * alpha;
		Color glow = new Color(210, 60, 255) * (alpha * 0.46f);
		Rectangle source = new Rectangle(0, 0, 1, 1);
		Vector2 origin = new Vector2(0.5f);
		Main.EntitySpriteDraw(pixel, position - Main.screenPosition, source, glow, rotation, origin, scale * 2.2f, SpriteEffects.None);
		Main.EntitySpriteDraw(pixel, position - Main.screenPosition, source, core, rotation, origin, scale, SpriteEffects.None);
	}

	private static void DrawHandle(Texture2D texture, Vector2 drawPosition, float rotation, SpriteEffects effects, float drawScale = 1f)
	{
		Main.EntitySpriteDraw(origin: new Vector2(18f, 10.5f), texture: texture, position: drawPosition, sourceRectangle: null, color: Color.White, rotation: rotation, scale: 0.99863356f * drawScale, effects: effects);
	}
}
