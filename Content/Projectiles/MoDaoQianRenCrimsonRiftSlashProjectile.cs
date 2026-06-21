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

public class MoDaoQianRenCrimsonRiftSlashProjectile : ModProjectile
{
	private const int WindupFrames = 7;
	private const int SlashFrames = 16;
	private const int RecoveryFrames = 8;
	private const int HoldReadyFrames = 10;
	private const float HoldAngle = 1.9f;
	private const float SwingArc = 4.45f;
	private const float RootDistance = 46f;
	private const float HoldoutSwingSideMagnitude = 1f;
	private const float ReleaseQueuedSwingSideMagnitude = 2f;

	private static readonly Color CrimsonFogOuterColor = new Color(88, 8, 28);

	private static readonly Color CrimsonFogBodyColor = new Color(158, 28, 62);

	private static readonly Color CrimsonFogCoreColor = new Color(242, 142, 172);

	private static readonly SoundStyle OverhaulSwordMediumSwing = new SoundStyle("TerrariaOverhaul/Assets/Sounds/Items/Melee/CuttingSwingMedium", 2)
	{
		Volume = 0.84f,
		Pitch = -0.08f,
		PitchVariance = 0.1f
	};

	private static readonly SoundStyle OverhaulSwordHeavySwing = new SoundStyle("TerrariaOverhaul/Assets/Sounds/Items/Melee/CuttingSwingHeavy", 2)
	{
		Volume = 1.08f,
		Pitch = -0.24f,
		PitchVariance = 0.12f
	};

	private static readonly SoundStyle OverhaulSwordKillingBlow = new SoundStyle("TerrariaOverhaul/Assets/Sounds/Items/Melee/KillingBlow", 2)
	{
		Volume = 0.58f,
		Pitch = -0.22f,
		PitchVariance = 0.08f
	};

	private Vector2 hitFlashCenter;

	private float hitFlashTimer;

	private float hitFlashRotation;

	private float hitFlashScale = 1f;

	private bool playedHeavyHitSound;

	public override string Texture => "Terraria/Images/Item_0";

	private ref float TargetBladeLength => ref Projectile.ai[0];

	private ref float AimRotation => ref Projectile.ai[1];

	private ref float SwingSide => ref Projectile.ai[2];

	private ref float Timer => ref Projectile.localAI[0];

	private ref float PreviousRotation => ref Projectile.localAI[2];

	private float CurrentRotation => Projectile.velocity.ToRotation();

	private bool IsSlashing => Timer > 0f;

	private bool IsActiveFrame => IsSlashing && Timer >= GetRuntimeStats().CrimsonRiftWindupFrames && Timer <= GetRuntimeStats().CrimsonRiftWindupFrames + GetRuntimeStats().CrimsonRiftSlashFrames;

	private bool IsReleaseQueued => MathF.Abs(SwingSide) >= ReleaseQueuedSwingSideMagnitude;

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
	}

	public override void SetDefaults()
	{
		Projectile.width = 32;
		Projectile.height = 32;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.ownerHitCheck = false;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.noEnchantmentVisuals = true;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 6;
		Projectile.timeLeft = 2;
	}

	public override bool ShouldUpdatePosition()
	{
		return false;
	}

	public override bool? CanDamage()
	{
		return IsActiveFrame ? null : false;
	}

	public override void AI()
	{
		Player player = Main.player[Projectile.owner];
		if (!IsOwnerValid(player))
		{
			Projectile.Kill();
			return;
		}
		Projectile.timeLeft = 2;
		Projectile.scale = GetOwnerBladeScale(player);
		Projectile.localNPCHitCooldown = GetRuntimeStats(player).CrimsonRiftLocalHitCooldown;
		TargetBladeLength = MathHelper.Clamp(TargetBladeLength, 120f, 1364f);
		if (SwingSide == 0f)
		{
			SwingSide = Main.rand.NextBool() ? HoldoutSwingSideMagnitude : -HoldoutSwingSideMagnitude;
		}
		if (hitFlashTimer > 0f)
		{
			hitFlashTimer--;
		}
		UpdateAim(player);
		UpdateSwingCycle(player);
		UpdatePlayerVisuals(player);
		ProduceAmbientEffects(player);
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		if (!IsActiveFrame || Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
		{
			return false;
		}
		Player player = Main.player[Projectile.owner];
		Vector2 handPosition = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
		float width = MathHelper.Lerp(68f, 116f, MoDaoQianRenHeldProjectile.GetDistanceFactor(TargetBladeLength)) * Projectile.scale;
		float collisionPoint = 0f;
		GetBladeLine(handPosition, CurrentRotation, out Vector2 currentStart, out Vector2 currentEnd);
		if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), currentStart, currentEnd, width, ref collisionPoint))
		{
			return true;
		}
		if (PreviousRotation != 0f)
		{
			GetBladeLine(handPosition, PreviousRotation, out Vector2 previousStart, out Vector2 previousEnd);
			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), previousStart, previousEnd, width, ref collisionPoint))
			{
				return true;
			}
		}
		return false;
	}

	public override void CutTiles()
	{
		if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
		{
			return;
		}
		Player player = Main.player[Projectile.owner];
		Vector2 handPosition = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
		GetBladeLine(handPosition, CurrentRotation, out Vector2 start, out Vector2 end);
		DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
		Utils.PlotTileLine(start, end, 56f * Projectile.scale, DelegateMethods.CutTiles);
	}

	public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
	{
		if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers && MoDaoQianRen.GetGreatswordCritChance(GetCurrentGrowthStage(Main.player[Projectile.owner])) >= 100)
		{
			modifiers.SetCrit();
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
		{
			return;
		}
		Player player = Main.player[Projectile.owner];
		player.GetModPlayer<MoDaoQianRenPlayer>().AddShardCharge(GetRuntimeStats(player).CrimsonRiftShardChargeGain);
		ProduceHitEffects(player, target, damageDone);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		if (MoDaoQianRenWarmupSystem.ShouldSkipCustomDrawingForLights || Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers)
		{
			return false;
		}
		Player player = Main.player[Projectile.owner];
		Vector2 drawPosition = Projectile.Center - Main.screenPosition;
		float rotation = CurrentRotation;
		Vector2 direction = rotation.ToRotationVector2();
		Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
		float distanceFactor = MoDaoQianRenHeldProjectile.GetDistanceFactor(TargetBladeLength);
		float swingPower = GetSwingPower();
		float bladePower = IsSlashing ? swingPower : 0.28f * SmoothStep(Utils.GetLerpValue(0f, HoldReadyFrames, Projectile.localAI[1], clamped: true));
		DrawSlashArc(player, foreground: false, swingPower);
		DrawFogBlade(direction, normal, distanceFactor, bladePower);
		Texture2D hiltTexture = ModContent.Request<Texture2D>(MoDaoQianRenMod.WeaponHiltTexture).Value;
		SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
		DrawHandle(hiltTexture, drawPosition, rotation, effects, Projectile.scale);
		DrawSlashArc(player, foreground: true, swingPower);
		DrawHitFlash();
		return false;
	}

	private bool IsOwnerValid(Player player)
	{
		if (!player.active || player.dead || MoDaoQianRenWarmupSystem.ShouldBlockBladeUseForLights)
		{
			return false;
		}
		if (!MoDaoQianRen.TryGetOwnedGrowthStage(player, out int growthStage) || !MoDaoQianRen.IsGreatswordDevilsModeUnlocked(growthStage))
		{
			return false;
		}
		if (Projectile.owner != Main.myPlayer)
		{
			return true;
		}
		if (player.noItems || player.CCed || !(player.HeldItem?.ModItem is MoDaoQianRen))
		{
			return false;
		}
		return player.GetModPlayer<MoDaoQianRenPlayer>().IsGreatswordDevilsMode;
	}

	private void UpdateAim(Player player)
	{
		if (Projectile.owner == Main.myPlayer)
		{
			Vector2 handPosition = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
			Vector2 aim = Main.MouseWorld - handPosition;
			if (aim == Vector2.Zero || aim.HasNaNs())
			{
				aim = AimRotation.ToRotationVector2();
			}
			AimRotation = aim.SafeNormalize(Vector2.UnitX * player.direction).ToRotation();
			if (!Main.mouseRight && !IsReleaseQueued)
			{
				SwingSide = SwingSide >= 0f ? ReleaseQueuedSwingSideMagnitude : -ReleaseQueuedSwingSideMagnitude;
				Projectile.netUpdate = true;
			}
			if (Timer % 4f == 0f)
			{
				Projectile.netUpdate = true;
			}
		}
	}

	private void UpdateSwingCycle(Player player)
	{
		PreviousRotation = CurrentRotation;
		if (!IsSlashing)
		{
			float holdProgress = SmoothStep(Utils.GetLerpValue(0f, HoldReadyFrames, Projectile.localAI[1]++, clamped: true));
			float side = SwingSide >= 0f ? 1f : -1f;
			float holdRotation = AimRotation + side * MathHelper.Lerp(HoldAngle * 0.42f, HoldAngle, holdProgress);
			Projectile.velocity = holdRotation.ToRotationVector2();
			Vector2 holdHandPosition = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
			Projectile.Center = holdHandPosition + Projectile.velocity * 12f;
			Projectile.rotation = holdRotation;
			if (Projectile.owner == Main.myPlayer && Main.mouseLeft && Main.mouseRight && !IsReleaseQueued)
			{
				Timer = 1f;
				Projectile.localAI[1] = 0f;
				PlayWindupSound();
				Projectile.netUpdate = true;
			}
			else if (IsReleaseQueued)
			{
				Projectile.Kill();
			}
			return;
		}
		Timer++;
		if (Timer == 1f)
		{
			PlayWindupSound();
		}
		MoDaoQianRenStageStats stats = GetRuntimeStats(player);
		if (Timer == stats.CrimsonRiftWindupFrames)
		{
			PlaySwingSound(player);
			AddScreenShake(player, 1.05f);
		}
		float rotation = GetRotationAtFrame(Timer);
		Projectile.velocity = rotation.ToRotationVector2();
		Vector2 handPosition = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
		Projectile.Center = handPosition + Projectile.velocity * 12f;
		Projectile.rotation = rotation;
		if (Timer >= stats.CrimsonRiftWindupFrames + stats.CrimsonRiftSlashFrames + stats.CrimsonRiftRecoveryFrames)
		{
			if (IsReleaseQueued)
			{
				Projectile.Kill();
				return;
			}
			SwingSide = SwingSide >= 0f ? -HoldoutSwingSideMagnitude : HoldoutSwingSideMagnitude;
			Timer = 0f;
			Projectile.localAI[1] = HoldReadyFrames;
			playedHeavyHitSound = false;
			PreviousRotation = rotation;
			Projectile.netUpdate = true;
		}
	}

	private float GetRotationAtFrame(float frame)
	{
		float side = SwingSide >= 0f ? 1f : -1f;
		MoDaoQianRenStageStats stats = GetRuntimeStats();
		if (frame < stats.CrimsonRiftWindupFrames)
		{
			float windup = SmoothStep(Utils.GetLerpValue(0f, stats.CrimsonRiftWindupFrames, frame, clamped: true));
			return AimRotation + side * MathHelper.Lerp(HoldAngle * 0.62f, HoldAngle, windup);
		}
		float slash = SmoothStep(Utils.GetLerpValue(stats.CrimsonRiftWindupFrames, stats.CrimsonRiftWindupFrames + stats.CrimsonRiftSlashFrames, frame, clamped: true));
		return AimRotation + side * MathHelper.Lerp(HoldAngle, HoldAngle - SwingArc, slash);
	}

	private void UpdatePlayerVisuals(Player player)
	{
		int facing = MathF.Cos(AimRotation) >= 0f ? 1 : -1;
		Projectile.direction = facing;
		Projectile.spriteDirection = facing;
		player.ChangeDir(facing);
		player.heldProj = Projectile.whoAmI;
		player.itemTime = 2;
		player.itemAnimation = 2;
		player.itemRotation = (Projectile.velocity * player.direction).ToRotation();
	}

	private void GetBladeLine(Vector2 handPosition, float rotation, out Vector2 start, out Vector2 end)
	{
		Vector2 direction = rotation.ToRotationVector2();
		float rootDistance = RootDistance * Projectile.scale;
		start = handPosition + direction * rootDistance;
		end = handPosition + direction * MathF.Max(rootDistance + 24f, TargetBladeLength * Projectile.scale);
	}

	private float GetSwingPower()
	{
		if (!IsSlashing)
		{
			return 0f;
		}
		MoDaoQianRenStageStats stats = GetRuntimeStats();
		float release = SmoothStep(Utils.GetLerpValue(stats.CrimsonRiftWindupFrames - 2f, stats.CrimsonRiftWindupFrames + 4f, Timer, clamped: true));
		float fade = 1f - SmoothStep(Utils.GetLerpValue(stats.CrimsonRiftWindupFrames + stats.CrimsonRiftSlashFrames * 0.66f, stats.CrimsonRiftWindupFrames + stats.CrimsonRiftSlashFrames + stats.CrimsonRiftRecoveryFrames, Timer, clamped: true));
		return MathHelper.Clamp(MathF.Max(0.22f, release * fade), 0f, 1f);
	}

	private void DrawFogBlade(Vector2 direction, Vector2 normal, float distanceFactor, float swingPower)
	{
		float rootDistance = RootDistance * Projectile.scale;
		float bladeLength = MathF.Max(36f, TargetBladeLength * Projectile.scale - rootDistance);
		Vector2 visibleRootWorldPosition = Projectile.Center + direction * rootDistance;
		float hiddenRootLength = MathHelper.Clamp(rootDistance * 0.9f, 34f * Projectile.scale, 82f * Projectile.scale);
		Vector2 fogRootWorldPosition = visibleRootWorldPosition - direction * hiddenRootLength;
		float fogBladeLength = bladeLength + hiddenRootLength;
		Vector2 rootPosition = fogRootWorldPosition - Main.screenPosition;
		float visualTimer = GetFogVisualTimer();
		float breath = 0.5f + MathF.Sin(visualTimer * 0.055f + Projectile.whoAmI * 0.37f) * 0.5f;
		float auraPower = MathHelper.Lerp(0.26f, 0.62f, swingPower) * MathHelper.Lerp(0.9f, 1.12f, breath);
		MoDaoQianRenGreatswordFogVisuals.Draw(rootPosition, direction, normal, fogBladeLength, progress => GetFogHalfWidth(progress, distanceFactor), distanceFactor, auraPower, visualTimer * 0.52f, 16300, CrimsonFogOuterColor, CrimsonFogBodyColor, CrimsonFogCoreColor);
		DrawFogBladeFlowStrands(visibleRootWorldPosition, direction, normal, bladeLength, distanceFactor, swingPower, visualTimer);
		DrawFogBladeRootBlend(direction, normal, rootDistance, distanceFactor, swingPower, visualTimer);
		DrawFogBladeShards(visibleRootWorldPosition, direction, normal, bladeLength, distanceFactor, swingPower);
	}

	private float GetFogVisualTimer()
	{
		return Timer + (float)Main.GameUpdateCount * 0.72f;
	}

	private void DrawFogBladeFlowStrands(Vector2 rootWorldPosition, Vector2 direction, Vector2 normal, float bladeLength, float distanceFactor, float bladePower, float visualTimer)
	{
		float presence = SmoothStep(Utils.GetLerpValue(0.025f, 0.24f, bladePower, clamped: true));
		if (presence <= 0.015f)
		{
			return;
		}
		Texture2D texture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
		Vector2 origin = texture.Size() * 0.5f;
		int strandCount = (int)MathHelper.Clamp(bladeLength / 42f, 5f, 28f);
		for (int i = 0; i < strandCount; i++)
		{
			float seed = MoDaoQianRenShardVisuals.Random01(i * 83 + Projectile.whoAmI * 31 + 19201);
			float speed = MathHelper.Lerp(0.0045f, 0.013f, MoDaoQianRenShardVisuals.Random01(i * 97 + 19229));
			float progress = Wrap01(seed + visualTimer * speed);
			progress = MathHelper.Lerp(0.045f, 0.965f, progress);
			float halfWidth = GetFogHalfWidth(progress, distanceFactor);
			float sideSeed = MoDaoQianRenShardVisuals.Random01(i * 67 + 19243) * 2f - 1f;
			float sideOffset = sideSeed * halfWidth * MathHelper.Lerp(0.16f, 0.72f, MoDaoQianRenShardVisuals.Random01(i * 71 + 19261));
			float wave = MathF.Sin(visualTimer * MathHelper.Lerp(0.055f, 0.14f, seed) + i * 1.71f) * MathHelper.Lerp(0.7f, 3.6f, progress);
			Vector2 position = rootWorldPosition + direction * (bladeLength * progress) + normal * (sideOffset + wave) - Main.screenPosition;
			float headFade = SmoothStep(Utils.GetLerpValue(0.045f, 0.18f, progress, clamped: true));
			float tipFade = 1f - SmoothStep(Utils.GetLerpValue(0.82f, 1f, progress, clamped: true)) * 0.64f;
			float flicker = MoDaoQianRenShardVisuals.Flicker(visualTimer, i + 19300, 0.05f, 0.46f);
			float alpha = presence * headFade * tipFade * MathHelper.Lerp(0.22f, 0.52f, flicker);
			float length = MathHelper.Lerp(34f, 86f, distanceFactor) * MathHelper.Lerp(0.65f, 1.2f, seed) * MathHelper.Lerp(0.7f, 0.38f, progress);
			float width = MathHelper.Lerp(8f, 18f, distanceFactor) * MathHelper.Lerp(0.58f, 1.08f, flicker);
			Vector2 scale = new Vector2(MathF.Max(0.01f, length / texture.Width), MathF.Max(0.01f, width / texture.Height)) * Projectile.scale;
			Color shadow = new Color(74, 4, 26) * (alpha * 0.55f);
			Color body = new Color(176, 34, 72) * (alpha * 0.46f);
			Color core = new Color(255, 190, 210) * (alpha * 0.2f);
			Main.EntitySpriteDraw(texture, position, null, shadow, direction.ToRotation(), origin, scale * new Vector2(1.25f, 1.8f), SpriteEffects.None);
			Main.EntitySpriteDraw(texture, position + normal * MathF.Sin(visualTimer * 0.08f + i) * 1.2f, null, body, direction.ToRotation(), origin, scale, SpriteEffects.None);
			if (flicker > 0.86f)
			{
				Main.EntitySpriteDraw(texture, position, null, core, direction.ToRotation(), origin, scale * new Vector2(0.72f, 0.46f), SpriteEffects.None);
			}
		}
	}

	private void DrawFogBladeRootBlend(Vector2 direction, Vector2 normal, float rootDistance, float distanceFactor, float bladePower, float visualTimer)
	{
		float presence = SmoothStep(Utils.GetLerpValue(0.015f, 0.22f, bladePower, clamped: true));
		if (presence <= 0.015f)
		{
			return;
		}
		Texture2D texture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
		Vector2 origin = texture.Size() * 0.5f;
		float rotation = direction.ToRotation();
		float pulse = 0.5f + MathF.Sin(visualTimer * 0.12f + Projectile.whoAmI) * 0.5f;
		Vector2 bridgeCenter = Projectile.Center + direction * (rootDistance * 0.48f) + normal * MathF.Sin(visualTimer * 0.075f) * 1.6f - Main.screenPosition;
		float bridgeLength = rootDistance * MathHelper.Lerp(1.12f, 1.36f, pulse);
		float bridgeWidth = MathHelper.Lerp(34f, 70f, distanceFactor) * Projectile.scale * MathHelper.Lerp(0.84f, 1.12f, pulse);
		Vector2 bridgeScale = new Vector2(MathF.Max(0.01f, bridgeLength / texture.Width), MathF.Max(0.01f, bridgeWidth / texture.Height));
		Color shadow = new Color(74, 0, 24) * (0.36f * presence);
		Color body = new Color(168, 30, 66) * (0.32f * presence);
		Color core = new Color(255, 184, 204) * (0.14f * presence);
		Main.EntitySpriteDraw(texture, bridgeCenter, null, shadow, rotation, origin, bridgeScale * new Vector2(1.18f, 1.55f), SpriteEffects.None);
		Main.EntitySpriteDraw(texture, bridgeCenter + normal * MathF.Sin(visualTimer * 0.18f) * 1.2f, null, body, rotation, origin, bridgeScale * new Vector2(0.96f, 0.86f), SpriteEffects.None);
		Main.EntitySpriteDraw(texture, bridgeCenter + direction * (rootDistance * 0.12f), null, core, rotation, origin, bridgeScale * new Vector2(0.68f, 0.36f), SpriteEffects.None);
		for (int i = 0; i < 5; i++)
		{
			float progress = ((float)i + 0.5f) / 5f;
			float side = (MoDaoQianRenShardVisuals.Random01(i * 37 + 19401 + Projectile.whoAmI) - 0.5f) * bridgeWidth * 0.34f;
			Vector2 wispPosition = Projectile.Center + direction * MathHelper.Lerp(7f * Projectile.scale, rootDistance * 0.96f, progress) + normal * (side + MathF.Sin(visualTimer * 0.16f + i) * 1.4f) - Main.screenPosition;
			float alpha = presence * MathHelper.Lerp(0.2f, 0.08f, progress) * MathHelper.Lerp(0.8f, 1.24f, pulse);
			Vector2 wispScale = new Vector2(MathF.Max(0.01f, MathHelper.Lerp(16f, 38f, progress) / texture.Width), MathF.Max(0.01f, MathHelper.Lerp(10f, 24f, distanceFactor) / texture.Height)) * Projectile.scale;
			Main.EntitySpriteDraw(texture, wispPosition, null, new Color(220, 54, 92) * alpha, rotation, origin, wispScale, SpriteEffects.None);
		}
		int featherCount = 11;
		float rootHalfWidth = GetFogHalfWidth(0.14f, distanceFactor) * Projectile.scale;
		for (int j = 0; j < featherCount; j++)
		{
			float seed = MoDaoQianRenShardVisuals.Random01(j * 61 + 19517 + Projectile.whoAmI);
			float sideSeed = MoDaoQianRenShardVisuals.Random01(j * 67 + 19531 + Projectile.whoAmI) * 2f - 1f;
			float side = sideSeed * rootHalfWidth * MathHelper.Lerp(0.18f, 0.82f, seed);
			float outward = MathHelper.Lerp(-10f, 26f, MoDaoQianRenShardVisuals.Random01(j * 73 + 19549));
			float wobble = MathF.Sin(visualTimer * MathHelper.Lerp(0.08f, 0.18f, seed) + j * 1.63f);
			Vector2 featherPosition = Projectile.Center + direction * (rootDistance + outward) + normal * (side + wobble * MathHelper.Lerp(0.8f, 3.2f, seed)) - Main.screenPosition;
			float alpha = presence * MathHelper.Lerp(0.16f, 0.34f, MoDaoQianRenShardVisuals.Flicker(visualTimer, j + 19580, 0.06f, 0.5f));
			float length = MathHelper.Lerp(20f, 58f, seed) * Projectile.scale;
			float width = MathHelper.Lerp(10f, 26f, distanceFactor) * MathHelper.Lerp(0.62f, 1.18f, seed) * Projectile.scale;
			Vector2 scale = new Vector2(MathF.Max(0.01f, length / texture.Width), MathF.Max(0.01f, width / texture.Height));
			float featherRotation = rotation + sideSeed * MathHelper.Lerp(0.08f, 0.34f, seed) + wobble * 0.05f;
			Main.EntitySpriteDraw(texture, featherPosition, null, new Color(94, 6, 32) * (alpha * 0.62f), featherRotation, origin, scale * new Vector2(1.18f, 1.55f), SpriteEffects.None);
			Main.EntitySpriteDraw(texture, featherPosition + direction * MathHelper.Lerp(2f, 9f, seed), null, new Color(206, 48, 82) * alpha, featherRotation, origin, scale * new Vector2(0.82f, 0.72f), SpriteEffects.None);
		}
	}

	private static float Wrap01(float value)
	{
		return value - MathF.Floor(value);
	}

	private void DrawFogBladeShards(Vector2 rootWorldPosition, Vector2 direction, Vector2 normal, float bladeLength, float distanceFactor, float bladePower)
	{
		float presence = SmoothStep(Utils.GetLerpValue(0.02f, 0.26f, bladePower, clamped: true));
		if (presence <= 0.02f)
		{
			return;
		}
		Texture2D shardTexture = MoDaoQianRenShardVisuals.Texture;
		Texture2D pixel = TextureAssets.MagicPixel.Value;
		float visualTimer = (float)Main.GameUpdateCount * 0.72f + Timer;
		int seedOffset = Projectile.whoAmI * 257;
		int coreShardCount = (int)MathHelper.Clamp(bladeLength / 14f, 12f, 82f);
		for (int i = 0; i < coreShardCount; i++)
		{
			float progress = MathF.Pow(((float)i + MoDaoQianRenShardVisuals.Random01(i * 97 + 18011 + seedOffset)) / coreShardCount, 1.08f);
			progress = MathHelper.Clamp(progress, 0.035f, 0.985f);
			float halfWidth = GetFogHalfWidth(progress, distanceFactor);
			float sideSeed = MoDaoQianRenShardVisuals.Random01(i * 73 + 18029 + seedOffset) * 2f - 1f;
			float sideOffset = MathF.Sign(sideSeed) * MathF.Pow(MathF.Abs(sideSeed), 0.76f) * halfWidth * MathHelper.Lerp(0.08f, 0.58f, MoDaoQianRenShardVisuals.Random01(i * 59 + 18041 + seedOffset));
			float drift = MathF.Sin(visualTimer * MathHelper.Lerp(0.045f, 0.13f, MoDaoQianRenShardVisuals.Random01(i * 31 + 18059)) + i * 1.77f) * MathHelper.Lerp(0.8f, 4.8f, progress);
			float alongJitter = MathHelper.Lerp(-5.5f, 5.5f, MoDaoQianRenShardVisuals.Random01(i * 43 + 18071 + seedOffset)) * MathHelper.Lerp(0.35f, 1f, progress);
			Vector2 shardPosition = rootWorldPosition + direction * (bladeLength * progress + alongJitter) + normal * (sideOffset + drift);
			float flicker = MoDaoQianRenShardVisuals.Flicker(visualTimer, i + 18100 + seedOffset, 0.055f, 0.72f);
			float rootFade = SmoothStep(Utils.GetLerpValue(0.015f, 0.16f, progress, clamped: true));
			float tipFade = 1f - SmoothStep(Utils.GetLerpValue(0.9f, 1f, progress, clamped: true)) * 0.48f;
			float alpha = presence * rootFade * tipFade * MathHelper.Lerp(0.58f, 1f, flicker);
			float shardScale = MathHelper.Lerp(0.078f, 0.035f, progress) * MathHelper.Lerp(0.78f, 1.24f, MoDaoQianRenShardVisuals.Random01(i * 47 + 18083 + seedOffset)) * MathHelper.Lerp(0.88f, 1.22f, flicker) * Projectile.scale;
			float shardRotation = direction.ToRotation() + sideSeed * MathHelper.Lerp(0.18f, 0.72f, progress) + MathF.Sin(visualTimer * 0.105f + i * 0.91f) * 0.18f;
			Color outline = Color.Lerp(new Color(84, 4, 40), new Color(188, 22, 68), flicker * 0.68f) * (0.44f * alpha);
			Color core = Color.Lerp(new Color(214, 42, 92), new Color(255, 188, 214), 0.16f + flicker * 0.4f) * (0.74f * alpha);
			Color flash = Color.Lerp(new Color(255, 78, 128), Color.White, flicker * 0.66f);
			MoDaoQianRenShardVisuals.DrawOutlinedShard(shardTexture, shardPosition - Main.screenPosition, i + 18200 + seedOffset, outline, core, flash * (0.48f * alpha), shardRotation, shardScale, flicker);
			if (flicker > 0.94f && i % 9 == (int)(Main.GameUpdateCount / 5) % 9)
			{
				DrawFogShardSpark(pixel, shardPosition, shardRotation, progress, alpha);
			}
			if (i % 13 == 0)
			{
				MoDaoQianRenWarmupSystem.AddLight(shardPosition, 0.2f * alpha, 0.018f * alpha, 0.07f * alpha);
			}
		}
		int dustShardCount = (int)MathHelper.Clamp(bladeLength / 10.5f, 16f, 112f);
		for (int j = 0; j < dustShardCount; j++)
		{
			float progress = ((float)j + MoDaoQianRenShardVisuals.Random01(j * 89 + 18503 + seedOffset)) / dustShardCount;
			progress = MathHelper.Clamp(progress, 0.025f, 0.99f);
			float halfWidth = GetFogHalfWidth(progress, distanceFactor);
			float sideSeed = MoDaoQianRenShardVisuals.Random01(j * 67 + 18521 + seedOffset) * 2f - 1f;
			float edgeBias = j % 3 == 0 ? 0.24f : MathHelper.Lerp(0.48f, 0.9f, MoDaoQianRenShardVisuals.Random01(j * 71 + 18539 + seedOffset));
			float sideOffset = MathF.Sign(sideSeed) * halfWidth * edgeBias + sideSeed * MathHelper.Lerp(0.4f, 3.2f, progress);
			Vector2 shardPosition = rootWorldPosition + direction * (bladeLength * progress + MathF.Sin(visualTimer * 0.055f + j * 1.23f) * MathHelper.Lerp(0.8f, 3.8f, progress)) + normal * (sideOffset + MathF.Cos(visualTimer * 0.085f + j * 1.91f) * MathHelper.Lerp(0.45f, 2.2f, progress));
			float flicker = MoDaoQianRenShardVisuals.Flicker(visualTimer, j + 18600 + seedOffset, 0.08f, 0.84f);
			float alpha = presence * MathHelper.Lerp(0.42f, 0.82f, flicker) * (1f - SmoothStep(Utils.GetLerpValue(0.93f, 1f, progress, clamped: true)) * 0.45f);
			float scale = MathHelper.Lerp(0.033f, 0.017f, progress) * MathHelper.Lerp(0.72f, 1.16f, MoDaoQianRenShardVisuals.Random01(j * 53 + 18557 + seedOffset)) * Projectile.scale;
			float rotation = direction.ToRotation() + sideSeed * MathHelper.Lerp(0.32f, 0.92f, progress) + MathF.Sin(visualTimer * 0.16f + j) * 0.2f;
			Color aura = new Color(112, 0, 34) * (0.18f * alpha);
			Color core = Color.Lerp(new Color(196, 38, 86), new Color(255, 166, 202), flicker * 0.54f) * (0.52f * alpha);
			Color flash = Color.Lerp(new Color(255, 70, 122), Color.White, flicker * 0.66f);
			DrawSmallFogShard(shardTexture, shardPosition - Main.screenPosition, j + 18700 + seedOffset, aura, core, flash, rotation, scale, flicker);
		}
	}

	private void DrawSlashArc(Player player, bool foreground, float swingPower)
	{
		if (swingPower <= 0.04f)
		{
			return;
		}
		float distanceFactor = MoDaoQianRenHeldProjectile.GetDistanceFactor(TargetBladeLength);
		Vector2 center = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true) + AimRotation.ToRotationVector2() * (RootDistance * Projectile.scale * 0.2f);
		float radius = MathHelper.Clamp(TargetBladeLength * Projectile.scale * 0.98f, 132f, 1380f);
		float arcPower = GetProcessArcPower() * swingPower;
		float slashProgress = GetSlashProgress();
		float side = SwingSide >= 0f ? 1f : -1f;
		float arcRotation = AimRotation + side * (HoldAngle - SwingArc * 0.5f);
		if (!foreground)
		{
			MoDaoQianRenCrimsonRiftArcVisuals.Draw(center - Main.screenPosition, arcRotation, radius, arcPower * 0.78f, slashProgress, SwingSide < 0f, foreground: false);
		}
		if (foreground)
		{
			MoDaoQianRenCrimsonRiftArcVisuals.Draw(center - Main.screenPosition, arcRotation, radius, arcPower * 0.82f, slashProgress, SwingSide < 0f, foreground: true);
		}
	}

	private float GetProcessArcPower()
	{
		float slashProgress = GetSlashProgress();
		float appear = SmoothStep(Utils.GetLerpValue(0.02f, 0.24f, slashProgress, clamped: true));
		float peak = 1f - SmoothStep(Utils.GetLerpValue(0.78f, 1f, slashProgress, clamped: true));
		float release = MathHelper.Lerp(0.72f, 1.1f, SmoothStep(Utils.GetLerpValue(0.18f, 0.66f, slashProgress, clamped: true)));
		return MathHelper.Clamp(appear * peak * release, 0f, 1f);
	}

	private float GetSlashProgress()
	{
		MoDaoQianRenStageStats stats = GetRuntimeStats();
		return Utils.GetLerpValue(stats.CrimsonRiftWindupFrames, stats.CrimsonRiftWindupFrames + stats.CrimsonRiftSlashFrames, Timer, clamped: true);
	}

	private void DrawHitFlash()
	{
		if (hitFlashTimer <= 0f)
		{
			return;
		}
		Texture2D texture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
		Vector2 origin = texture.Size() * 0.5f;
		float progress = 1f - hitFlashTimer / 10f;
		float power = Utils.GetLerpValue(1f, 0f, progress, clamped: true);
		Vector2 position = hitFlashCenter - Main.screenPosition;
		Vector2 longScale = new Vector2(0.42f, 3.8f) * Projectile.scale * hitFlashScale * MathHelper.Lerp(1.35f, 0.55f, progress);
		Vector2 wideScale = new Vector2(0.22f, 2.25f) * Projectile.scale * hitFlashScale * MathHelper.Lerp(1.15f, 0.45f, progress);
		Color red = new Color(255, 24, 54) * (0.78f * power);
		Color core = new Color(255, 232, 236) * (0.42f * power);
		Main.EntitySpriteDraw(texture, position, null, red, hitFlashRotation + MathHelper.PiOver2, origin, longScale, SpriteEffects.None);
		Main.EntitySpriteDraw(texture, position, null, red, hitFlashRotation, origin, wideScale, SpriteEffects.None);
		Main.EntitySpriteDraw(texture, position, null, core, hitFlashRotation + MathHelper.PiOver2, origin, longScale * 0.42f, SpriteEffects.None);
	}

	private static void DrawSmallFogShard(Texture2D texture, Vector2 drawPosition, int seed, Color auraColor, Color coreColor, Color flashColor, float rotation, float scale, float flicker)
	{
		Rectangle source = MoDaoQianRenShardVisuals.GetFrame(seed);
		Vector2 origin = source.Size() * 0.5f;
		Main.EntitySpriteDraw(texture, drawPosition, source, auraColor * (0.16f + flicker * 0.14f), rotation, origin, scale * (1.64f + flicker * 0.18f), SpriteEffects.None);
		Main.EntitySpriteDraw(texture, drawPosition, source, coreColor, rotation, origin, scale, SpriteEffects.None);
		if (flicker > 0.82f)
		{
			Main.EntitySpriteDraw(texture, drawPosition, source, flashColor * MathHelper.Clamp((flicker - 0.82f) / 0.36f, 0f, 0.36f), rotation, origin, scale * 0.66f, SpriteEffects.None);
		}
	}

	private static void DrawFogShardSpark(Texture2D pixel, Vector2 position, float rotation, float progress, float alpha)
	{
		alpha = MathHelper.Clamp(alpha, 0f, 1f);
		Vector2 scale = new Vector2(MathHelper.Lerp(5f, 12f, progress) * alpha, MathHelper.Lerp(0.8f, 1.7f, progress) * alpha);
		Rectangle source = new Rectangle(0, 0, 1, 1);
		Vector2 origin = new Vector2(0.5f);
		Main.EntitySpriteDraw(pixel, position - Main.screenPosition, source, new Color(255, 42, 118) * (0.32f * alpha), rotation, origin, scale * 2f, SpriteEffects.None);
		Main.EntitySpriteDraw(pixel, position - Main.screenPosition, source, new Color(255, 226, 240) * (0.76f * alpha), rotation, origin, scale, SpriteEffects.None);
	}

	private void ProduceAmbientEffects(Player player)
	{
		Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
		MoDaoQianRenWarmupSystem.AddLight(Projectile.Center + direction * MathHelper.Min(TargetBladeLength * 0.45f, 340f), 0.34f, 0.035f, 0.18f);
	}

	private void ProduceHitEffects(Player player, NPC target, int damageDone)
	{
		if (!Main.dedServ)
		{
			Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
			Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
			float impactScale = MathHelper.Clamp((float)damageDone / MathF.Max(1f, Projectile.damage), 0.7f, 1.7f);
			hitFlashCenter = target.Center;
			hitFlashTimer = 10f;
			hitFlashRotation = direction.ToRotation();
			hitFlashScale = impactScale;
			PlayHeavyHitSoundOnce(target.Center);
			SoundEngine.PlaySound(SoundID.Item10 with
			{
				Volume = 0.64f,
				Pitch = -0.46f,
				PitchVariance = 0.08f
			}, target.Center);
			for (int i = 0; i < 32; i++)
			{
				Vector2 velocity = direction.RotatedBy(Main.rand.NextFloat(-0.85f, 0.85f)) * Main.rand.NextFloat(2.2f, 8.6f) + normal * Main.rand.NextFloat(-3f, 3f);
				Dust dust = Dust.NewDustPerfect(target.Center + normal * Main.rand.NextFloat(-34f, 34f) - direction * Main.rand.NextFloat(4f, 22f), Main.rand.NextBool(3) ? DustID.Blood : DustID.RedTorch, velocity, 26, new Color(255, 34, 62), Main.rand.NextFloat(1.1f, 2.35f) * impactScale);
				dust.noGravity = true;
				dust.fadeIn = Main.rand.NextFloat(0.12f, 0.42f);
			}
			MoDaoQianRenWarmupSystem.AddLight(target.Center, 1.35f, 0.08f, 0.12f);
		}
		AddScreenShake(player, 0.58f);
	}

	private void PlayWindupSound()
	{
		if (Main.dedServ)
		{
			return;
		}
		SoundEngine.PlaySound(SoundID.Item1 with
		{
			Volume = 0.3f,
			Pitch = -0.72f,
			PitchVariance = 0.04f
		}, Projectile.Center);
	}

	private void PlaySwingSound(Player player)
	{
		if (Main.dedServ)
		{
			return;
		}
		if (ModLoader.HasMod("TerrariaOverhaul"))
		{
			SoundEngine.PlaySound(OverhaulSwordMediumSwing, Projectile.Center);
			return;
		}
		SoundEngine.PlaySound(SoundID.Item71 with
		{
			Volume = 0.78f,
			Pitch = -0.48f,
			PitchVariance = 0.08f
		}, Projectile.Center);
		SoundEngine.PlaySound(SoundID.Item15 with
		{
			Volume = 0.62f,
			Pitch = -0.36f,
			PitchVariance = 0.06f
		}, Projectile.Center);
	}

	private void PlayHeavyHitSoundOnce(Vector2 position)
	{
		if (playedHeavyHitSound || !ModLoader.HasMod("TerrariaOverhaul"))
		{
			return;
		}
		playedHeavyHitSound = true;
		SoundEngine.PlaySound(OverhaulSwordHeavySwing, position);
		SoundEngine.PlaySound(OverhaulSwordKillingBlow, position);
	}

	private void AddScreenShake(Player player, float strength)
	{
		if (!Main.dedServ && Projectile.owner == Main.myPlayer)
		{
			Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
			Main.instance.CameraModifiers.Add(new PunchCameraModifier(player.Center + direction * 120f, direction, 0.55f * strength, 9f * strength, 12, 1400f));
		}
	}

	private static int GetCurrentGrowthStage(Player player)
	{
		return MoDaoQianRen.TryGetGrowthStage(player.HeldItem, out int growthStage) ? growthStage : 0;
	}

	private static MoDaoQianRenStageStats GetRuntimeStats(Player player)
	{
		return MoDaoQianRen.GetRuntimeStats(GetCurrentGrowthStage(player));
	}

	private MoDaoQianRenStageStats GetRuntimeStats()
	{
		if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
		{
			return GetRuntimeStats(Main.player[Projectile.owner]);
		}
		return MoDaoQianRen.GetRuntimeStats(0);
	}

	private static float GetOwnerBladeScale(Player player)
	{
		float scale = player != null && player.active && player.HeldItem?.ModItem is MoDaoQianRen ? player.GetAdjustedItemScale(player.HeldItem) : 1f;
		if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f)
		{
			scale = 1f;
		}
		return MathHelper.Clamp(scale, 0.25f, 3f);
	}

	private static float GetFogHalfWidth(float progress, float distanceFactor)
	{
		progress = MathHelper.Clamp(progress, 0f, 1f);
		float body = MathF.Sin(progress * MathHelper.Pi);
		float root = SmoothStep(Utils.GetLerpValue(0f, 0.16f, progress, clamped: true));
		float tip = 1f - SmoothStep(Utils.GetLerpValue(0.78f, 1f, progress, clamped: true));
		float profile = MathHelper.Clamp(MathF.Max(body, 0.34f) * root * MathHelper.Lerp(0.48f, 1f, tip), 0.18f, 1f);
		return MathHelper.Lerp(62f, 116f, distanceFactor) * profile;
	}

	private static float SmoothStep(float value)
	{
		value = MathHelper.Clamp(value, 0f, 1f);
		return value * value * (3f - 2f * value);
	}

	private static void DrawHandle(Texture2D texture, Vector2 drawPosition, float rotation, SpriteEffects effects, float drawScale = 1f)
	{
		Main.EntitySpriteDraw(texture, drawPosition, null, Color.White, rotation, new Vector2(18f, 10.5f), 0.99863356f * drawScale, effects);
	}
}
