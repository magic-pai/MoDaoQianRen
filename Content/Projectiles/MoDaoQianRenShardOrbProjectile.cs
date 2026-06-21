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

public class MoDaoQianRenShardOrbProjectile : ModProjectile
{
	private const float MinimumOrbRadius = 58f;

	private const float MaximumOrbRadius = 165f;

	private const float FieldMinimumOrbRadius = 82f;

	private const float FieldMaximumOrbRadius = 220f;

	private const float MoonlitFieldMinimumOrbRadius = 96f;

	private const float MoonlitFieldMaximumOrbRadius = 270f;

	private const float FinalFieldMinimumOrbRadius = 118f;

	private const float FinalFieldMaximumOrbRadius = 340f;

	private const int OrbShardCount = 92;

	private const int GeometryShardCount = 76;

	private const int FieldBurstChargeCost = 16;

	private const int MoonlitBurstChargeCost = 24;

	private const int FinalBurstChargeCost = 30;

	private const int FieldMinimumBurstCharge = 6;

	private const int MoonlitMinimumBurstCharge = 8;

	private const int FinalMinimumBurstCharge = 10;

	private const int FieldBurstShardCount = 24;

	private const int MoonlitBurstShardCount = 36;

	private const int FinalBurstShardCount = 46;

	private const int FinalConvergenceDuration = 72;

	private const int FinalConvergencePulseRate = 12;

	private const float GuardOriginX = 18f;

	private const float GuardOriginY = 10.5f;

	private const float IconBladeLength = 120.1642f;

	private const float BaseDrawScale = 0.99863356f;

	public override string Texture => "Terraria/Images/Item_0";

	private ref float Timer => ref base.Projectile.localAI[0];

	private ref float CurrentOrbRadius => ref base.Projectile.localAI[1];

	private ref float FieldPower => ref base.Projectile.ai[0];

	private ref float StoredBurstCharge => ref base.Projectile.ai[1];

	private ref float ConvergenceTimer => ref base.Projectile.ai[2];

	public override void SetDefaults()
	{
		base.Projectile.width = 132;
		base.Projectile.height = 132;
		base.Projectile.friendly = true;
		base.Projectile.hostile = false;
		base.Projectile.penetrate = -1;
		base.Projectile.tileCollide = false;
		base.Projectile.ignoreWater = true;
		base.Projectile.DamageType = DamageClass.Melee;
		base.Projectile.noEnchantmentVisuals = true;
		base.Projectile.timeLeft = 2;
		base.Projectile.usesLocalNPCImmunity = true;
		base.Projectile.localNPCHitCooldown = 8;
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
		Timer += 1f;
		int growthStage = GetOwnerGrowthStage(player);
		int baseHitCooldown = ((!MoDaoQianRen.UsesCalamityBalance) ? ((growthStage >= 7) ? 6 : ((growthStage >= 6) ? 7 : 8)) : ((growthStage >= 7) ? 5 : ((growthStage >= 6) ? 6 : 7)));
		base.Projectile.localNPCHitCooldown = MoDaoQianRen.ApplyMeleeAttackSpeed(player, baseHitCooldown, 3);
		if (base.Projectile.owner == Main.myPlayer)
		{
			if (!Main.mouseMiddle || player.noItems || player.CCed || !MoDaoQianRen.TryGetGrowthStage(player.HeldItem, out var _))
			{
				TryReleaseStoredFieldBurst(player, growthStage);
				base.Projectile.Kill();
				return;
			}
			UpdateFieldPower(player, growthStage);
			Vector2 toMouse = Main.MouseWorld - base.Projectile.Center;
			float distance = toMouse.Length();
			Vector2 desiredVelocity = ((distance > 10f) ? (toMouse.SafeNormalize(Vector2.Zero) * MathHelper.Clamp(distance * 0.13f, 5f, 26f)) : Vector2.Zero);
			base.Projectile.velocity = Vector2.Lerp(base.Projectile.velocity, desiredVelocity, 0.22f);
			base.Projectile.netUpdate = Timer % 10f == 0f;
		}
		else
		{
			FieldPower = MathHelper.Lerp(FieldPower, 0f, 0.06f);
		}
		base.Projectile.rotation += ((ConvergenceTimer > 0f) ? 0.07f : 0.035f) * (float)Math.Sign((base.Projectile.velocity.X == 0f) ? 1f : base.Projectile.velocity.X);
		UpdateOrbRadius(player, growthStage);
		UpdateFinalConvergence(player, growthStage);
		base.Projectile.timeLeft = 2;
		UpdatePlayerVisuals(player);
		ProduceOrbDust();
		ProduceFieldDust();
		EmitFieldEnchantmentVisuals();
		float lightRadiusFactor = MathHelper.Clamp(CurrentOrbRadius / GetFieldMaximumRadius(growthStage), 0f, 1f);
		MoDaoQianRenWarmupSystem.AddLight(base.Projectile.Center, 0.72f + lightRadiusFactor * 0.35f + FieldPower * 0.35f, 0.12f, 1.25f + lightRadiusFactor * 0.35f + FieldPower * 0.5f);
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		float closestX = MathHelper.Clamp(base.Projectile.Center.X, targetHitbox.Left, targetHitbox.Right);
		float closestY = MathHelper.Clamp(base.Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom);
		float radius = GetRadiusOrDefault();
		if (Vector2.DistanceSquared(base.Projectile.Center, new Vector2(closestX, closestY)) <= radius * radius)
		{
			return true;
		}
		Player player = Main.player[base.Projectile.owner];
		if (!TryGetTangentGeometry(player, radius, out var origin, out var _, out var _, out var leftPoint, out var rightPoint))
		{
			return false;
		}
		float collisionPoint = 0f;
		if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), origin, leftPoint, 28f, ref collisionPoint) || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), origin, rightPoint, 28f, ref collisionPoint))
		{
			return true;
		}
		return IsPointInsideTangentArea(targetHitbox.Center.ToVector2(), origin, base.Projectile.Center, radius);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		if (MoDaoQianRenWarmupSystem.ShouldSkipCustomDrawingForLights)
		{
			return false;
		}
		Texture2D hiltTexture = ModContent.Request<Texture2D>(MoDaoQianRenMod.WeaponHiltTexture).Value;
		Texture2D shardTexture = MoDaoQianRenShardVisuals.Texture;
		Player player = Main.player[base.Projectile.owner];
		float radius = GetRadiusOrDefault();
		DrawHandle(hiltTexture, player);
		DrawGeometryShards(shardTexture, player, radius);
		DrawFieldRing(shardTexture, radius, GetOwnerGrowthStage(player));
		for (int i = 0; i < 92; i++)
		{
			float num = (float)i * 2.399963f;
			float fill = MathF.Sqrt(((float)i + 0.5f) / 92f);
			float num2 = num + Timer * MathHelper.Lerp(0.018f, 0.052f, fill) * ((i % 2 == 0) ? 1f : (-1f));
			Vector2 radial = num2.ToRotationVector2();
			Vector2 tangent = radial.RotatedBy(1.5707963705062866);
			float tremble = MathF.Sin(Timer * 0.36f + (float)i * 1.77f) * MathHelper.Lerp(2.5f, 13f, fill);
			float pulse = MathF.Cos(Timer * 0.21f + (float)i * 0.91f) * MathHelper.Lerp(1.5f, 8f, fill);
			Vector2 position = base.Projectile.Center + radial * (radius * fill + pulse) + tangent * tremble;
			float shardScale = MathHelper.Lerp(0.12f, 0.22f, 1f - fill);
			float flicker = MoDaoQianRenShardVisuals.Flicker(Timer, i + 73, 0.05f, 0.52f);
			float rotation = num2 + base.Projectile.rotation + MathF.Sin(Timer * MathHelper.Lerp(0.08f, 0.22f, MoDaoQianRenShardVisuals.Random01(i * 23 + 9)) + (float)i) * 0.55f;
			Color outline = new Color(150, 42, 255) * (0.45f + flicker * 0.28f);
			Color shardColor = Color.Lerp(new Color(188, 98, 255), Color.White, 0.22f + flicker * 0.4f) * MathHelper.Lerp(0.72f, 1f, fill);
			Color flash = Color.Lerp(new Color(255, 165, 255), Color.White, flicker * 0.55f);
			MoDaoQianRenShardVisuals.DrawOutlinedShard(shardTexture, position - Main.screenPosition, i, outline, shardColor, flash, rotation, shardScale * MathHelper.Lerp(0.92f, 1.14f, flicker), flicker);
		}
		return false;
	}

	private void UpdatePlayerVisuals(Player player)
	{
		int direction = ((base.Projectile.Center.X >= player.Center.X) ? 1 : (-1));
		player.ChangeDir(direction);
		player.heldProj = base.Projectile.whoAmI;
		player.itemTime = 2;
		player.itemAnimation = 2;
		player.itemRotation = (base.Projectile.Center - player.MountedCenter).ToRotation() * (float)direction;
	}

	private void ProduceOrbDust()
	{
		if (!Main.dedServ)
		{
			for (int i = 0; i < 3; i++)
			{
				float radius = GetRadiusOrDefault();
				Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius) * Main.rand.NextFloat(0.35f, 1f);
				Vector2 velocity = offset.RotatedBy(1.5707963705062866).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.4f, 1.8f);
				Dust dust = Dust.NewDustPerfect(base.Projectile.Center + offset, Main.rand.NextBool(3) ? 242 : 62, velocity, 45, new Color(205, 110, 255), Main.rand.NextFloat(1f, 1.75f));
				dust.noGravity = true;
				dust.fadeIn = Main.rand.NextFloat(0.35f, 0.8f);
			}
		}
	}

	private void ProduceFieldDust()
	{
		if (!Main.dedServ && !(FieldPower <= 0.08f))
		{
			int dustCount = ((!(FieldPower > 0.65f)) ? 1 : 3);
			float radius = GetRadiusOrDefault();
			for (int i = 0; i < dustCount; i++)
			{
				Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius);
				Vector2 velocity = -offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.6f, 2.2f);
				Dust dust = Dust.NewDustPerfect(base.Projectile.Center + offset, Main.rand.NextBool(4) ? 27 : 62, velocity, 50, Color.Lerp(new Color(160, 64, 255), Color.White, FieldPower * 0.35f), Main.rand.NextFloat(1.15f, 2.1f) * MathHelper.Lerp(0.75f, 1.25f, FieldPower));
				dust.noGravity = true;
				dust.fadeIn = 0.45f;
			}
		}
	}

	private void EmitFieldEnchantmentVisuals()
	{
		if (!Main.dedServ && !base.Projectile.noEnchantments && Timer % 4f == 0f)
		{
			float radius = GetRadiusOrDefault();
			int sampleCount = ((!(FieldPower > 0.12f)) ? 1 : 2);
			for (int i = 0; i < sampleCount; i++)
			{
				Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius) * Main.rand.NextFloat(0.35f, 1f);
				Vector2 position = base.Projectile.Center + offset;
				base.Projectile.EmitEnchantmentVisualsAt(position - new Vector2(6f), 12, 12);
			}
		}
	}

	private void UpdateFieldPower(Player player, int growthStage)
	{
		if (growthStage < 5)
		{
			FieldPower = MathHelper.Lerp(FieldPower, 0f, 0.08f);
			return;
		}
		MoDaoQianRenPlayer bladePlayer = player.GetModPlayer<MoDaoQianRenPlayer>();
		bool hasCharge = bladePlayer.ShardCharge > 0;
		int consumeRate = MoDaoQianRen.ApplyMeleeAttackSpeed(player, MoDaoQianRenHeldProjectile.GetShardStreamConsumeRate(growthStage), 2);
		if (Timer % (float)consumeRate == 0f)
		{
			hasCharge = TryConsumeFieldCharge(bladePlayer, player, growthStage);
		}
		FieldPower = MathHelper.Lerp(FieldPower, hasCharge ? 1f : 0.22f, 0.16f);
	}

	private bool TryConsumeFieldCharge(MoDaoQianRenPlayer bladePlayer, Player player, int growthStage)
	{
		if (!bladePlayer.TryConsumeShardCharge(1))
		{
			TryReleaseStoredFieldBurst(player, growthStage);
			return false;
		}
		StoredBurstCharge += 1f;
		TryAutoReleaseFieldBurst(player, growthStage);
		return true;
	}

	private void TryAutoReleaseFieldBurst(Player player, int growthStage)
	{
		int fullBurstCharge = GetFullBurstCharge(growthStage);
		if (!(StoredBurstCharge < (float)fullBurstCharge))
		{
			ReleaseFieldBurst(player, growthStage, fullBurstCharge, fullBurstCharge);
			StoredBurstCharge = MathF.Max(0f, StoredBurstCharge - (float)fullBurstCharge);
		}
	}

	private void TryReleaseStoredFieldBurst(Player player, int growthStage)
	{
		if (growthStage < 5)
		{
			StoredBurstCharge = 0f;
			return;
		}
		int storedCharge = (int)MathF.Floor(StoredBurstCharge);
		if (storedCharge < GetMinimumBurstCharge(growthStage))
		{
			StoredBurstCharge = 0f;
			return;
		}
		int fullBurstCharge = GetFullBurstCharge(growthStage);
		ReleaseFieldBurst(player, growthStage, Math.Min(storedCharge, fullBurstCharge), fullBurstCharge);
		StoredBurstCharge = 0f;
	}

	private void ReleaseFieldBurst(Player player, int growthStage, int burstCharge, int fullBurstCharge)
	{
		float burstRatio = MathHelper.Clamp((float)burstCharge / (float)fullBurstCharge, 0f, 1f);
		SoundStyle style = SoundID.Item62 with
		{
			Volume = MathHelper.Lerp(0.45f, 0.78f, burstRatio),
			Pitch = MathHelper.Lerp(0.06f, -0.1f, burstRatio)
		};
		SoundEngine.PlaySound(in style, base.Projectile.Center);
		int maximumShardCount = GetMaximumBurstShardCount(growthStage);
		int shardCount = Math.Max(1, (int)MathF.Round((float)maximumShardCount * burstRatio));
		float damageMultiplier = MoDaoQianRen.GetFieldBurstDamageMultiplier(growthStage);
		float knockbackMultiplier = ((growthStage >= 7) ? 0.75f : ((growthStage >= 6) ? 0.7f : 0.55f));
		float minimumSpeed = ((growthStage >= 7) ? 17.5f : ((growthStage >= 6) ? 15f : 13.5f));
		float maximumSpeed = ((growthStage >= 7) ? 25.5f : ((growthStage >= 6) ? 22f : 19f));
		for (int i = 0; i < shardCount; i++)
		{
			Vector2 velocity = ((float)Math.PI * 2f * (float)i / (float)shardCount + Main.rand.NextFloat(-0.08f, 0.08f)).ToRotationVector2() * Main.rand.NextFloat(minimumSpeed, maximumSpeed);
			Projectile.NewProjectile(base.Projectile.GetSource_FromThis(), base.Projectile.Center + velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(18f, 46f), velocity, ModContent.ProjectileType<MoDaoQianRenShardProjectile>(), (int)((float)base.Projectile.damage * damageMultiplier), base.Projectile.knockBack * knockbackMultiplier, base.Projectile.owner, Main.rand.Next(10000), Main.rand.NextFloat(1.05f, (growthStage >= 7) ? 1.72f : ((growthStage >= 6) ? 1.55f : 1.35f)), 1f);
		}
		if (growthStage >= 7 && burstCharge >= fullBurstCharge)
		{
			ConvergenceTimer = MoDaoQianRen.GetRuntimeStats(growthStage).FinalConvergenceDuration;
			base.Projectile.netUpdate = true;
		}
	}

	private static int GetFullBurstCharge(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).FieldBurstFullCharge;
	}

	private static int GetMinimumBurstCharge(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).FieldBurstMinimumCharge;
	}

	private static int GetMaximumBurstShardCount(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).FieldBurstShardCount;
	}

	private void UpdateFinalConvergence(Player player, int growthStage)
	{
		if (growthStage < 7)
		{
			ConvergenceTimer = 0f;
		}
		else if (!(ConvergenceTimer <= 0f))
		{
			if (base.Projectile.owner == Main.myPlayer && Timer % (float)MoDaoQianRen.GetRuntimeStats(growthStage).FinalConvergencePulseRate == 0f)
			{
				FireFinalConvergenceShards(player);
			}
			if (!Main.dedServ && Main.rand.NextBool(2))
			{
				float radius = GetRadiusOrDefault();
				Vector2 offset = Main.rand.NextVector2CircularEdge(radius, radius);
				Dust dust = Dust.NewDustPerfect(base.Projectile.Center + offset, Main.rand.NextBool(3) ? 242 : 27, -offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.2f, 3.4f), 55, Color.Lerp(new Color(190, 105, 255), Color.White, 0.35f), Main.rand.NextFloat(1.25f, 2.35f));
				dust.noGravity = true;
				dust.fadeIn = 0.5f;
			}
			ConvergenceTimer -= 1f;
		}
	}

	private void FireFinalConvergenceShards(Player player)
	{
		NPC target = FindFinalConvergenceTarget(920f);
		int shardCount = ((target == null) ? 3 : 4);
		int growthStage = GetOwnerGrowthStage(player);
		int shardDamage = Math.Max(1, (int)((float)base.Projectile.damage * MoDaoQianRen.GetFinalConvergenceShardDamageMultiplier(growthStage)));
		for (int i = 0; i < shardCount; i++)
		{
			Vector2 radial = ((float)Math.PI * 2f * ((float)i / (float)shardCount) + Main.rand.NextFloat(-0.28f, 0.28f)).ToRotationVector2();
			Vector2 spawnPosition = base.Projectile.Center + radial * Main.rand.NextFloat(GetRadiusOrDefault() * 0.45f, GetRadiusOrDefault() * 0.92f);
			Vector2 velocity = ((target == null) ? radial : (target.Center - spawnPosition).SafeNormalize(radial)).RotatedBy(Main.rand.NextFloat(-0.12f, 0.12f)) * Main.rand.NextFloat(16.5f, 24f);
			Projectile.NewProjectile(base.Projectile.GetSource_FromThis(), spawnPosition, velocity, ModContent.ProjectileType<MoDaoQianRenShardProjectile>(), shardDamage, base.Projectile.knockBack * 0.52f, player.whoAmI, Main.rand.Next(10000), Main.rand.NextFloat(1.12f, 1.55f), 1f);
		}
	}

	private NPC FindFinalConvergenceTarget(float maxDistance)
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

	private void UpdateOrbRadius(Player player, int growthStage)
	{
		float distance = Vector2.Distance(player.MountedCenter, base.Projectile.Center);
		float distanceFactor = Utils.GetLerpValue(180f, 980f, distance, clamped: true);
		float minimumRadius = MathHelper.Lerp(58f, GetFieldMinimumRadius(growthStage), FieldPower);
		float maximumRadius = MathHelper.Lerp(165f, GetFieldMaximumRadius(growthStage), FieldPower);
		float desiredRadius = MathHelper.Lerp(minimumRadius, maximumRadius, distanceFactor);
		desiredRadius = MathF.Min(desiredRadius, MathF.Max(minimumRadius, distance * MathHelper.Lerp(0.82f, 0.96f, FieldPower)));
		if (CurrentOrbRadius <= 0f)
		{
			CurrentOrbRadius = desiredRadius;
		}
		else
		{
			CurrentOrbRadius = MathHelper.Lerp(CurrentOrbRadius, desiredRadius, 0.14f);
		}
	}

	private static int GetOwnerGrowthStage(Player player)
	{
		if (!MoDaoQianRen.TryGetGrowthStage(player.HeldItem, out var growthStage))
		{
			return 0;
		}
		return growthStage;
	}

	private static float GetFieldMinimumRadius(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).FieldMinimumRadius;
	}

	private static float GetFieldMaximumRadius(int growthStage)
	{
		return MoDaoQianRen.GetRuntimeStats(growthStage).FieldMaximumRadius;
	}

	private float GetRadiusOrDefault()
	{
		if (!(CurrentOrbRadius > 0f))
		{
			return 58f;
		}
		return CurrentOrbRadius;
	}

	private void DrawGeometryShards(Texture2D shardTexture, Player player, float radius)
	{
		if (TryGetTangentGeometry(player, radius, out var origin, out var center, out var distance, out var _, out var _))
		{
			Vector2 axis = (center - origin).SafeNormalize(Vector2.UnitX);
			Vector2 normal = axis.RotatedBy(1.5707963705062866);
			float areaLength = MathF.Max(48f, distance - radius * 0.25f);
			float tangentWidthFactor = MathF.Tan(MathF.Asin(MathHelper.Clamp(radius / distance, -0.95f, 0.95f)));
			for (int i = 0; i < 76; i++)
			{
				float seed = (float)i * 12.9898f;
				float lengthProgress = MathF.Pow(((float)i + 0.5f) / 76f, 0.72f);
				float along = MathHelper.Lerp(34f, areaLength, lengthProgress);
				float halfWidth = MathHelper.Clamp(along * tangentWidthFactor, 8f, radius * 1.05f);
				float wave = MathF.Sin(Timer * 0.17f + seed) * 0.5f + 0.5f;
				float side = MathHelper.Lerp(0f - halfWidth, halfWidth, wave);
				side += MathF.Cos(Timer * 0.39f + (float)i * 1.47f) * MathHelper.Lerp(2f, 18f, lengthProgress);
				along += MathF.Sin(Timer * 0.29f + (float)i * 0.83f) * MathHelper.Lerp(2f, 14f, lengthProgress);
				Vector2 position = origin + axis * along + normal * side;
				float scale = MathHelper.Lerp(0.1f, 0.2f, lengthProgress);
				float flicker = MoDaoQianRenShardVisuals.Flicker(Timer, i + 191, 0.045f, 0.46f);
				float rotation = axis.ToRotation() + MathF.Sin(Timer * MathHelper.Lerp(0.09f, 0.24f, MoDaoQianRenShardVisuals.Random01(i * 41 + 13)) + (float)i) * 0.75f;
				Color outline = new Color(132, 38, 245) * (0.34f + FieldPower * 0.2f + flicker * 0.24f);
				Color color = Color.Lerp(new Color(165, 105, 255), Color.White, 0.22f + 0.38f * flicker) * MathHelper.Lerp(0.55f, 0.95f, lengthProgress);
				Color flash = Color.Lerp(new Color(230, 150, 255), Color.White, flicker * 0.65f);
				MoDaoQianRenShardVisuals.DrawOutlinedShard(shardTexture, position - Main.screenPosition, i + 101, outline, color, flash, rotation, scale * MathHelper.Lerp(0.9f, 1.12f, flicker), flicker);
			}
		}
	}

	private void DrawFieldRing(Texture2D shardTexture, float radius, int growthStage)
	{
		if (!(FieldPower <= 0.05f))
		{
			bool finalConvergence = growthStage >= 7 && ConvergenceTimer > 0f;
			int shardCount = (finalConvergence ? 58 : 42);
			float ringRadius = radius * MathHelper.Lerp(0.9f, finalConvergence ? 1.16f : 1.08f, FieldPower);
			for (int i = 0; i < shardCount; i++)
			{
				float progress = (float)i / (float)shardCount;
				float angle = (float)Math.PI * 2f * progress - Timer * (finalConvergence ? 0.06f : 0.035f);
				Vector2 radial = angle.ToRotationVector2();
				Vector2 position = base.Projectile.Center + radial * (ringRadius + MathF.Sin(Timer * 0.18f + (float)i) * 5f * FieldPower);
				float scale = MathHelper.Lerp(0.08f, finalConvergence ? 0.19f : 0.15f, FieldPower);
				float flicker = MoDaoQianRenShardVisuals.Flicker(Timer, i + 307, finalConvergence ? 0.08f : 0.045f, finalConvergence ? 0.72f : 0.5f);
				Color outline = (finalConvergence ? new Color(230, 105, 255) : new Color(165, 60, 255)) * (0.3f + FieldPower * 0.28f + flicker * 0.22f);
				Color color = Color.Lerp(new Color(180, 88, 255), Color.White, finalConvergence ? (0.35f + flicker * 0.44f) : (0.22f + flicker * 0.36f)) * (0.42f + FieldPower * 0.45f);
				Color flash = Color.Lerp(new Color(245, 170, 255), Color.White, flicker * 0.7f);
				MoDaoQianRenShardVisuals.DrawOutlinedShard(shardTexture, position - Main.screenPosition, i + 211, outline, color, flash, angle + (float)Math.PI / 2f, scale * MathHelper.Lerp(0.92f, 1.16f, flicker), flicker);
			}
		}
	}

	private bool TryGetTangentGeometry(Player player, float radius, out Vector2 origin, out Vector2 center, out float distance, out Vector2 leftPoint, out Vector2 rightPoint)
	{
		origin = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
		center = base.Projectile.Center;
		Vector2 toCenter = center - origin;
		distance = toCenter.Length();
		if (distance <= radius + 8f)
		{
			leftPoint = center;
			rightPoint = center;
			return false;
		}
		Vector2 axis = toCenter / distance;
		float tangentAngle = MathF.Asin(MathHelper.Clamp(radius / distance, -0.95f, 0.95f));
		float tangentLength = MathF.Sqrt(MathF.Max(0f, distance * distance - radius * radius));
		leftPoint = origin + axis.RotatedBy(tangentAngle) * tangentLength;
		rightPoint = origin + axis.RotatedBy(0f - tangentAngle) * tangentLength;
		return true;
	}

	private static bool IsPointInsideTangentArea(Vector2 point, Vector2 origin, Vector2 center, float radius)
	{
		Vector2 toCenter = center - origin;
		float distance = toCenter.Length();
		if (distance <= radius + 8f)
		{
			return false;
		}
		Vector2 axis = toCenter / distance;
		Vector2 toPoint = point - origin;
		float along = Vector2.Dot(toPoint, axis);
		if (along < 0f || along > distance)
		{
			return false;
		}
		float allowedWidth = MathF.Tan(MathF.Asin(MathHelper.Clamp(radius / distance, -0.95f, 0.95f))) * along + 10f;
		return MathF.Abs(Vector2.Dot(toPoint, axis.RotatedBy(1.5707963705062866))) <= allowedWidth;
	}

	private void DrawHandle(Texture2D texture, Player player)
	{
		Vector2 handPosition = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: true);
		Vector2 direction = (base.Projectile.Center - handPosition).SafeNormalize(Vector2.UnitX * player.direction);
		SpriteEffects effects = ((((direction.X >= 0f) ? 1 : (-1)) == -1) ? SpriteEffects.FlipVertically : SpriteEffects.None);
		Vector2 origin = new Vector2(18f, 10.5f);
		float scale = GetOwnerBladeScale(player);
		Main.EntitySpriteDraw(texture, handPosition - Main.screenPosition, null, Color.White, direction.ToRotation(), origin, 0.99863356f * scale, effects);
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
