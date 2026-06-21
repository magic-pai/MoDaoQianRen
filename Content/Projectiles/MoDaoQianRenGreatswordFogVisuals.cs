using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace 魔刀千刃.Content.Projectiles;

internal static class MoDaoQianRenGreatswordFogVisuals
{
	private delegate void FogLayerEmitter(Texture2D texture, Rectangle source, Vector2 origin, Vector2 position, Color color, float rotation, Vector2 scale);

	private const int TextureWidth = 512;

	private const int TextureHeight = 128;

	private const int FrameCount = 8;

	private const float FogOpacityCompensation = 4f;

	public const int BundledWarmUpStepCount = 8;

	private static readonly Color DefaultOuterColor = new Color(132, 70, 225);

	private static readonly Color DefaultBodyColor = new Color(184, 116, 250);

	private static readonly Color DefaultCoreColor = new Color(238, 210, 255);

	private static Texture2D[] fogFrames;

	public static void AddDrawData(List<DrawData> drawData, Vector2 rootPosition, Vector2 direction, Vector2 normal, float bladeLength, Func<float, float> halfWidthAt, float distanceFactor, float auraPower, float timer, int seedBase)
	{
		if (drawData != null && !Main.dedServ)
		{
			EmitFogLayers(rootPosition, direction, normal, bladeLength, halfWidthAt, distanceFactor, auraPower, timer, seedBase, delegate(Texture2D texture, Rectangle source, Vector2 origin, Vector2 position, Color color, float rotation, Vector2 scale)
			{
				drawData.Add(new DrawData(texture, position, source, color, rotation, origin, scale, SpriteEffects.None));
			}, DefaultOuterColor, DefaultBodyColor, DefaultCoreColor);
		}
	}

	public static void Draw(Vector2 rootPosition, Vector2 direction, Vector2 normal, float bladeLength, Func<float, float> halfWidthAt, float distanceFactor, float auraPower, float timer, int seedBase)
	{
		Draw(rootPosition, direction, normal, bladeLength, halfWidthAt, distanceFactor, auraPower, timer, seedBase, DefaultOuterColor, DefaultBodyColor, DefaultCoreColor);
	}

	public static void Draw(Vector2 rootPosition, Vector2 direction, Vector2 normal, float bladeLength, Func<float, float> halfWidthAt, float distanceFactor, float auraPower, float timer, int seedBase, Color outerColor, Color bodyColor, Color coreColor)
	{
		if (!Main.dedServ)
		{
			EmitFogLayers(rootPosition, direction, normal, bladeLength, halfWidthAt, distanceFactor, auraPower, timer, seedBase, delegate(Texture2D texture, Rectangle source, Vector2 origin, Vector2 position, Color color, float rotation, Vector2 scale)
			{
				Main.EntitySpriteDraw(texture, position, source, color, rotation, origin, scale, SpriteEffects.None);
			}, outerColor, bodyColor, coreColor);
		}
	}

	public static void Unload()
	{
		Texture2D[] frames = fogFrames;
		fogFrames = null;
		DisposeFramesOnMainThread(frames);
	}

	private static void DisposeFramesOnMainThread(Texture2D[] frames)
	{
		if (frames != null)
		{
			Main.RunOnMainThread(delegate
			{
				DisposeFrames(frames);
			});
		}
	}

	private static void DisposeFrames(Texture2D[] frames)
	{
		for (int i = 0; i < frames.Length; i++)
		{
			frames[i]?.Dispose();
			frames[i] = null;
		}
	}

	public static bool WarmUpStep()
	{
		if (Main.dedServ)
		{
			return true;
		}
		if (MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode)
		{
			return true;
		}
		if (Main.graphics?.GraphicsDevice == null)
		{
			return false;
		}
		if (fogFrames == null)
		{
			fogFrames = new Texture2D[8];
		}
		for (int frame = 0; frame < 8; frame++)
		{
			if (fogFrames[frame] == null)
			{
				fogFrames[frame] = new Texture2D(Main.graphics.GraphicsDevice, 512, 128);
				fogFrames[frame].SetData(CreateFogPixels(frame));
				return AreFramesReady();
			}
		}
		return true;
	}

	public static void WarmUpBundledFrames()
	{
		if (!Main.dedServ)
		{
			for (int frame = 0; frame < 8; frame++)
			{
				RequestBundledFogFrame(frame);
			}
		}
	}

	public static bool WarmUpBundledFrameStep(int step)
	{
		if (Main.dedServ || step < 0 || step >= 8)
		{
			return true;
		}
		RequestBundledFogFrame(step);
		return step >= 7;
	}

	private static void EmitFogLayers(Vector2 rootPosition, Vector2 direction, Vector2 normal, float bladeLength, Func<float, float> halfWidthAt, float distanceFactor, float auraPower, float timer, int seedBase, FogLayerEmitter emit, Color outerColor, Color bodyColor, Color coreColor)
	{
		auraPower = MathHelper.Clamp(auraPower, 0f, 1.35f);
		if (auraPower <= 0.012f || bladeLength <= 8f || halfWidthAt == null)
		{
			return;
		}
		direction = direction.SafeNormalize(Vector2.UnitX);
		normal = normal.SafeNormalize(direction.RotatedBy(1.5707963705062866));
		float rotation = direction.ToRotation();
		float rootFill = MathHelper.Lerp(18f, 34f, distanceFactor);
		rootPosition -= direction * rootFill;
		bladeLength += rootFill;
		float fogWidth = MathHelper.Clamp(GetMaxHalfWidth(halfWidthAt) * MathHelper.Lerp(2.24f, 2.78f, distanceFactor), 24f, 270f);
		float num = timer / 7f + (float)seedBase * 0.013f;
		int frameA = PositiveModulo((int)MathF.Floor(num), 8);
		int frameB = (frameA + 1) % 8;
		float frameBlend = SmoothStep(num - MathF.Floor(num));
		Rectangle source = new Rectangle(0, 0, 512, 128);
		Vector2 origin = new Vector2(0f, 64f);
		Vector2 baseScale = new Vector2(bladeLength / 512f, fogWidth / 128f);
		bool useBundledFrames = MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode;
		if (!useBundledFrames)
		{
			EnsureFogFrames();
			if (fogFrames == null)
			{
				return;
			}
		}
		Texture2D frameATexture = (useBundledFrames ? RequestBundledFogFrame(frameA) : fogFrames[frameA]);
		Texture2D frameBTexture = (useBundledFrames ? RequestBundledFogFrame(frameB) : fogFrames[frameB]);
		if (frameATexture != null && frameBTexture != null)
		{
			DrawFrameLayers(emit, frameATexture, source, origin, rootPosition, rotation, baseScale, distanceFactor, auraPower * (1f - frameBlend), outerColor, bodyColor, coreColor);
			DrawFrameLayers(emit, frameBTexture, source, origin, rootPosition, rotation, baseScale, distanceFactor, auraPower * frameBlend, outerColor, bodyColor, coreColor);
		}
	}

	private static void DrawFrameLayers(FogLayerEmitter emit, Texture2D texture, Rectangle source, Vector2 origin, Vector2 rootPosition, float rotation, Vector2 baseScale, float distanceFactor, float alphaScale, Color outerColor, Color bodyColor, Color coreColor)
	{
		if (texture != null && !(alphaScale <= 0.004f))
		{
			float power = MathHelper.Clamp(alphaScale * 4f, 0f, 5.4f);
			emit(texture, source, origin, rootPosition, outerColor * (0.08f * power), rotation, baseScale * new Vector2(1f, 1.28f));
			emit(texture, source, origin, rootPosition, bodyColor * (0.115f * power), rotation, baseScale * new Vector2(1f, 0.92f));
			emit(texture, source, origin, rootPosition, coreColor * (0.04f * power * MathHelper.Lerp(0.72f, 1f, distanceFactor)), rotation, baseScale * new Vector2(1f, 0.54f));
		}
	}

	private static Texture2D RequestBundledFogFrame(int frame)
	{
		return ModContent.Request<Texture2D>($"{ModContent.GetInstance<MoDaoQianRenMod>().Name}/Content/Projectiles/Generated/MoDaoQianRenGreatswordFog_{frame}").Value;
	}

	private static float GetMaxHalfWidth(Func<float, float> halfWidthAt)
	{
		float maxHalfWidth = 1f;
		for (int i = 0; i < 14; i++)
		{
			float progress = MathHelper.Lerp(0.04f, 0.98f, (float)i / 13f);
			maxHalfWidth = MathF.Max(maxHalfWidth, halfWidthAt(progress));
		}
		return MathHelper.Clamp(maxHalfWidth, 6f, 180f);
	}

	private static void EnsureFogFrames()
	{
		while (!WarmUpStep() && Main.graphics?.GraphicsDevice != null)
		{
		}
	}

	private static bool AreFramesReady()
	{
		if (fogFrames == null)
		{
			return false;
		}
		for (int frame = 0; frame < 8; frame++)
		{
			if (fogFrames[frame] == null)
			{
				return false;
			}
		}
		return true;
	}

	private static Color[] CreateFogPixels(int frame)
	{
		Color[] pixels = new Color[65536];
		float phase = (float)frame / 8f;
		for (int y = 0; y < 128; y++)
		{
			float v = (((float)y + 0.5f) / 128f - 0.5f) * 2f;
			for (int x = 0; x < 512; x++)
			{
				float num = ((float)x + 0.5f) / 512f;
				float width = GetTextureWidthProfile(num);
				float signedEdgeDistance = MathF.Abs(v) / MathF.Max(width, 0.001f);
				float edgeFade = 1f - SmoothStepRange(0.72f, 1.04f, signedEdgeDistance);
				float centerFade = MathF.Pow(MathHelper.Clamp(1f - signedEdgeDistance * 0.82f, 0f, 1f), 1.25f);
				float lengthFade = GetTextureLengthProfile(num);
				float num2 = num * 7.2f + phase * 1.8f;
				float vertical = v * 2.65f;
				float broadNoise = Fbm(num2, vertical + phase * 3.1f, 4, 101 + frame * 29);
				float fineNoise = Fbm(num2 * 4.8f - phase * 2.4f, vertical * 4.4f + phase * 5.7f, 3, 503 + frame * 31);
				float filament = 1f - SmoothStepRange(0.18f, 0.84f, MathF.Abs(fineNoise - 0.5f) * 2f);
				byte alpha = (byte)MathHelper.Clamp(MathF.Pow(MathHelper.Clamp(lengthFade * edgeFade * centerFade * MathHelper.Lerp(0.5f, 1.08f, broadNoise) * MathHelper.Lerp(0.5f, 1f, filament), 0f, 1f), 1.18f) * 144f, 0f, 144f);
				byte premultiplied = alpha;
				pixels[y * 512 + x] = new Color(premultiplied, premultiplied, premultiplied, alpha);
			}
		}
		return pixels;
	}

	private static float GetTextureWidthProfile(float progress)
	{
		float num = MathHelper.Lerp(0.56f, 1f, SmoothStepRange(0f, 0.18f, progress));
		float tipFade = MathHelper.Lerp(1f, 0.46f, SmoothStepRange(0.78f, 1f, progress));
		float chip = (ValueNoise(progress * 17.5f, 3.1f, 1901) - 0.5f) * 0.08f;
		return MathHelper.Clamp(num * tipFade + chip, 0.26f, 1.08f);
	}

	private static float GetTextureLengthProfile(float progress)
	{
		float num = MathHelper.Lerp(0.62f, 1f, SmoothStepRange(0f, 0.08f, progress));
		float tipFade = MathHelper.Lerp(1f, 0.2f, SmoothStepRange(0.82f, 1f, progress));
		return MathHelper.Clamp(num * tipFade, 0f, 1f);
	}

	private static float Fbm(float x, float y, int octaves, int seed)
	{
		float sum = 0f;
		float amplitude = 0.5f;
		float frequency = 1f;
		float norm = 0f;
		for (int i = 0; i < octaves; i++)
		{
			sum += ValueNoise(x * frequency, y * frequency, seed + i * 37) * amplitude;
			norm += amplitude;
			frequency *= 2.03f;
			amplitude *= 0.52f;
		}
		if (!(norm <= 0f))
		{
			return sum / norm;
		}
		return 0f;
	}

	private static float ValueNoise(float x, float y, int seed)
	{
		int xi = (int)MathF.Floor(x);
		int yi = (int)MathF.Floor(y);
		float tx = SmoothStep(x - (float)xi);
		float ty = SmoothStep(y - (float)yi);
		float value = Hash(xi, yi, seed);
		float b = Hash(xi + 1, yi, seed);
		float c = Hash(xi, yi + 1, seed);
		return MathHelper.Lerp(value2: MathHelper.Lerp(c, Hash(xi + 1, yi + 1, seed), tx), value1: MathHelper.Lerp(value, b, tx), amount: ty);
	}

	private static float Hash(int x, int y, int seed)
	{
		return MoDaoQianRenShardVisuals.Random01((x * 73856093) ^ (y * 19349663) ^ (seed * 83492791));
	}

	private static int PositiveModulo(int value, int divisor)
	{
		int result = value % divisor;
		if (result >= 0)
		{
			return result;
		}
		return result + divisor;
	}

	private static float SmoothStep(float value)
	{
		value = MathHelper.Clamp(value, 0f, 1f);
		return value * value * (3f - 2f * value);
	}

	private static float SmoothStepRange(float edge0, float edge1, float value)
	{
		return SmoothStep(Utils.GetLerpValue(edge0, edge1, value, clamped: true));
	}
}
