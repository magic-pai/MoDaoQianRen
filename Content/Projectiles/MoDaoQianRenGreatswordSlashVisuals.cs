using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace 魔刀千刃.Content.Projectiles;

internal static class MoDaoQianRenGreatswordSlashVisuals
{
	private const int TextureSize = 384;

	private const int FrameCount = 6;

	private const float ArcHalfAngle = 1.24f;

	private const float OuterArcNormalizedRadius = 0.81f;

	public const float OuterArcAngle = 2.48f;

	public const int BundledWarmUpStepCount = 12;

	private static Texture2D[] bodyFrames;

	private static Texture2D[] edgeFrames;

	public static void Draw(Vector2 centerPosition, float rotation, float outerArcRadius, float distanceFactor, float power, float timer, bool heavy, bool reverse, bool foreground)
	{
		if (Main.dedServ || power <= 0.01f || outerArcRadius <= 16f)
		{
			return;
		}
		float num = timer / (foreground ? 4.2f : 6.5f) + (heavy ? 0.37f : 0.11f);
		int frameA = PositiveModulo((int)MathF.Floor(num), 6);
		int frameB = (frameA + 1) % 6;
		float blend = SmoothStep(num - MathF.Floor(num));
		Texture2D frameATexture;
		Texture2D frameBTexture;
		if (MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode)
		{
			frameATexture = RequestBundledSlashFrame(frameA, foreground);
			frameBTexture = RequestBundledSlashFrame(frameB, foreground);
		}
		else
		{
			EnsureFrames();
			Texture2D[] frames = (foreground ? edgeFrames : bodyFrames);
			if (frames == null)
			{
				return;
			}
			frameATexture = frames[frameA];
			frameBTexture = frames[frameB];
		}
		if (frameATexture != null && frameBTexture != null)
		{
			DrawFrame(frameATexture, centerPosition, rotation, outerArcRadius, distanceFactor, power * (1f - blend), heavy, reverse, foreground);
			DrawFrame(frameBTexture, centerPosition, rotation, outerArcRadius, distanceFactor, power * blend, heavy, reverse, foreground);
		}
	}

	public static void Unload()
	{
		Texture2D[] bodyFramesToDispose = bodyFrames;
		Texture2D[] frames = edgeFrames;
		bodyFrames = null;
		edgeFrames = null;
		DisposeFramesOnMainThread(bodyFramesToDispose);
		DisposeFramesOnMainThread(frames);
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
		if (bodyFrames == null)
		{
			bodyFrames = new Texture2D[6];
		}
		if (edgeFrames == null)
		{
			edgeFrames = new Texture2D[6];
		}
		for (int frame = 0; frame < 6; frame++)
		{
			if (bodyFrames[frame] == null)
			{
				bodyFrames[frame] = new Texture2D(Main.graphics.GraphicsDevice, 384, 384);
				bodyFrames[frame].SetData(CreatePixels(frame, edge: false));
				return AreFramesReady();
			}
		}
		for (int i = 0; i < 6; i++)
		{
			if (edgeFrames[i] == null)
			{
				edgeFrames[i] = new Texture2D(Main.graphics.GraphicsDevice, 384, 384);
				edgeFrames[i].SetData(CreatePixels(i, edge: true));
				return AreFramesReady();
			}
		}
		return true;
	}

	public static void WarmUpBundledFrames()
	{
		if (!Main.dedServ)
		{
			for (int frame = 0; frame < 6; frame++)
			{
				RequestBundledSlashFrame(frame, edge: false);
				RequestBundledSlashFrame(frame, edge: true);
			}
		}
	}

	public static bool WarmUpBundledFrameStep(int step)
	{
		if (Main.dedServ || step < 0 || step >= 12)
		{
			return true;
		}
		int frame = step / 2;
		bool edge = step % 2 == 1;
		RequestBundledSlashFrame(frame, edge);
		return step >= 11;
	}

	private static void DrawFrame(Texture2D texture, Vector2 centerPosition, float rotation, float outerArcRadius, float distanceFactor, float power, bool heavy, bool reverse, bool foreground)
	{
		if (texture != null && !(power <= 0.004f))
		{
			Vector2 origin = new Vector2(192f);
			float scale = outerArcRadius / 155.52f;
			SpriteEffects effects = (reverse ? SpriteEffects.FlipVertically : SpriteEffects.None);
			if (foreground)
			{
				Vector2 sharpScale = Vector2.One * scale;
				Color edgeColor = (heavy ? new Color(255, 154, 250) : new Color(238, 142, 255));
				Color coreColor = new Color(255, 236, 255);
				Main.EntitySpriteDraw(texture, centerPosition, null, edgeColor * (0.62f * power), rotation, origin, sharpScale, effects);
				Main.EntitySpriteDraw(texture, centerPosition, null, coreColor * (0.28f * power), rotation, origin, sharpScale * new Vector2(0.99f, 0.96f), effects);
			}
			else
			{
				Vector2 broadScale = Vector2.One * scale * (heavy ? 1.08f : 1.04f);
				Vector2 bodyScale = Vector2.One * scale;
				Color shadowColor = (heavy ? new Color(52, 0, 96) : new Color(44, 0, 88));
				Color bodyColor = (heavy ? new Color(172, 42, 255) : new Color(144, 54, 246));
				Color hotColor = (heavy ? new Color(236, 112, 255) : new Color(210, 106, 255));
				Main.EntitySpriteDraw(texture, centerPosition, null, shadowColor * (0.38f * power), rotation, origin, broadScale, effects);
				Main.EntitySpriteDraw(texture, centerPosition, null, bodyColor * (0.48f * power), rotation, origin, bodyScale, effects);
				Main.EntitySpriteDraw(texture, centerPosition, null, hotColor * (0.18f * power), rotation, origin, bodyScale * new Vector2(0.98f, 0.9f), effects);
			}
		}
	}

	private static Texture2D RequestBundledSlashFrame(int frame, bool edge)
	{
		string kind = (edge ? "Edge" : "Body");
		return ModContent.Request<Texture2D>($"{ModContent.GetInstance<MoDaoQianRenMod>().Name}/Content/Projectiles/Generated/MoDaoQianRenGreatswordSlash{kind}_{frame}").Value;
	}

	private static void EnsureFrames()
	{
		while (!WarmUpStep() && Main.graphics?.GraphicsDevice != null)
		{
		}
	}

	private static bool AreFramesReady()
	{
		if (bodyFrames == null || edgeFrames == null)
		{
			return false;
		}
		for (int frame = 0; frame < 6; frame++)
		{
			if (bodyFrames[frame] == null || edgeFrames[frame] == null)
			{
				return false;
			}
		}
		return true;
	}

	private static Color[] CreatePixels(int frame, bool edge)
	{
		Color[] pixels = new Color[147456];
		float phase = (float)frame / 6f;
		for (int y = 0; y < 384; y++)
		{
			float py = (((float)y + 0.5f) / 384f - 0.5f) * 2f;
			for (int x = 0; x < 384; x++)
			{
				float px = (((float)x + 0.5f) / 384f - 0.5f) * 2f;
				float radius = MathF.Sqrt(px * px + py * py);
				float angle = MathF.Atan2(py, px);
				float angleT = MathF.Abs(angle) / 1.24f;
				if (angleT >= 1.08f || radius <= 0.04f || radius >= 1.08f)
				{
					continue;
				}
				float angularFade = 1f - SmoothStepRange(0.86f, 1.04f, angleT);
				float endFade = SmoothStepRange(0.02f, 0.12f, 1.04f - angleT);
				float crescentBias = SmoothStep(1f - angleT);
				float centerRadius = MathHelper.Lerp(0.74f, 0.64f, crescentBias);
				float halfWidth = MathHelper.Lerp(0.07f, 0.16f, crescentBias);
				float innerEdge = centerRadius - halfWidth * MathHelper.Lerp(0.86f, 1.16f, crescentBias);
				float outerEdge = centerRadius + halfWidth;
				float radialMask = SmoothStepRange(innerEdge - 0.05f, innerEdge + 0.025f, radius) * (1f - SmoothStepRange(outerEdge - 0.025f, outerEdge + 0.07f, radius));
				if (!(radialMask <= 0f) && !(angularFade <= 0f))
				{
					float x2 = angle * 2.15f + phase * 1.7f;
					float flowY = radius * 8.5f - phase * 2.6f;
					float broadNoise = Fbm(x2, flowY, 4, 331 + frame * 41);
					float fineNoise = Fbm(angle * 8.4f - phase * 2.2f, radius * 25f + phase * 4.8f, 3, 971 + frame * 53);
					float torn = MathHelper.Lerp(0.52f, 1.14f, SmoothStepRange(0.2f, 0.88f, broadNoise + fineNoise * 0.26f));
					if (edge)
					{
						float num = 1f - SmoothStepRange(0.004f, 0.03f, MathF.Abs(radius - outerEdge));
						float capLine = (1f - SmoothStepRange(0.015f, 0.08f, MathF.Abs(angleT - 0.96f))) * radialMask;
						float crack = (1f - SmoothStepRange(0.035f, 0.13f, MathF.Abs(fineNoise - 0.5f))) * radialMask;
						byte alpha = (byte)MathHelper.Clamp(MathF.Pow(MathHelper.Clamp(MathHelper.Clamp(num * 0.92f + capLine * 0.55f + crack * 0.18f, 0f, 1f) * (angularFade * endFade * MathHelper.Lerp(0.7f, 1.12f, broadNoise)), 0f, 1f), 1.08f) * 230f, 0f, 230f);
						pixels[y * 384 + x] = new Color(alpha, alpha, alpha, alpha);
					}
					else
					{
						float outerHeat = SmoothStepRange(centerRadius - halfWidth * 0.25f, outerEdge + 0.02f, radius);
						byte bodyAlpha = (byte)MathHelper.Clamp(MathF.Pow(MathHelper.Clamp(radialMask * angularFade * endFade * MathHelper.Lerp(0.68f, 1.1f, outerHeat) * torn * MathHelper.Lerp(0.82f, 1.08f, fineNoise), 0f, 1f), 1.14f) * 205f, 0f, 205f);
						pixels[y * 384 + x] = new Color(bodyAlpha, bodyAlpha, bodyAlpha, bodyAlpha);
					}
				}
			}
		}
		return pixels;
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
			frequency *= 2.04f;
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
