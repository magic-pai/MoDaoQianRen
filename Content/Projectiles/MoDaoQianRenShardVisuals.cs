using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace 魔刀千刃.Content.Projectiles;

internal static class MoDaoQianRenShardVisuals
{
	private const int FrameWidth = 32;

	private const int FrameHeight = 24;

	private const int FrameCount = 8;

	public static Texture2D Texture => ModContent.Request<Texture2D>(MoDaoQianRenMod.ShardParticleTexture).Value;

	public static void WarmUp()
	{
		if (!Main.dedServ)
		{
			_ = Texture;
		}
	}

	public static Rectangle GetFrame(int seed)
	{
		return new Rectangle(Math.Abs(seed) % 8 * 32, 0, 32, 24);
	}

	public static float Random01(int seed)
	{
		float num = MathF.Sin((float)seed * 12.9898f + 78.233f) * 43758.547f;
		return num - MathF.Floor(num);
	}

	public static float Flicker(float timer, int seed, float minimumRate = 0.055f, float maximumRate = 0.42f)
	{
		float phaseA = Random01(seed * 17 + 3) * ((float)Math.PI * 2f);
		float phaseB = Random01(seed * 29 + 11) * ((float)Math.PI * 2f);
		float rateA = MathHelper.Lerp(minimumRate, maximumRate, Random01(seed * 37 + 19));
		float rateB = MathHelper.Lerp(maximumRate * 0.45f, maximumRate * 1.35f, Random01(seed * 43 + 23));
		float wave = 0.5f + MathF.Sin(timer * rateA + phaseA) * 0.5f;
		float spark = 0.5f + MathF.Sin(timer * rateB + phaseB) * 0.5f;
		return MathHelper.Clamp(0.48f + wave * 0.34f + MathF.Pow(spark, 5f) * 0.36f, 0f, 1.18f);
	}

	public static void DrawOutlinedShard(Texture2D texture, Vector2 drawPosition, int seed, Color outlineColor, Color coreColor, Color flashColor, float rotation, float scale, float flicker, SpriteEffects effects = SpriteEffects.None)
	{
		Rectangle source = GetFrame(seed);
		Vector2 origin = source.Size() * 0.5f;
		float outlineAlpha = MathHelper.Clamp(0.28f + flicker * 0.36f, 0f, 0.76f);
		float outlineOffset = MathHelper.Lerp(0.85f, 1.65f, MathHelper.Clamp(scale * 4.5f, 0f, 1f));
		Main.EntitySpriteDraw(texture, drawPosition, source, outlineColor * (0.12f + flicker * 0.16f), rotation, origin, scale * (1.42f + flicker * 0.16f), effects);
		for (int i = 0; i < 6; i++)
		{
			Vector2 offset = ((float)Math.PI * 2f * (float)i / 6f + Random01(seed * 53 + i) * 0.25f).ToRotationVector2() * outlineOffset;
			Main.EntitySpriteDraw(texture, drawPosition + offset, source, outlineColor * outlineAlpha, rotation, origin, scale * 1.04f, effects);
		}
		Main.EntitySpriteDraw(texture, drawPosition, source, coreColor, rotation, origin, scale, effects);
		Main.EntitySpriteDraw(texture, drawPosition, source, flashColor * MathHelper.Clamp(0.12f + flicker * 0.32f, 0f, 0.52f), rotation, origin, scale * 0.68f, effects);
	}
}
