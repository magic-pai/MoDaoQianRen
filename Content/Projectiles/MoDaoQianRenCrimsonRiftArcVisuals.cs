using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace 魔刀千刃.Content.Projectiles;

internal static class MoDaoQianRenCrimsonRiftArcVisuals
{
	private const int TextureSize = 384;

	private const int FrameCount = 8;

	private const float ArcHalfAngle = 2.2f;

	private const float OuterArcNormalizedRadius = 0.84f;

	private static Texture2D[] fillFrames;

	private static Texture2D[] edgeFrames;

	public static void Draw(Vector2 centerPosition, float rotation, float outerArcRadius, float power, float slashProgress, bool reverse, bool foreground)
	{
		if (Main.dedServ || power <= 0.012f || outerArcRadius <= 16f)
		{
			return;
		}
		EnsureFrames();
		Texture2D[] frames = foreground ? edgeFrames : fillFrames;
		if (frames == null)
		{
			return;
		}
		float frameValue = MathHelper.Clamp(slashProgress, 0f, 1f) * (FrameCount - 1);
		int frameA = PositiveModulo((int)MathF.Floor(frameValue), FrameCount);
		int frameB = Math.Min(frameA + 1, FrameCount - 1);
		float blend = SmoothStep(frameValue - MathF.Floor(frameValue));
		DrawFrame(frames[frameA], centerPosition, rotation, outerArcRadius, power * (1f - blend), reverse, foreground);
		DrawFrame(frames[frameB], centerPosition, rotation, outerArcRadius, power * blend, reverse, foreground);
	}

	public static void Unload()
	{
		Texture2D[] fills = fillFrames;
		Texture2D[] edges = edgeFrames;
		fillFrames = null;
		edgeFrames = null;
		DisposeFramesOnMainThread(fills);
		DisposeFramesOnMainThread(edges);
	}

	public static bool WarmUpStep()
	{
		if (Main.dedServ)
		{
			return true;
		}
		if (Main.graphics?.GraphicsDevice == null)
		{
			return false;
		}
		if (fillFrames == null)
		{
			fillFrames = new Texture2D[FrameCount];
		}
		if (edgeFrames == null)
		{
			edgeFrames = new Texture2D[FrameCount];
		}
		for (int frame = 0; frame < FrameCount; frame++)
		{
			if (fillFrames[frame] == null)
			{
				fillFrames[frame] = new Texture2D(Main.graphics.GraphicsDevice, TextureSize, TextureSize);
				fillFrames[frame].SetData(CreatePixels(frame, edge: false));
				return AreFramesReady();
			}
			if (edgeFrames[frame] == null)
			{
				edgeFrames[frame] = new Texture2D(Main.graphics.GraphicsDevice, TextureSize, TextureSize);
				edgeFrames[frame].SetData(CreatePixels(frame, edge: true));
				return AreFramesReady();
			}
		}
		return true;
	}

	private static void EnsureFrames()
	{
		if (Main.graphics?.GraphicsDevice == null)
		{
			return;
		}
		if (fillFrames == null)
		{
			fillFrames = new Texture2D[FrameCount];
		}
		if (edgeFrames == null)
		{
			edgeFrames = new Texture2D[FrameCount];
		}
		for (int frame = 0; frame < FrameCount; frame++)
		{
			if (fillFrames[frame] == null)
			{
				fillFrames[frame] = new Texture2D(Main.graphics.GraphicsDevice, TextureSize, TextureSize);
				fillFrames[frame].SetData(CreatePixels(frame, edge: false));
			}
			if (edgeFrames[frame] == null)
			{
				edgeFrames[frame] = new Texture2D(Main.graphics.GraphicsDevice, TextureSize, TextureSize);
				edgeFrames[frame].SetData(CreatePixels(frame, edge: true));
			}
		}
	}

	private static bool AreFramesReady()
	{
		if (fillFrames == null || edgeFrames == null)
		{
			return false;
		}
		for (int frame = 0; frame < FrameCount; frame++)
		{
			if (fillFrames[frame] == null || edgeFrames[frame] == null)
			{
				return false;
			}
		}
		return true;
	}

	private static void DrawFrame(Texture2D texture, Vector2 centerPosition, float rotation, float outerArcRadius, float power, bool reverse, bool foreground)
	{
		if (texture == null || power <= 0.004f)
		{
			return;
		}
		Vector2 origin = new Vector2(TextureSize * 0.5f);
		float scale = outerArcRadius / (TextureSize * 0.5f * OuterArcNormalizedRadius);
		SpriteEffects effects = reverse ? SpriteEffects.FlipVertically : SpriteEffects.None;
		if (foreground)
		{
			Color edge = new Color(255, 34, 62) * (0.42f * power);
			Color core = new Color(255, 232, 236) * (0.22f * power);
			Main.EntitySpriteDraw(texture, centerPosition, null, edge, rotation, origin, Vector2.One * scale, effects);
			Main.EntitySpriteDraw(texture, centerPosition, null, core, rotation, origin, Vector2.One * scale * 0.98f, effects);
			return;
		}
		Color shadow = new Color(34, 0, 6) * (0.18f * power);
		Color body = new Color(176, 9, 28) * (0.24f * power);
		Color heat = new Color(255, 48, 72) * (0.11f * power);
		Main.EntitySpriteDraw(texture, centerPosition, null, shadow, rotation, origin, Vector2.One * scale * 1.01f, effects);
		Main.EntitySpriteDraw(texture, centerPosition, null, body, rotation, origin, Vector2.One * scale, effects);
		Main.EntitySpriteDraw(texture, centerPosition, null, heat, rotation, origin, Vector2.One * scale * 0.99f, effects);
	}

	private static Color[] CreatePixels(int frame, bool edge)
	{
		Color[] pixels = new Color[TextureSize * TextureSize];
		float phase = FrameCount <= 1 ? 1f : (float)frame / (FrameCount - 1);
		for (int y = 0; y < TextureSize; y++)
		{
			float py = (((float)y + 0.5f) / TextureSize - 0.5f) * 2f;
			for (int x = 0; x < TextureSize; x++)
			{
				float px = (((float)x + 0.5f) / TextureSize - 0.5f) * 2f;
				float radius = MathF.Sqrt(px * px + py * py);
				float angle = MathF.Atan2(py, px);
				float angleT = MathF.Abs(angle) / ArcHalfAngle;
				if (angleT >= 1.012f || radius <= 0.32f || radius >= 0.87f)
				{
					continue;
				}
				float sweepT = MathHelper.Clamp(1f - (angle / ArcHalfAngle + 1f) * 0.5f, 0f, 1f);
				float angularFade = 1f - SmoothStepRange(0.93f, 1.008f, angleT);
				float startFade = SmoothStepRange(0.018f, 0.105f, sweepT);
				float endFade = 1f - SmoothStepRange(0.965f, 1.012f, sweepT);
				float directionFade = MathHelper.Lerp(0.08f, 1.18f, SmoothStepRange(0.08f, 0.94f, sweepT));
				float activeFront = MathHelper.Lerp(0.1f, 0.96f, SmoothStep(phase));
				float revealFade = 1f - SmoothStepRange(activeFront + 0.02f, activeFront + 0.19f, sweepT);
				float frontGlow = 1f - SmoothStepRange(0.015f, 0.16f, MathF.Abs(sweepT - activeFront));
				float processGlow = MathHelper.Lerp(0.76f, 1.28f, frontGlow) * MathHelper.Lerp(0.78f, 1.08f, SmoothStep(phase));
				float capT = SmoothStep(angleT);
				float innerEdge = MathHelper.Lerp(OuterArcNormalizedRadius * 0.5f, OuterArcNormalizedRadius * 0.56f, capT);
				float outerEdge = MathHelper.Lerp(OuterArcNormalizedRadius * 0.92f, OuterArcNormalizedRadius * 0.975f, capT);
				float band = SmoothStepRange(innerEdge - 0.028f, innerEdge + 0.045f, radius) * (1f - SmoothStepRange(outerEdge - 0.045f, outerEdge + 0.052f, radius));
				if (band <= 0f)
				{
					continue;
				}
				float flowX = angle * 2.2f + phase * 2.7f;
				float flowY = radius * 8.4f - phase * 3.8f;
				float broad = Fbm(flowX, flowY, 4, 1201 + frame * 43);
				float fine = Fbm(angle * 8.2f - phase * 3.1f, radius * 26f + phase * 5.4f, 3, 2603 + frame * 59);
				float scratch = Fbm(angle * 15.5f + radius * 4.2f - phase * 6.1f, radius * 40f + phase * 7.8f, 2, 4301 + frame * 67);
				float torn = MathHelper.Lerp(0.34f, 1.22f, SmoothStepRange(0.28f, 0.9f, broad + fine * 0.22f));
				float fissures = MathHelper.Lerp(0.42f, 1.1f, SmoothStepRange(0.32f, 0.82f, scratch));
				float fade = angularFade * startFade * endFade * directionFade * revealFade * processGlow;
				float radialT = MathHelper.Clamp((radius - innerEdge) / MathF.Max(0.001f, outerEdge - innerEdge), 0f, 1f);
				float outerLayer = SmoothStepRange(0.48f, 0.6f, radialT);
				float hotRidge = (1f - SmoothStepRange(0.018f, 0.07f, MathF.Abs(radialT - MathHelper.Lerp(0.6f, 0.78f, broad)))) * band;
				float trailingCuts = (1f - SmoothStepRange(0.018f, 0.065f, MathF.Abs(fine - 0.5f))) * band * MathHelper.Lerp(0.35f, 1f, sweepT);
				if (edge)
				{
					float outerLine = 1f - SmoothStepRange(0.01f, 0.05f, MathF.Abs(radius - outerEdge));
					float innerLine = 1f - SmoothStepRange(0.012f, 0.06f, MathF.Abs(radius - innerEdge));
					byte alpha = (byte)MathHelper.Clamp(MathF.Pow(MathHelper.Clamp((outerLine * 0.54f + innerLine * 0.5f + hotRidge * 0.42f + trailingCuts * 0.2f) * fade * MathHelper.Lerp(0.74f, 1.12f, broad), 0f, 1f), 0.96f) * 226f, 0f, 226f);
					pixels[y * TextureSize + x] = new Color(alpha, alpha, alpha, alpha);
				}
				else
				{
					float layerBalance = MathHelper.Lerp(0.72f, 1.12f, outerLayer);
					float body = MathHelper.Clamp(band * fade * torn * fissures * layerBalance + hotRidge * fade * 0.28f, 0f, 1f);
					byte alpha = (byte)MathHelper.Clamp(MathF.Pow(body, 1.05f) * 166f, 0f, 166f);
					pixels[y * TextureSize + x] = new Color(alpha, alpha, alpha, alpha);
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
				for (int i = 0; i < frames.Length; i++)
				{
					frames[i]?.Dispose();
					frames[i] = null;
				}
			});
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
		return norm <= 0f ? 0f : sum / norm;
	}

	private static float ValueNoise(float x, float y, int seed)
	{
		int xi = (int)MathF.Floor(x);
		int yi = (int)MathF.Floor(y);
		float tx = SmoothStep(x - xi);
		float ty = SmoothStep(y - yi);
		float a = Hash(xi, yi, seed);
		float b = Hash(xi + 1, yi, seed);
		float c = Hash(xi, yi + 1, seed);
		float d = Hash(xi + 1, yi + 1, seed);
		return MathHelper.Lerp(MathHelper.Lerp(a, b, tx), MathHelper.Lerp(c, d, tx), ty);
	}

	private static float Hash(int x, int y, int seed)
	{
		return MoDaoQianRenShardVisuals.Random01((x * 73856093) ^ (y * 19349663) ^ (seed * 83492791));
	}

	private static int PositiveModulo(int value, int divisor)
	{
		int result = value % divisor;
		return result >= 0 ? result : result + divisor;
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
