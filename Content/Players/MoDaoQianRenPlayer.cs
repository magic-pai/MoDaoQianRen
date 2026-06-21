using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using 魔刀千刃.Content.Items.Weapons;
using 魔刀千刃.Content.Projectiles;
using 魔刀千刃.Content.Systems;

namespace 魔刀千刃.Content.Players;

public class MoDaoQianRenPlayer : ModPlayer
{
	private const int BladeComboExpireTicks = 46;

	private const int GreatswordFormAnimationTicks = 32;

	private const float GreatswordWeaponOutTipVisualPadding = 34f;

	private static bool updateNoticeShownThisSession;

	public const int BladeMode = 1;

	public const int ShardPrismMode = 2;

	public const int GreatswordMode = 3;

	public const int GuardMode = 4;

	public const int CrimsonRiftMode = 5;

	public const int GuardFormNormal = 0;

	public const int GuardFormEnhanced = 1;

	public const int GuardFormSuper = 2;

	public const int DormantMaxShardCharge = 10;

	public const int AwakenedMaxShardCharge = 16;

	public const int BaseMaxShardCharge = 24;

	public const int UnboundMaxShardCharge = 35;

	public const int BladeOrbMaxShardCharge = 55;

	public const int BladeFieldMaxShardCharge = 80;

	public const int MoonlitMaxShardCharge = 100;

	public const int FinalMaxShardCharge = 140;

	public const int MaxShardCharge = 140;

	public const int CalamityDormantMaxShardCharge = 12;

	public const int CalamityAwakenedMaxShardCharge = 20;

	public const int CalamityBaseMaxShardCharge = 30;

	public const int CalamityUnboundMaxShardCharge = 44;

	public const int CalamityBladeOrbMaxShardCharge = 70;

	public const int CalamityBladeFieldMaxShardCharge = 100;

	public const int CalamityMoonlitMaxShardCharge = 128;

	public const int CalamityFinalMaxShardCharge = 180;

	private int bladeComboStep;

	private int bladeComboTimer;

	private int greatswordFormTimer;

	private bool shardPrismShearsMode;

	private bool shardPrismShearsTyphoonChanneling;

	private bool wasMiddleMousePressed;

	private bool wasRightMousePressed;

	private bool wasGuardLeftMousePressed;

	private int guardForm;

	private int lockedGuardSelectedItem = -1;

	private int autoUpgradeCheckTimer;

	private readonly List<DrawData> greatswordWeaponOutDrawData = new List<DrawData>(720);

	public int ShardCharge { get; private set; }

	public int CurrentBladeMode { get; private set; } = 1;

	public bool IsShardPrismMode => CurrentBladeMode == 2;

	public bool IsShardPrismShearsMode
	{
		get
		{
			if (IsShardPrismMode)
			{
				return shardPrismShearsMode;
			}
			return false;
		}
	}

	public bool IsGreatswordMode => CurrentBladeMode == 3 || CurrentBladeMode == 5;

	public bool IsGreatswordDevilsMode
	{
		get
		{
			return CurrentBladeMode == 5;
		}
	}

	public bool HasCrimsonRiftHoldout => Player.ownedProjectileCounts[ModContent.ProjectileType<MoDaoQianRenCrimsonRiftSlashProjectile>()] > 0;

	public bool IsGuardMode => CurrentBladeMode == 4;

	public int GuardForm => guardForm;

	public bool IsSuperGuardMode => IsGuardMode && guardForm == 2;

	public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
	{
		if (!mediumCoreDeath)
		{
			yield return new Item(ModContent.ItemType<MoDaoQianRen>());
		}
	}

	public override void OnEnterWorld()
	{
		if (base.Player.whoAmI != Main.myPlayer)
		{
			return;
		}
		if (MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode)
		{
			MoDaoQianRenWarmupSystem.StartLightsStartupSuppression();
			return;
		}
		MoDaoQianRenWarmupSystem.QueueCombatWarmup();
		if (!updateNoticeShownThisSession)
		{
			updateNoticeShownThisSession = true;
			Color headerColor = new Color(225, 150, 255);
			Color lineColor = new Color(190, 110, 255);
			Main.NewText(Language.GetTextValue("Mods.魔刀千刃.UpdateNotice.Header"), headerColor);
			Main.NewText(Language.GetTextValue("Mods.魔刀千刃.UpdateNotice.Line1"), lineColor);
			Main.NewText(Language.GetTextValue("Mods.魔刀千刃.UpdateNotice.Line2"), lineColor);
			Main.NewText(Language.GetTextValue("Mods.魔刀千刃.UpdateNotice.Line3"), lineColor);
		}
	}

	public override void PostUpdateEquips()
	{
		if (!TryGetOwnedBladeStage(out var growthStage))
		{
			ShardCharge = 0;
			CurrentBladeMode = 1;
			guardForm = 0;
			lockedGuardSelectedItem = -1;
		}
		else
		{
			ShardCharge = Utils.Clamp(ShardCharge, 0, GetMaxShardCharge(growthStage));
		}
	}

	public override void PostUpdate()
	{
		if (bladeComboTimer > 0)
		{
			bladeComboTimer--;
		}
		else
		{
			bladeComboStep = 0;
		}
		if (greatswordFormTimer > 0)
		{
			greatswordFormTimer--;
		}
		if (MoDaoQianRenWarmupSystem.ShouldBlockBladeUseForLights)
		{
			wasMiddleMousePressed = Main.mouseMiddle;
			wasRightMousePressed = Main.mouseRight;
			wasGuardLeftMousePressed = Main.mouseLeft;
			lockedGuardSelectedItem = -1;
			return;
		}
		if (base.Player.whoAmI == Main.myPlayer)
		{
			UpdateAutoUpgradeAfterBoss();
			UpdateBladeModeInput();
			if (IsGuardMode)
			{
				UpdateGuardModeInput();
				UpdateSuperGuardLock();
				UpdateGuardModeProjectiles();
			}
			else
			{
				UpdateShardPrismSkillInput();
				UpdateShardPrismShearsTyphoonInput();
				UpdateShardPrismModeProjectile();
				UpdateGreatswordBurstInput();
				UpdateGreatswordComboDashInput();
			}
		}
		UpdateShardOrbControl();
		UpdateHeldWeaponLight();
	}

	public override void TransformDrawData(ref PlayerDrawSet drawInfo)
	{
		if (MoDaoQianRenWarmupSystem.ShouldSkipCustomDrawingForLights)
		{
			if (drawInfo.heldItem?.ModItem is MoDaoQianRen && drawInfo.heldItem.type > 0 && drawInfo.heldItem.type < TextureAssets.Item.Length)
			{
				RemoveHeldItemDrawData(drawInfo.DrawDataCache, TextureAssets.Item[drawInfo.heldItem.type].Value);
			}
		}
		else
		{
			if (!(drawInfo.heldItem?.ModItem is MoDaoQianRen) && !IsGuardMode)
			{
				return;
			}
			bool hiltOnly = IsShardPrismMode || IsGuardMode;
			bool greatswordOut = IsGreatswordMode;
			Texture2D weaponOutTexture = TextureAssets.Item[drawInfo.heldItem.type].Value;
			Texture2D replacementTexture = (hiltOnly ? ModContent.Request<Texture2D>(MoDaoQianRenMod.WeaponHiltTexture).Value : (greatswordOut ? ModContent.Request<Texture2D>(MoDaoQianRenMod.WeaponHiltTexture).Value : ModContent.Request<Texture2D>("魔刀千刃/Content/Items/Weapons/MoDaoQianRen_weaponout_pulse").Value));
			Rectangle replacementFrame = ((hiltOnly || greatswordOut) ? new Rectangle(0, 0, replacementTexture.Width, replacementTexture.Height) : MoDaoQianRenHeldProjectile.GetWeaponOutPulseFrame(replacementTexture));
			float itemScale = GetHeldItemScale(drawInfo.heldItem);
			Vector2 replacementScale = Vector2.One * 0.99863356f * itemScale;
			if (drawInfo.shadow != 0f)
			{
				RemoveHeldItemDrawData(drawInfo.DrawDataCache, weaponOutTexture);
				return;
			}
			int heldItemDrawIndex = FindPrimaryHeldItemDrawIndex(drawInfo.DrawDataCache, weaponOutTexture);
			if (heldItemDrawIndex < 0)
			{
				return;
			}
			for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--)
			{
				if (i != heldItemDrawIndex && drawInfo.DrawDataCache[i].texture == weaponOutTexture)
				{
					drawInfo.DrawDataCache.RemoveAt(i);
					if (i < heldItemDrawIndex)
					{
						heldItemDrawIndex--;
					}
				}
			}
			DrawData drawData = drawInfo.DrawDataCache[heldItemDrawIndex];
			drawData.texture = replacementTexture;
			drawData.sourceRect = replacementFrame;
			drawData.scale = replacementScale;
			if (hiltOnly || greatswordOut)
			{
				float originX = 18f;
				if ((drawData.effect & SpriteEffects.FlipHorizontally) != SpriteEffects.None)
				{
					originX = (float)replacementFrame.Width - originX;
				}
				drawData.origin = new Vector2(originX, 10.5f);
			}
			else
			{
				float originX2 = 156f;
				if ((drawData.effect & SpriteEffects.FlipHorizontally) != SpriteEffects.None)
				{
					originX2 = (float)replacementFrame.Width - originX2;
				}
				drawData.origin = new Vector2(originX2, 17.5f);
			}
			drawInfo.DrawDataCache[heldItemDrawIndex] = drawData;
			if (greatswordOut)
			{
				List<DrawData> bladeDrawData = CreateGreatswordWeaponOutShardDrawData(drawData, drawInfo.heldItem);
				drawInfo.DrawDataCache.InsertRange(heldItemDrawIndex, bladeDrawData);
				heldItemDrawIndex += bladeDrawData.Count;
			}
			InsertWeaponOutGlowDrawData(drawInfo.DrawDataCache, heldItemDrawIndex, drawData, hiltOnly, greatswordOut);
		}
	}

	private void UpdateHeldWeaponLight()
	{
		if (!Main.dedServ && !MoDaoQianRenWarmupSystem.ShouldSkipCustomDrawingForLights && (base.Player.HeldItem?.ModItem is MoDaoQianRen || IsGuardMode))
		{
			float intensity = (IsGreatswordMode ? 0.86f : (IsGuardMode ? MathHelper.Lerp(0.52f, 1.05f, (float)guardForm / 2f) : (IsShardPrismMode ? 0.36f : 0.62f)));
			MoDaoQianRenWarmupSystem.AddLight(base.Player.Center + new Vector2((float)base.Player.direction * 22f, -4f), 0.42f * intensity, 0.08f * intensity, 0.82f * intensity);
			if (IsGreatswordMode)
			{
				MoDaoQianRenWarmupSystem.AddLight(base.Player.Center + new Vector2((float)base.Player.direction * 92f, -8f), 0.34f, 0.05f, 0.72f);
			}
		}
	}

	private static int FindPrimaryHeldItemDrawIndex(List<DrawData> drawDataCache, Texture2D weaponTexture)
	{
		int bestIndex = -1;
		float bestScore = float.NegativeInfinity;
		for (int i = 0; i < drawDataCache.Count; i++)
		{
			DrawData drawData = drawDataCache[i];
			if (drawData.texture == weaponTexture)
			{
				float colorPower = (float)(drawData.color.R + drawData.color.G + drawData.color.B) / 765f;
				float num = (float)(int)drawData.color.A / 255f;
				float scalePower = MathF.Abs(drawData.scale.X) + MathF.Abs(drawData.scale.Y);
				float score = num * 4f + colorPower + scalePower * 0.05f + (float)i * 0.001f;
				if (score >= bestScore)
				{
					bestScore = score;
					bestIndex = i;
				}
			}
		}
		return bestIndex;
	}

	private static void RemoveHeldItemDrawData(List<DrawData> drawDataCache, Texture2D weaponTexture)
	{
		for (int i = drawDataCache.Count - 1; i >= 0; i--)
		{
			if (drawDataCache[i].texture == weaponTexture)
			{
				drawDataCache.RemoveAt(i);
			}
		}
	}

	private static void InsertWeaponOutGlowDrawData(List<DrawData> drawDataCache, int heldItemDrawIndex, DrawData baseDrawData, bool hiltOnly, bool greatswordOut)
	{
		Color auraColor = (greatswordOut ? (new Color(162, 58, 255) * 0.34f) : (hiltOnly ? (new Color(176, 82, 255) * 0.22f) : (new Color(234, 78, 255) * 0.38f)));
		Color coreColor = (greatswordOut ? (new Color(252, 214, 255) * 0.2f) : (hiltOnly ? (new Color(235, 198, 255) * 0.14f) : (new Color(255, 220, 255) * 0.28f)));
		DrawData auraDrawData = baseDrawData;
		auraDrawData.color = auraColor;
		auraDrawData.scale = new Vector2(baseDrawData.scale.X * 1.055f, baseDrawData.scale.Y * 1.14f);
		drawDataCache.Insert(heldItemDrawIndex, auraDrawData);
		DrawData coreDrawData = baseDrawData;
		coreDrawData.color = coreColor;
		drawDataCache.Insert(heldItemDrawIndex + 2, coreDrawData);
	}

	private List<DrawData> CreateGreatswordWeaponOutShardDrawData(DrawData hiltDrawData, Item heldItem)
	{
		List<DrawData> drawData = greatswordWeaponOutDrawData;
		drawData.Clear();
		Texture2D shardTexture = MoDaoQianRenShardVisuals.Texture;
		float itemScale = GetHeldItemScale(heldItem);
		float targetLength = ((heldItem?.ModItem is MoDaoQianRen blade) ? MathHelper.Clamp(blade.GetWeaponOutStageBladeLength(), 120f, 880f) : 120f);
		float distanceFactor = MoDaoQianRenHeldProjectile.GetDistanceFactor(targetLength);
		float formProgress = 1f - (float)greatswordFormTimer / 32f;
		formProgress = SmoothStep(MathHelper.Clamp(formProgress, 0f, 1f));
		Vector2 direction = hiltDrawData.rotation.ToRotationVector2() * base.Player.direction;
		Vector2 normal = direction.RotatedBy(1.5707963705062866);
		float rotation = direction.ToRotation();
		float rootDistance = 45.937145f * itemScale;
		Vector2 rootPosition = hiltDrawData.position + direction * rootDistance;
		float visibleLength = MathHelper.Lerp(36f, MathF.Max(44f, targetLength - rootDistance), formProgress);
		float fogVisibleLength = visibleLength + GreatswordWeaponOutTipVisualPadding * itemScale * formProgress;
		float lengthBuild = MathHelper.Lerp(0.08f, 1f, formProgress);
		float halfWidth = MathHelper.Lerp(6f, MathHelper.Lerp(18f, 42f, distanceFactor), formProgress) * itemScale;
		float timer = Main.GameUpdateCount;
		AddGreatswordWeaponOutRangeAuraDrawData(drawData, rootPosition, direction, normal, fogVisibleLength, distanceFactor, halfWidth, formProgress, timer);
		int coreShardCount = (int)MathHelper.Clamp(visibleLength / 5.2f, 64f, 150f);
		for (int i = 0; i < coreShardCount; i++)
		{
			float progress = MathF.Pow(((float)i + MoDaoQianRenShardVisuals.Random01(i * 17 + 5)) / (float)coreShardCount, 1.72f);
			progress = MathHelper.Clamp(progress, 0.01f, 0.995f);
			float rootAuraFade = SmoothStep(Utils.GetLerpValue(0.14f, 0.28f, progress, clamped: true));
			float widthAtPoint = GetGreatswordOutHalfWidth(progress, halfWidth);
			float sideSeed = MoDaoQianRenShardVisuals.Random01(i * 41 + 19) * 2f - 1f;
			float sideOffset = (float)MathF.Sign(sideSeed) * MathF.Pow(MathF.Abs(sideSeed), 0.72f) * widthAtPoint * MathF.Sqrt(MoDaoQianRenShardVisuals.Random01(i * 59 + 29));
			float formScatter = (1f - formProgress) * MathHelper.Lerp(3f, 28f, MoDaoQianRenShardVisuals.Random01(i * 73 + 11));
			Vector2 assembleOffset = -direction * formScatter * MathHelper.Lerp(0.05f, 0.8f, progress) + normal * sideSeed * formScatter * 0.48f;
			float ripple = MathF.Sin(timer * MathHelper.Lerp(0.08f, 0.18f, MoDaoQianRenShardVisuals.Random01(i * 31 + 7)) + (float)i * 1.71f) * MathHelper.Lerp(0.35f, 2.4f, progress) * formProgress;
			Vector2 position = rootPosition + direction * (MathHelper.Lerp(visibleLength, fogVisibleLength, MathF.Pow(progress, 2.4f)) * progress * lengthBuild + MathHelper.Lerp(-4.2f, 4.2f, MoDaoQianRenShardVisuals.Random01(i * 43 + 13)) * formProgress) + normal * (sideOffset * formProgress + ripple) + assembleOffset;
			float flicker = MoDaoQianRenShardVisuals.Flicker(timer, i + 3000, 0.07f, 0.82f);
			float densityScale = MathHelper.Lerp(1.08f, 0.52f, progress);
			float scale = MathHelper.Lerp(0.058f, 0.021f, progress) * MathHelper.Lerp(0.72f, 1.14f, MoDaoQianRenShardVisuals.Random01(i * 67 + 31)) * MathHelper.Lerp(0.8f, 1.2f, flicker) * itemScale * densityScale;
			Rectangle source = MoDaoQianRenShardVisuals.GetFrame(i + 3300);
			Vector2 origin = source.Size() * 0.5f;
			float auraStrength = MathHelper.Lerp(0.015f, MathHelper.Lerp(0.16f, 0.06f, progress), rootAuraFade);
			Color aura = new Color(126, 18, 255) * auraStrength;
			Color core = Color.Lerp(new Color(196, 66, 255), Color.White, 0.18f + flicker * 0.45f) * MathHelper.Lerp(0.74f, 0.42f, progress) * (0.62f + formProgress * 0.38f);
			Color flash = Color.Lerp(new Color(255, 140, 255), Color.White, flicker * 0.72f);
			float shardRotation = rotation + sideSeed * MathHelper.Lerp(0.24f, 0.88f, progress) + MathF.Sin(timer * 0.17f + (float)i) * MathHelper.Lerp(0.08f, 0.24f, formProgress);
			if (i % 6 == 0 && progress > 0.16f)
			{
				drawData.Add(new DrawData(shardTexture, position, source, aura * (0.18f + flicker * 0.12f), shardRotation, origin, scale * (1.65f + flicker * 0.16f), SpriteEffects.None));
			}
			drawData.Add(new DrawData(shardTexture, position, source, core, shardRotation, origin, scale, SpriteEffects.None));
			if (flicker > 0.96f && i % 9 == 0 && progress > 0.18f)
			{
				drawData.Add(new DrawData(shardTexture, position, source, flash * MathHelper.Clamp((flicker - 0.82f) / 0.36f, 0f, 0.38f), shardRotation, origin, scale * 0.62f, SpriteEffects.None));
			}
		}
		int looseShardCount = (int)MathHelper.Clamp(visibleLength / 13f, 14f, 46f);
		for (int j = 0; j < looseShardCount; j++)
		{
			float progress2 = MathF.Pow(((float)j + MoDaoQianRenShardVisuals.Random01(j * 29 + 503)) / (float)looseShardCount, 1.98f);
			progress2 = MathHelper.Clamp(progress2, 0.015f, 0.995f);
			float widthAtPoint2 = GetGreatswordOutHalfWidth(progress2, halfWidth);
			float sideSeed2 = MoDaoQianRenShardVisuals.Random01(j * 53 + 11) * 2f - 1f;
			float sideOffset2 = sideSeed2 * widthAtPoint2 * MathHelper.Lerp(0.92f, 1.42f, MoDaoQianRenShardVisuals.Random01(j * 71 + 13));
			float formScatter2 = (1f - formProgress) * MathHelper.Lerp(5f, 34f, MoDaoQianRenShardVisuals.Random01(j * 47 + 23));
			Vector2 position2 = rootPosition + direction * (MathHelper.Lerp(visibleLength, fogVisibleLength, MathF.Pow(progress2, 2.2f)) * progress2 * lengthBuild + MathHelper.Lerp(-8f, 4f, MoDaoQianRenShardVisuals.Random01(j * 89 + 43)) * formProgress) + normal * (sideOffset2 * formProgress + MathF.Sin(timer * 0.11f + (float)j * 1.31f) * MathHelper.Lerp(0.4f, 3.2f, progress2) * formProgress) - direction * formScatter2 * MathHelper.Lerp(0.05f, 0.9f, progress2) + normal * sideSeed2 * formScatter2 * 0.55f;
			float flicker2 = MoDaoQianRenShardVisuals.Flicker(timer, j + 4300, 0.08f, 0.72f);
			float scale2 = MathHelper.Lerp(0.04f, 0.014f, progress2) * MathHelper.Lerp(0.72f, 1.08f, MoDaoQianRenShardVisuals.Random01(j * 37 + 19)) * MathHelper.Lerp(0.72f, 1.18f, flicker2) * itemScale;
			Rectangle source2 = MoDaoQianRenShardVisuals.GetFrame(j + 4300);
			Vector2 origin2 = source2.Size() * 0.5f;
			float shardRotation2 = rotation + sideSeed2 * MathHelper.Lerp(0.3f, 0.95f, progress2) + MathF.Sin(timer * 0.17f + (float)j * 0.7f) * 0.2f;
			drawData.Add(new DrawData(shardTexture, position2, source2, Color.Lerp(new Color(174, 58, 255), new Color(255, 210, 255), flicker2 * 0.44f) * MathHelper.Lerp(0.42f, 0.18f, progress2), shardRotation2, origin2, scale2, SpriteEffects.None));
		}
		return drawData;
	}

	private static void AddGreatswordWeaponOutRangeAuraDrawData(List<DrawData> drawData, Vector2 rootPosition, Vector2 direction, Vector2 normal, float visibleLength, float distanceFactor, float baseHalfWidth, float formProgress, float timer)
	{
		if (!(formProgress <= 0.025f) && !(visibleLength <= 8f))
		{
			float auraPower = MathHelper.Lerp(0.44f, 0.54f, distanceFactor) * SmoothStep(formProgress);
			MoDaoQianRenGreatswordFogVisuals.AddDrawData(drawData, rootPosition, direction, normal, visibleLength, (float progress) => GetGreatswordOutHalfWidth(progress, baseHalfWidth), distanceFactor, auraPower, timer, 7100);
		}
	}

	private float GetHeldItemScale(Item heldItem)
	{
		float scale = heldItem?.scale ?? 1f;
		if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f)
		{
			scale = 1f;
		}
		return MathHelper.Clamp(scale, 0.25f, 3f);
	}

	private static float GetGreatswordOutHalfWidth(float progress, float halfWidth)
	{
		progress = MathHelper.Clamp(progress, 0f, 1f);
		float rootGather = MathHelper.Lerp(0.34f, 1f, SmoothStep(Utils.GetLerpValue(0f, 0.22f, progress, clamped: true)));
		float tipFade = MathHelper.Lerp(1f, 0.46f, SmoothStep(Utils.GetLerpValue(0.78f, 1f, progress, clamped: true)));
		float chip = (MoDaoQianRenShardVisuals.Random01((int)(progress * 997f) + 1201) - 0.5f) * 0.09f;
		return halfWidth * MathHelper.Clamp(rootGather * tipFade + chip, 0.28f, 1.08f);
	}

	private static float SmoothStep(float value)
	{
		value = MathHelper.Clamp(value, 0f, 1f);
		return value * value * (3f - 2f * value);
	}

	public int GetNextBladeAttackMode(int growthStage)
	{
		if (bladeComboTimer <= 0 || bladeComboStep >= 3)
		{
			bladeComboStep = 0;
		}
		int currentStep = bladeComboStep;
		bladeComboStep = (bladeComboStep + 1) % 3;
		bladeComboTimer = 46;
		return currentStep switch
		{
			2 => 3, 
			1 => 2, 
			_ => 0, 
		};
	}

	public int GetNextGreatswordAttackMode(int growthStage)
	{
		if (bladeComboTimer <= 0 || bladeComboStep >= 3)
		{
			bladeComboStep = 0;
		}
		int currentStep = bladeComboStep;
		bladeComboStep = (bladeComboStep + 1) % 3;
		bladeComboTimer = 160;
		return currentStep switch
		{
			2 => 7, 
			1 => 6, 
			_ => 5, 
		};
	}

	public void AddShardCharge(int amount)
	{
		ShardCharge = Utils.Clamp(ShardCharge + amount, 0, GetCurrentMaxShardCharge());
	}

	public bool TryConsumeShardCharge(int amount)
	{
		if (ShardCharge < amount)
		{
			return false;
		}
		ShardCharge -= amount;
		return true;
	}

	public int GetCurrentMaxShardCharge()
	{
		if (!TryGetOwnedBladeStage(out var growthStage))
		{
			return GetMaxShardCharge(0);
		}
		return GetMaxShardCharge(growthStage);
	}

	public static int GetMaxShardCharge(int growthStage)
	{
		return MoDaoQianRenStageParameterSystem.GetStageStats(growthStage).MaxShardCharge;
	}

	private void UpdateAutoUpgradeAfterBoss()
	{
		if (!MoDaoQianRenStageParameterSystem.AutoUpgradeAfterBoss || Main.gameMenu || base.Player.dead)
		{
			return;
		}
		if (autoUpgradeCheckTimer > 0)
		{
			autoUpgradeCheckTimer--;
			return;
		}
		autoUpgradeCheckTimer = 30;
		if (TryAutoUpgradeBlade(base.Player.HeldItem))
		{
			return;
		}
		if (TryAutoUpgradeBladeArray(base.Player.inventory))
		{
			return;
		}
		if (TryAutoUpgradeBladeArray(base.Player.bank.item))
		{
			return;
		}
		if (TryAutoUpgradeBladeArray(base.Player.bank2.item))
		{
			return;
		}
		if (TryAutoUpgradeBladeArray(base.Player.bank3.item))
		{
			return;
		}
		TryAutoUpgradeBladeArray(base.Player.bank4.item);
	}

	private bool TryAutoUpgradeBladeArray(Item[] items)
	{
		if (items == null)
		{
			return false;
		}
		for (int i = 0; i < items.Length; i++)
		{
			if (TryAutoUpgradeBlade(items[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool TryAutoUpgradeBlade(Item item)
	{
		if (item?.ModItem is not MoDaoQianRen blade || !blade.TryAutoUpgradeAfterBoss(base.Player, out var message))
		{
			return false;
		}
		if (!string.IsNullOrWhiteSpace(message))
		{
			Main.NewText(message, new Color(190, 95, 255));
		}
		autoUpgradeCheckTimer = 60;
		return true;
	}

	private void UpdateBladeModeInput()
	{
		bool hasHeldBlade = MoDaoQianRen.TryGetGrowthStage(base.Player.HeldItem, out var heldGrowthStage);
		bool hasOwnedBlade = MoDaoQianRen.TryGetOwnedGrowthStage(base.Player, out var ownedGrowthStage);
		if (MoDaoQianRenUISystem.IsStageEditorOpen || (!hasHeldBlade && (!IsGuardMode || !hasOwnedBlade)))
		{
			return;
		}
		int growthStage = hasHeldBlade ? heldGrowthStage : ownedGrowthStage;
		if (!IsModeUnlocked(CurrentBladeMode, growthStage) && CurrentBladeMode != 1)
		{
			SetBladeMode(1, showFeedback: false);
		}
		if (!hasHeldBlade && IsGuardMode)
		{
			return;
		}
		ModKeybind switchToBladeModeKeybind = MoDaoQianRenUISystem.SwitchToBladeModeKeybind;
		if (switchToBladeModeKeybind != null && switchToBladeModeKeybind.JustPressed)
		{
			SetBladeMode(1);
			return;
		}
		ModKeybind switchToShardPrismModeKeybind = MoDaoQianRenUISystem.SwitchToShardPrismModeKeybind;
		if (switchToShardPrismModeKeybind != null && switchToShardPrismModeKeybind.JustPressed)
		{
			TrySetAdvancedBladeMode(2, growthStage);
			return;
		}
		ModKeybind switchToGreatswordModeKeybind = MoDaoQianRenUISystem.SwitchToGreatswordModeKeybind;
		if (switchToGreatswordModeKeybind != null && switchToGreatswordModeKeybind.JustPressed)
		{
			TrySetAdvancedBladeMode(3, growthStage);
			return;
		}
		ModKeybind switchToGuardModeKeybind = MoDaoQianRenUISystem.SwitchToGuardModeKeybind;
		if (switchToGuardModeKeybind != null && switchToGuardModeKeybind.JustPressed)
		{
			TrySetAdvancedBladeMode(4, growthStage);
			return;
		}
		ModKeybind switchGreatswordDevilsModeKeybind = MoDaoQianRenUISystem.SwitchGreatswordDevilsModeKeybind;
		if (switchGreatswordDevilsModeKeybind != null && switchGreatswordDevilsModeKeybind.JustPressed)
		{
			TrySetAdvancedBladeMode(5, growthStage);
		}
	}

	private static bool IsModeUnlocked(int mode, int growthStage)
	{
		return mode switch
		{
			5 => MoDaoQianRen.IsGreatswordDevilsModeUnlocked(growthStage), 
			4 => MoDaoQianRen.IsGuardModeUnlocked(growthStage), 
			3 => MoDaoQianRen.IsGreatswordModeUnlocked(growthStage), 
			2 => MoDaoQianRen.IsShardPrismModeUnlocked(growthStage), 
			_ => true, 
		};
	}

	private bool TrySetAdvancedBladeMode(int mode, int growthStage)
	{
		if (!IsModeUnlocked(mode, growthStage))
		{
			string lockKey = mode switch
			{
				5 => "Mode5Locked", 
				4 => "Mode4Locked", 
				3 => "Mode3Locked", 
				_ => "Mode2Locked", 
			};
			Main.NewText(Language.GetTextValue("Mods." + base.Mod.Name + ".Items.MoDaoQianRen." + lockKey), new Color(190, 110, 255));
			SoundEngine.PlaySound(SoundID.MenuClose with
			{
				Volume = 0.45f
			}, base.Player.Center);
			return false;
		}
		SetBladeMode(mode);
		return true;
	}

	private void SetBladeMode(int mode, bool showFeedback = true)
	{
		mode = mode switch
		{
			5 => 5, 
			4 => 4, 
			3 => 3, 
			2 => 2, 
			_ => 1, 
		};
		if (CurrentBladeMode != mode)
		{
			int previousMode = CurrentBladeMode;
			CurrentBladeMode = mode;
			bladeComboStep = 0;
			bladeComboTimer = 0;
			greatswordFormTimer = ((mode == 3 || mode == 5) ? 32 : 0);
			lockedGuardSelectedItem = ((mode == 4) ? base.Player.selectedItem : -1);
			if (mode != 2 || previousMode != 2)
			{
				shardPrismShearsMode = false;
			}
			if (showFeedback)
			{
				Color color = mode switch
				{
					5 => new Color(255, 62, 82), 
					4 => new Color(176, 120, 255), 
					3 => new Color(245, 95, 255), 
					2 => new Color(206, 112, 255), 
					_ => new Color(230, 205, 255), 
				};
				Main.NewText(mode switch
				{
					2 => "Mode 2: Shard Orb", 
					3 => "Mode 3: Reforged Greatsword", 
					4 => "Mode 4: Thousand Guard", 
					5 => "Mode 5: Crimson Rift", 
					_ => "Mode 1: Blade", 
				}, color);
				SoundStyle style = SoundID.Item4 with
				{
					Volume = 0.45f,
					Pitch = mode switch
					{
						5 => -0.5f, 
						4 => -0.18f, 
						3 => -0.34f, 
						2 => 0.18f, 
						_ => -0.08f, 
					}
				};
				SoundEngine.PlaySound(in style, base.Player.Center);
			}
		}
	}

	private void UpdateShardPrismSkillInput()
	{
		if (!IsShardPrismMode)
		{
			return;
		}
		bool middlePressed = Main.mouseMiddle;
		bool num = middlePressed && !wasMiddleMousePressed;
		wasMiddleMousePressed = middlePressed;
		if (num && !MoDaoQianRenUISystem.IsStageEditorOpen && !base.Player.noItems && !base.Player.CCed && base.Player.HeldItem?.ModItem is MoDaoQianRen)
		{
			shardPrismShearsMode = !shardPrismShearsMode;
			if (shardPrismShearsMode)
			{
				KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenPrismBladeProjectile>());
				KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenPrismShardProjectile>());
				Main.NewText("Mode 2: Shears", new Color(178, 216, 232));
				SoundStyle style = SoundID.Item4 with
				{
					Volume = 0.42f,
					Pitch = 0.38f
				};
				SoundEngine.PlaySound(in style, base.Player.Center);
			}
			else
			{
				KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenShearsProjectile>());
				KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenShearsTyphoonProjectile>());
				Main.NewText("Mode 2: Shard Orb", new Color(206, 112, 255));
				SoundStyle style = SoundID.Item4 with
				{
					Volume = 0.42f,
					Pitch = 0.18f
				};
				SoundEngine.PlaySound(in style, base.Player.Center);
			}
		}
	}

	public float GetGuardShardCapacityMultiplier()
	{
		if (!IsGuardMode)
		{
			return 1f;
		}
		MoDaoQianRenStageStats stats = GetBestOwnedRuntimeStats();
		return guardForm switch
		{
			2 => stats.GuardSuperShardCapacityMultiplier, 
			1 => stats.GuardWallShardCapacityMultiplier, 
			_ => stats.GuardNormalShardCapacityMultiplier, 
		};
	}

	private float GetGuardDamageMultiplier()
	{
		if (!IsGuardMode)
		{
			return 1f;
		}
		MoDaoQianRenStageStats stats = GetBestOwnedRuntimeStats();
		return guardForm switch
		{
			2 => stats.GuardSuperDamageMultiplier, 
			1 => stats.GuardWallDamageMultiplier, 
			_ => stats.GuardNormalDamageMultiplier, 
		};
	}

	public override bool CanUseItem(Item item)
	{
		return !IsSuperGuardMode || !HasHeldOrLockedGuardBlade();
	}

	public override bool CanConsumeAmmo(Item weapon, Item ammo)
	{
		return !IsSuperGuardMode || !HasHeldOrLockedGuardBlade();
	}

	public override void ModifyHurt(ref Player.HurtModifiers modifiers)
	{
		if (IsGuardMode && HasOwnedGuardBlade())
		{
			modifiers.FinalDamage *= GetGuardDamageMultiplier();
		}
	}

	private bool HasOwnedGuardBlade()
	{
		return MoDaoQianRen.TryGetOwnedGrowthStage(base.Player, out var growthStage) && MoDaoQianRen.IsGuardModeUnlocked(growthStage);
	}

	private bool HasHeldOrLockedGuardBlade()
	{
		if (base.Player.HeldItem?.ModItem is MoDaoQianRen)
		{
			return true;
		}
		if (IsSuperGuardMode && lockedGuardSelectedItem >= 0 && lockedGuardSelectedItem < base.Player.inventory.Length)
		{
			return base.Player.inventory[lockedGuardSelectedItem]?.ModItem is MoDaoQianRen;
		}
		return false;
	}

	private void UpdateGuardModeInput()
	{
		bool mouseLeft = Main.mouseLeft;
		bool leftPressed = mouseLeft && !wasGuardLeftMousePressed;
		bool middlePressed = Main.mouseMiddle && !wasMiddleMousePressed;
		bool rightPressed = Main.mouseRight && !wasRightMousePressed;
		wasMiddleMousePressed = Main.mouseMiddle;
		wasRightMousePressed = Main.mouseRight;
		if (!IsGuardMode || MoDaoQianRenUISystem.IsStageEditorOpen)
		{
			wasGuardLeftMousePressed = mouseLeft;
			return;
		}
		if (leftPressed)
		{
			SetGuardForm(0);
		}
		else if (middlePressed)
		{
			SetGuardForm(1);
		}
		else if (rightPressed)
		{
			SetGuardForm(2);
		}
		wasGuardLeftMousePressed = mouseLeft;
	}

	private void SetGuardForm(int form)
	{
		form = Utils.Clamp(form, 0, 2);
		if (IsGuardMode && guardForm != form)
		{
			guardForm = form;
			lockedGuardSelectedItem = (form == 2) ? base.Player.selectedItem : -1;
			Color color = form switch
			{
				2 => new Color(240, 210, 255), 
				1 => new Color(202, 150, 255), 
				_ => new Color(176, 120, 255), 
			};
			Main.NewText(form switch
			{
				2 => "Mode 4: Super Guard", 
				1 => "Mode 4: Guard Wall", 
				_ => "Mode 4: Thousand Guard", 
			}, color);
			SoundStyle style = SoundID.Item4 with
			{
				Volume = 0.42f,
				Pitch = form switch
				{
					2 => -0.42f, 
					1 => -0.24f, 
					_ => -0.08f, 
				}
			};
			SoundEngine.PlaySound(in style, base.Player.Center);
		}
	}

	private void UpdateSuperGuardLock()
	{
		if (IsSuperGuardMode)
		{
			if (lockedGuardSelectedItem < 0 || lockedGuardSelectedItem >= base.Player.inventory.Length)
			{
				lockedGuardSelectedItem = base.Player.selectedItem;
			}
			base.Player.selectedItem = lockedGuardSelectedItem;
			base.Player.controlUseItem = false;
			base.Player.controlUseTile = false;
			base.Player.channel = false;
			return;
		}
		lockedGuardSelectedItem = -1;
	}

	private void UpdateGuardModeProjectiles()
	{
		if (!IsGuardMode || base.Player.CCed || !TryGetBestOwnedBlade(out var bladeItem, out var blade))
		{
			KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenGuardFieldProjectile>());
			return;
		}
		blade.ApplyModeStats(base.Player);
		int fieldType = ModContent.ProjectileType<MoDaoQianRenGuardFieldProjectile>();
		if (base.Player.ownedProjectileCounts[fieldType] <= 0)
		{
			Projectile.NewProjectile(base.Player.GetSource_ItemUse(bladeItem), base.Player.Center, Vector2.Zero, fieldType, 0, 0f, base.Player.whoAmI, guardForm);
		}
		UpdateGuardPrismProjectile(bladeItem, blade);
	}

	private void UpdateGuardPrismProjectile(Item bladeItem, MoDaoQianRen blade)
	{
		KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenShearsProjectile>());
		KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenShearsTyphoonProjectile>());
		int prismType = ModContent.ProjectileType<MoDaoQianRenPrismBladeProjectile>();
		if (base.Player.ownedProjectileCounts[prismType] <= 0)
		{
			Vector2 spawnOffset = new Vector2((float)base.Player.direction * 96f, -58f);
			int prismDamage = Math.Max(1, (int)MathF.Round((float)base.Player.GetWeaponDamage(bladeItem) * MoDaoQianRen.GetShardPrismDamageMultiplier(blade.GrowthStage)));
			int prismIndex = Projectile.NewProjectile(base.Player.GetSource_ItemUse(bladeItem), base.Player.Center + spawnOffset, Vector2.Zero, prismType, prismDamage, base.Player.GetWeaponKnockback(bladeItem, bladeItem.knockBack), base.Player.whoAmI);
			if (prismIndex >= 0 && prismIndex < Main.maxProjectiles)
			{
				Main.projectile[prismIndex].originalDamage = prismDamage;
			}
		}
	}

	private void UpdateShardPrismModeProjectile()
	{
		if (!IsShardPrismMode || base.Player.noItems || base.Player.CCed || !(base.Player.HeldItem?.ModItem is MoDaoQianRen blade))
		{
			return;
		}
		blade.ApplyModeStats(base.Player);
		if (shardPrismShearsMode)
		{
			KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenPrismBladeProjectile>());
			KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenPrismShardProjectile>());
			if (shardPrismShearsTyphoonChanneling)
			{
				KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenShearsProjectile>());
			}
			else
			{
				UpdateShardPrismShearsProjectile();
			}
			return;
		}
		KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenShearsProjectile>());
		KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenShearsTyphoonProjectile>());
		int prismType = ModContent.ProjectileType<MoDaoQianRenPrismBladeProjectile>();
		if (base.Player.ownedProjectileCounts[prismType] <= 0)
		{
			Vector2 spawnOffset = new Vector2((float)base.Player.direction * 96f, -58f);
			int prismDamage = Math.Max(1, (int)MathF.Round((float)base.Player.GetWeaponDamage(base.Player.HeldItem) * MoDaoQianRen.GetShardPrismDamageMultiplier(blade.GrowthStage)));
			int prismIndex = Projectile.NewProjectile(base.Player.GetSource_ItemUse(base.Player.HeldItem), base.Player.Center + spawnOffset, Vector2.Zero, prismType, prismDamage, base.Player.GetWeaponKnockback(base.Player.HeldItem, base.Player.HeldItem.knockBack), base.Player.whoAmI);
			if (prismIndex >= 0 && prismIndex < Main.maxProjectiles)
			{
				Main.projectile[prismIndex].originalDamage = prismDamage;
			}
		}
	}

	private void UpdateShardPrismShearsProjectile()
	{
		int shearsType = ModContent.ProjectileType<MoDaoQianRenShearsProjectile>();
		int shearsCapacity = GetShardPrismShearsCapacity();
		int shearsDamage = GetShardPrismShearsDamage();
		int shearsOriginalDamage = GetShardPrismShearsOriginalDamage(shearsDamage);
		float shearsKnockback = base.Player.GetWeaponKnockback(base.Player.HeldItem, base.Player.HeldItem.knockBack);
		int shearsCount = SyncOwnedShears(shearsType, shearsCapacity, shearsDamage, shearsOriginalDamage, shearsKnockback);
		if (shearsCount >= shearsCapacity)
		{
			return;
		}
		for (int slot = shearsCount; slot < shearsCapacity; slot++)
		{
			Vector2 spawnOffset = GetShardPrismShearsSpawnOffset(slot, shearsCapacity);
			int shearsIndex = Projectile.NewProjectile(base.Player.GetSource_ItemUse(base.Player.HeldItem), base.Player.Center + spawnOffset, Vector2.Zero, shearsType, shearsDamage, shearsKnockback, base.Player.whoAmI, 0f, 0f, slot);
			if (shearsIndex >= 0 && shearsIndex < Main.maxProjectiles)
			{
				Projectile obj = Main.projectile[shearsIndex];
				obj.originalDamage = shearsOriginalDamage;
				obj.ai[2] = slot;
				obj.netUpdate = true;
			}
		}
	}

	private void UpdateShardPrismShearsTyphoonInput()
	{
		shardPrismShearsTyphoonChanneling = false;
		if (!IsShardPrismShearsMode)
		{
			KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenShearsTyphoonProjectile>());
			return;
		}
		if (!Main.mouseRight || MoDaoQianRenUISystem.IsStageEditorOpen || base.Player.noItems || base.Player.CCed || !(base.Player.HeldItem?.ModItem is MoDaoQianRen))
		{
			KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenShearsTyphoonProjectile>());
			return;
		}
		shardPrismShearsTyphoonChanneling = true;
		KillOwnedProjectileType(ModContent.ProjectileType<MoDaoQianRenShearsProjectile>());
		int typhoonType = ModContent.ProjectileType<MoDaoQianRenShearsTyphoonProjectile>();
		int typhoonDamage = GetShardPrismShearsTyphoonDamage();
		int typhoonOriginalDamage = GetShardPrismShearsOriginalDamage(typhoonDamage);
		float typhoonKnockback = base.Player.GetWeaponKnockback(base.Player.HeldItem, base.Player.HeldItem.knockBack);
		if (base.Player.ownedProjectileCounts[typhoonType] > 0)
		{
			SyncOwnedShearsTyphoon(typhoonType, typhoonDamage, typhoonOriginalDamage, typhoonKnockback);
			return;
		}
		int typhoonIndex = Projectile.NewProjectile(base.Player.GetSource_ItemUse(base.Player.HeldItem), Main.MouseWorld, Vector2.Zero, typhoonType, typhoonDamage, typhoonKnockback, base.Player.whoAmI, Main.MouseWorld.X, Main.MouseWorld.Y);
		if (typhoonIndex >= 0 && typhoonIndex < Main.maxProjectiles)
		{
			Projectile obj = Main.projectile[typhoonIndex];
			obj.originalDamage = typhoonOriginalDamage;
			obj.netUpdate = true;
		}
	}

	private int GetShardPrismShearsCapacity()
	{
		return Math.Max(1, base.Player.maxMinions);
	}

	private int GetShardPrismShearsDamage()
	{
		int summonPanelDamage = base.Player.GetWeaponDamage(base.Player.HeldItem);
		int growthStage = GetHeldBladeStageOrDefault();
		return Math.Max(1, (int)MathF.Round((float)summonPanelDamage * MoDaoQianRen.GetRuntimeStats(growthStage).ShearsSummonDamageMultiplier));
	}

	private int GetShardPrismShearsTyphoonDamage()
	{
		int summonPanelDamage = base.Player.GetWeaponDamage(base.Player.HeldItem);
		int growthStage = GetHeldBladeStageOrDefault();
		return Math.Max(1, (int)MathF.Round((float)summonPanelDamage * MoDaoQianRen.GetRuntimeStats(growthStage).ShearsTyphoonDamageMultiplier));
	}

	private int GetShardPrismShearsOriginalDamage(int shearsDamage)
	{
		float originalDamage = base.Player.GetTotalDamage(DamageClass.Summon).Undo(shearsDamage);
		if (float.IsNaN(originalDamage) || float.IsInfinity(originalDamage) || originalDamage <= 0f)
		{
			return shearsDamage;
		}
		return Math.Max(1, (int)MathF.Round(originalDamage));
	}

	private void SyncOwnedShearsTyphoon(int typhoonType, int typhoonDamage, int typhoonOriginalDamage, float typhoonKnockback)
	{
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile typhoon = Main.projectile[i];
			if (typhoon.active && typhoon.owner == base.Player.whoAmI && typhoon.type == typhoonType)
			{
				bool needsNetUpdate = false;
				if (typhoon.damage != typhoonDamage || typhoon.originalDamage != typhoonOriginalDamage || MathF.Abs(typhoon.knockBack - typhoonKnockback) > 0.001f)
				{
					typhoon.damage = typhoonDamage;
					typhoon.originalDamage = typhoonOriginalDamage;
					typhoon.knockBack = typhoonKnockback;
					needsNetUpdate = true;
				}
				if (needsNetUpdate)
				{
					typhoon.netUpdate = true;
				}
			}
		}
	}

	private int SyncOwnedShears(int shearsType, int shearsCapacity, int shearsDamage, int shearsOriginalDamage, float shearsKnockback)
	{
		int shearsCount = 0;
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile shears = Main.projectile[i];
			if (!shears.active || shears.owner != base.Player.whoAmI || shears.type != shearsType)
			{
				continue;
			}
			if (shearsCount >= shearsCapacity)
			{
				shears.Kill();
				continue;
			}
			bool needsNetUpdate = false;
			if ((int)shears.ai[2] != shearsCount)
			{
				shears.ai[2] = shearsCount;
				needsNetUpdate = true;
			}
			if (shears.damage != shearsDamage || shears.originalDamage != shearsOriginalDamage || MathF.Abs(shears.knockBack - shearsKnockback) > 0.001f)
			{
				shears.damage = shearsDamage;
				shears.originalDamage = shearsOriginalDamage;
				shears.knockBack = shearsKnockback;
				needsNetUpdate = true;
			}
			if (needsNetUpdate)
			{
				shears.netUpdate = true;
			}
			shearsCount++;
		}
		return shearsCount;
	}

	private Vector2 GetShardPrismShearsSpawnOffset(int slot, int shearsCapacity)
	{
		if (shearsCapacity <= 1)
		{
			return new Vector2((float)base.Player.direction * 112f, -62f);
		}
		float centeredSlot = (float)slot - ((float)shearsCapacity - 1f) * 0.5f;
		return (-(float)Math.PI / 2f + centeredSlot * 0.34f).ToRotationVector2() * 96f + new Vector2((float)base.Player.direction * 38f, -4f);
	}

	private void KillOwnedProjectileType(int projectileType)
	{
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile projectile = Main.projectile[i];
			if (projectile.active && projectile.owner == base.Player.whoAmI && projectile.type == projectileType)
			{
				projectile.Kill();
			}
		}
	}

	private void UpdateGreatswordBurstInput()
	{
		bool middlePressed = Main.mouseMiddle;
		bool num = middlePressed && !wasMiddleMousePressed;
		wasMiddleMousePressed = middlePressed;
		if (!num || MoDaoQianRenUISystem.IsStageEditorOpen || !IsGreatswordMode || base.Player.noItems || base.Player.CCed || !(base.Player.HeldItem?.ModItem is MoDaoQianRen blade))
		{
			return;
		}
		blade.ApplyModeStats(base.Player);
		int burstType = ModContent.ProjectileType<MoDaoQianRenHeldProjectile>();
		if (base.Player.ownedProjectileCounts[burstType] > 0)
		{
			return;
		}
		if (ShardCharge <= 0)
		{
			SoundStyle style = SoundID.MenuTick with
			{
				Volume = 0.45f,
				Pitch = -0.24f
			};
			SoundEngine.PlaySound(in style, base.Player.Center);
			Main.NewText(Language.GetTextValue("Mods." + base.Mod.Name + ".Items.MoDaoQianRen.Mode3BurstNeedCharge"), new Color(210, 120, 255));
			return;
		}
		Vector2 handPosition = base.Player.RotatedRelativePoint(base.Player.MountedCenter, reverseRotation: true);
		Vector2 aim = Main.MouseWorld - handPosition;
		if (aim == Vector2.Zero || aim.HasNaNs())
		{
			aim = Vector2.UnitX * base.Player.direction;
		}
		Vector2 direction = aim.SafeNormalize(Vector2.UnitX * base.Player.direction);
		float initialLength = MathHelper.Clamp(blade.GetScaledStageBladeLength(base.Player), 120f, 880f);
		MoDaoQianRenStageStats stats = MoDaoQianRen.GetRuntimeStats(blade.GrowthStage);
		int damage = Math.Max(1, (int)MathF.Round((float)base.Player.GetWeaponDamage(base.Player.HeldItem) * stats.GreatswordBurstDamageMultiplier));
		float knockback = base.Player.GetWeaponKnockback(base.Player.HeldItem, base.Player.HeldItem.knockBack) * MathF.Max(1f, MoDaoQianRen.GetGreatswordKnockbackMultiplier(blade.GrowthStage, 7));
		int projectileIndex = Projectile.NewProjectile(base.Player.GetSource_ItemUse(base.Player.HeldItem), handPosition, direction, burstType, damage, knockback, base.Player.whoAmI, 9f, initialLength);
		if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
		{
			Main.projectile[projectileIndex].originalDamage = damage;
		}
	}

	private void UpdateGreatswordComboDashInput()
	{
		bool rightPressed = Main.mouseRight;
		bool num = rightPressed && !wasRightMousePressed;
		wasRightMousePressed = rightPressed;
		if ((!num && !IsGreatswordDevilsMode) || MoDaoQianRenUISystem.IsStageEditorOpen || !IsGreatswordMode || base.Player.noItems || base.Player.CCed || !(base.Player.HeldItem?.ModItem is MoDaoQianRen blade))
		{
			return;
		}
		blade.ApplyModeStats(base.Player);
		if (IsGreatswordDevilsMode)
		{
			if (!rightPressed)
			{
				return;
			}
			int crimsonRiftType = ModContent.ProjectileType<MoDaoQianRenCrimsonRiftSlashProjectile>();
			if (base.Player.ownedProjectileCounts[crimsonRiftType] > 0)
			{
				return;
			}
			Vector2 devilsHandPosition = base.Player.RotatedRelativePoint(base.Player.MountedCenter, reverseRotation: true);
			Vector2 devilsAim = Main.MouseWorld - devilsHandPosition;
			if (devilsAim == Vector2.Zero || devilsAim.HasNaNs())
			{
				devilsAim = Vector2.UnitX * base.Player.direction;
			}
			Vector2 devilsDirection = devilsAim.SafeNormalize(Vector2.UnitX * base.Player.direction);
			MoDaoQianRenStageStats stats = MoDaoQianRen.GetRuntimeStats(blade.GrowthStage);
			float devilsBladeLength = MathHelper.Clamp(blade.GetScaledStageBladeLength(base.Player) * stats.CrimsonRiftBladeLengthMultiplier, 120f, 1364f);
			int devilsDamage = Math.Max(1, (int)MathF.Round((float)base.Player.GetWeaponDamage(base.Player.HeldItem) * stats.CrimsonRiftDamageMultiplier));
			float devilsKnockback = base.Player.GetWeaponKnockback(base.Player.HeldItem, base.Player.HeldItem.knockBack) * MoDaoQianRen.GetGreatswordKnockbackMultiplier(blade.GrowthStage, MoDaoQianRenHeldProjectile.GreatswordHeavySlashAttackMode);
			int devilsIndex = Projectile.NewProjectile(base.Player.GetSource_ItemUse(base.Player.HeldItem), devilsHandPosition, devilsDirection, crimsonRiftType, devilsDamage, devilsKnockback, base.Player.whoAmI, devilsBladeLength, devilsDirection.ToRotation(), Main.rand.NextBool() ? 1f : -1f);
			if (devilsIndex >= 0 && devilsIndex < Main.maxProjectiles)
			{
				Main.projectile[devilsIndex].originalDamage = devilsDamage;
			}
			return;
		}
		int heldType = ModContent.ProjectileType<MoDaoQianRenHeldProjectile>();
		if (base.Player.ownedProjectileCounts[heldType] > 0)
		{
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile projectile = Main.projectile[i];
				if (projectile.active && projectile.owner == base.Player.whoAmI && projectile.type == heldType)
				{
					projectile.Kill();
				}
			}
		}
		Vector2 handPosition = base.Player.RotatedRelativePoint(base.Player.MountedCenter, reverseRotation: true);
		Vector2 aim = Main.MouseWorld - handPosition;
		if (aim == Vector2.Zero || aim.HasNaNs())
		{
			aim = Vector2.UnitX * base.Player.direction;
		}
		Vector2 bladeDirection = -aim.SafeNormalize(Vector2.UnitX * base.Player.direction);
		MoDaoQianRenStageStats dashStats = MoDaoQianRen.GetRuntimeStats(blade.GrowthStage);
		float bladeLength = MathHelper.Clamp(MathF.Max(blade.GetScaledStageBladeLength(base.Player) * dashStats.GreatswordComboDashLengthMultiplier, aim.Length()), 120f, 1364f);
		if (!TryConsumeShardCharge(dashStats.GreatswordComboDashShardCost))
		{
			SoundStyle style = SoundID.MenuTick with
			{
				Volume = 0.45f,
				Pitch = -0.28f
			};
			SoundEngine.PlaySound(in style, base.Player.Center);
			Main.NewText(Language.GetTextValue("Mods." + base.Mod.Name + ".Items.MoDaoQianRen.Mode3BurstNeedCharge"), new Color(210, 120, 255));
			return;
		}
		int damage = Math.Max(1, base.Player.GetWeaponDamage(base.Player.HeldItem));
		float knockback = base.Player.GetWeaponKnockback(base.Player.HeldItem, base.Player.HeldItem.knockBack) * MathF.Max(1f, MoDaoQianRen.GetGreatswordKnockbackMultiplier(blade.GrowthStage, 5));
		int projectileIndex = Projectile.NewProjectile(base.Player.GetSource_ItemUse(base.Player.HeldItem), handPosition, bladeDirection, heldType, damage, knockback, base.Player.whoAmI, 10f, bladeLength);
		if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
		{
			Main.projectile[projectileIndex].originalDamage = damage;
		}
	}

	private void UpdateShardOrbControl()
	{
		if (base.Player.whoAmI != Main.myPlayer || !Main.mouseMiddle || IsShardPrismMode || IsGreatswordMode || IsGuardMode || base.Player.noItems || base.Player.CCed || !MoDaoQianRen.TryGetGrowthStage(base.Player.HeldItem, out var growthStage) || growthStage < 4)
		{
			return;
		}
		int orbType = ModContent.ProjectileType<MoDaoQianRenShardOrbProjectile>();
		if (base.Player.ownedProjectileCounts[orbType] <= 0)
		{
			Vector2 handPosition = base.Player.RotatedRelativePoint(base.Player.MountedCenter, reverseRotation: true);
			Vector2 aim = Main.MouseWorld - handPosition;
			if (aim == Vector2.Zero || aim.HasNaNs())
			{
				aim = Vector2.UnitX * base.Player.direction;
			}
			Projectile.NewProjectile(base.Player.GetSource_ItemUse(base.Player.HeldItem), handPosition + aim.SafeNormalize(Vector2.UnitX * base.Player.direction) * 120f, aim.SafeNormalize(Vector2.UnitX * base.Player.direction) * 8f, orbType, base.Player.GetWeaponDamage(base.Player.HeldItem), base.Player.GetWeaponKnockback(base.Player.HeldItem, base.Player.HeldItem.knockBack), base.Player.whoAmI);
		}
	}

	private bool TryGetOwnedBladeStage(out int growthStage)
	{
		return MoDaoQianRen.TryGetOwnedGrowthStage(base.Player, out growthStage);
	}

	private int GetHeldBladeStageOrDefault()
	{
		if (MoDaoQianRen.TryGetGrowthStage(base.Player.HeldItem, out var growthStage))
		{
			return growthStage;
		}
		return 0;
	}

	private MoDaoQianRenStageStats GetBestOwnedRuntimeStats()
	{
		if (!TryGetOwnedBladeStage(out var growthStage))
		{
			growthStage = 0;
		}
		return MoDaoQianRen.GetRuntimeStats(growthStage);
	}

	private bool TryGetBestOwnedBlade(out Item bladeItem, out MoDaoQianRen blade)
	{
		bladeItem = null;
		blade = null;
		int bestStage = -1;
		for (int i = 0; i < base.Player.inventory.Length; i++)
		{
			Item item = base.Player.inventory[i];
			if (item?.ModItem is MoDaoQianRen candidate && candidate.GrowthStage > bestStage)
			{
				bestStage = candidate.GrowthStage;
				bladeItem = item;
				blade = candidate;
			}
		}
		return blade != null && MoDaoQianRen.IsGuardModeUnlocked(bestStage);
	}
}
