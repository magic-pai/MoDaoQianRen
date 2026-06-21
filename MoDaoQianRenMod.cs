using Terraria.ModLoader;
using 魔刀千刃.Content.Projectiles;

namespace 魔刀千刃;

public class MoDaoQianRenMod : Mod
{
	public const string TransparentPlaceholderTexture = "Terraria/Images/Item_0";

	public const string WeaponIconTexture = "魔刀千刃/Content/Items/Weapons/MoDaoQianRen_preview_4x";

	public const string WeaponOutTexture = "魔刀千刃/Content/Items/Weapons/MoDaoQianRen_weaponout";

	public const string WeaponOutPulseTexture = "魔刀千刃/Content/Items/Weapons/MoDaoQianRen_weaponout_pulse";

	public static string WeaponOutAnchorTexture => ModContent.GetInstance<MoDaoQianRenMod>().Name + "/Content/Items/Weapons/MoDaoQianRen_weaponout_anchor";

	public static bool UseLightsAndShadowsCompatibilityMode
	{
		get
		{
			Mod result;
			return ModLoader.TryGetMod("Lights", out result);
		}
	}

	public static string InventoryIconTexture => ModContent.GetInstance<MoDaoQianRenMod>().Name + "/icon";

	public static string InventoryIconPulseTexture => ModContent.GetInstance<MoDaoQianRenMod>().Name + "/icon_pulse_spritesheet";

	public static string WeaponHiltTexture => ModContent.GetInstance<MoDaoQianRenMod>().Name + "/Content/Items/Weapons/MoDaoQianRen_hilt_4x";

	public static string ShearsTexture => ModContent.GetInstance<MoDaoQianRenMod>().Name + "/Content/Items/Weapons/QianRenShears";

	public static string ShardParticleTexture => ModContent.GetInstance<MoDaoQianRenMod>().Name + "/Content/Projectiles/MoDaoQianRenShardParticle";

	public static string GreatswordBladeTexture => ModContent.GetInstance<MoDaoQianRenMod>().Name + "/Content/Projectiles/MoDaoQianRenGreatswordBlade";

	public override void Unload()
	{
		MoDaoQianRenGreatswordFogVisuals.Unload();
		MoDaoQianRenGreatswordSlashVisuals.Unload();
		MoDaoQianRenCrimsonRiftArcVisuals.Unload();
	}
}
