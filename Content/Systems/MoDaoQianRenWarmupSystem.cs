using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using 魔刀千刃.Content.Projectiles;

namespace 魔刀千刃.Content.Systems;

public class MoDaoQianRenWarmupSystem : ModSystem
{
	private const int LightsStableDrawFramesBeforeWarmup = 120;

	private const int LightsStableUpdateFramesBeforeWarmup = 120;

	private const int LightsWarmupStepSpacingFrames = 2;

	private const long LightsMinimumStartupSuppressionMilliseconds = 2500L;

	private const int RequestedAssetWarmupStepCount = 5;

	private const string LightsModName = "Lights";

	private const string LightsTypeName = "Lights.Lights";

	private const string LightsUseLightFieldName = "useLight";

	private const string LightsUseBloomFieldName = "useBloom";

	private const string LightsEndCaptureBridgeMethodName = "FilterManager_EndCapture";

	private const string LightsInitTargetsBridgeMethodName = "On_Main_InitTargets_int_int";

	private static bool warmupQueued;

	private static int warmupStep;

	private static int warmupDelay;

	private static bool lightsStartupWarmupActive;

	private static bool lightsStartupResourcesReady = true;

	private static int lightsStableDrawFrames;

	private static int lightsStableUpdateFrames;

	private static int lightsRequestedAssetWarmupStep;

	private static int lightsBundledWarmupStep;

	private static long lightsStartupSuppressionStartedAtMs;

	private static FieldInfo lightsUseLightField;

	private static FieldInfo lightsUseBloomField;

	private static bool lightsOriginalUseLight;

	private static bool lightsOriginalUseBloom;

	private static bool lightsOriginalPostProcessingCaptured;

	private static bool lightsPostProcessingSuppressed;

	private static readonly List<IDisposable> lightsBridgeBypassHooks = new List<IDisposable>();

	private static bool lightsBridgeBypassHooksInstalled;

	private static bool lightsBridgeBypassHookInstallFailed;

	private static bool lightsBridgeBypassActive;

	private static bool lightsBridgeBypassLoggedThisSuppression;

	private static int lightsBridgeBypassFrames;

	public static bool ShouldSkipCustomDrawingForLights
	{
		get
		{
			if (MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode && lightsStartupWarmupActive)
			{
				return !lightsStartupResourcesReady;
			}
			return false;
		}
	}

	public static bool ShouldBlockBladeUseForLights => ShouldSkipCustomDrawingForLights;

	public static bool ShouldBypassLightsBridgeHooks()
	{
		if (!MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode || !lightsBridgeBypassActive)
		{
			return false;
		}
		lightsBridgeBypassFrames++;
		if (!lightsBridgeBypassLoggedThisSuppression)
		{
			lightsBridgeBypassLoggedThisSuppression = true;
			LogLightsCompatibility("Bypassing Lights startup bridge hooks until the world and magic-blade resources are stable.");
		}
		return true;
	}

	public static void StartLightsStartupSuppression()
	{
		if (!Main.dedServ && MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode)
		{
			EnsureLightsBridgeBypassHooksInstalled();
			lightsStartupWarmupActive = true;
			lightsStartupResourcesReady = false;
			lightsStableDrawFrames = 0;
			lightsStableUpdateFrames = 0;
			lightsRequestedAssetWarmupStep = 0;
			lightsBundledWarmupStep = 0;
			lightsStartupSuppressionStartedAtMs = Environment.TickCount64;
			lightsBridgeBypassActive = true;
			lightsBridgeBypassLoggedThisSuppression = false;
			lightsBridgeBypassFrames = 0;
			warmupQueued = false;
			warmupStep = 0;
			warmupDelay = 0;
			SetLightsPostProcessingSuppressed(suppressed: true);
		}
	}

	public static void AddLight(Vector2 position, float r, float g, float b)
	{
		if (!ShouldSkipCustomDrawingForLights)
		{
			Lighting.AddLight(position, r, g, b);
		}
	}

	public static void QueueCombatWarmup()
	{
		if (!Main.dedServ)
		{
			if (MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode)
			{
				StartLightsStartupSuppression();
				return;
			}
			warmupQueued = true;
			warmupStep = 0;
			warmupDelay = 0;
			lightsRequestedAssetWarmupStep = 0;
			lightsBundledWarmupStep = 0;
		}
	}

	public override void PostSetupContent()
	{
		StartLightsStartupSuppression();
	}

	public override void OnWorldLoad()
	{
		StartLightsStartupSuppression();
	}

	public override void OnWorldUnload()
	{
		warmupQueued = false;
		warmupStep = 0;
		warmupDelay = 0;
		lightsStartupWarmupActive = false;
		lightsStartupResourcesReady = true;
		lightsStableDrawFrames = 0;
		lightsStableUpdateFrames = 0;
		lightsRequestedAssetWarmupStep = 0;
		lightsBundledWarmupStep = 0;
		lightsStartupSuppressionStartedAtMs = 0L;
		lightsBridgeBypassActive = false;
		lightsBridgeBypassLoggedThisSuppression = false;
		lightsBridgeBypassFrames = 0;
		SetLightsPostProcessingSuppressed(suppressed: false);
	}

	public override void Unload()
	{
		warmupQueued = false;
		warmupStep = 0;
		warmupDelay = 0;
		lightsStartupWarmupActive = false;
		lightsStartupResourcesReady = true;
		lightsStableDrawFrames = 0;
		lightsStableUpdateFrames = 0;
		lightsRequestedAssetWarmupStep = 0;
		lightsBundledWarmupStep = 0;
		lightsStartupSuppressionStartedAtMs = 0L;
		lightsBridgeBypassActive = false;
		lightsBridgeBypassLoggedThisSuppression = false;
		lightsBridgeBypassFrames = 0;
		SetLightsPostProcessingSuppressed(suppressed: false);
		DisposeLightsBridgeBypassHooks();
		lightsUseLightField = null;
		lightsUseBloomField = null;
	}

	public override void PostUpdateEverything()
	{
		if (Main.dedServ)
		{
			return;
		}
		if (!Main.gameMenu && MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode && lightsStartupWarmupActive && !lightsStartupResourcesReady)
		{
			if (lightsPostProcessingSuppressed)
			{
				SetLightsPostProcessingSuppressed(suppressed: true);
			}
			if (lightsStableUpdateFrames < 120)
			{
				lightsStableUpdateFrames++;
			}
			TryQueueLightsStartupWarmup();
		}
		if (!Main.gameMenu && warmupQueued)
		{
			if (warmupDelay > 0)
			{
				warmupDelay--;
			}
			else if (RunWarmupStep() && warmupQueued)
			{
				warmupDelay = GetWarmupStepSpacing();
			}
		}
	}

	public override void PostDrawInterface(SpriteBatch spriteBatch)
	{
		if (!Main.dedServ && !Main.gameMenu && MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode && lightsStartupWarmupActive && !lightsStartupResourcesReady)
		{
			if (lightsStableDrawFrames < 120)
			{
				lightsStableDrawFrames++;
			}
			TryQueueLightsStartupWarmup();
		}
	}

	private static bool RunWarmupStep()
	{
		switch (warmupStep)
		{
		case 0:
			if (WarmUpRequestedAssetStep())
			{
				lightsRequestedAssetWarmupStep = 0;
				warmupStep++;
			}
			return true;
		case 1:
			if (MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode)
			{
				if (WarmUpLightsBundledFrameStep())
				{
					warmupStep = 3;
				}
				return true;
			}
			if (MoDaoQianRenGreatswordFogVisuals.WarmUpStep())
			{
				warmupStep++;
			}
			return false;
		case 2:
			if (MoDaoQianRenGreatswordSlashVisuals.WarmUpStep())
			{
				warmupStep++;
			}
			return false;
		case 3:
			if (MoDaoQianRenCrimsonRiftArcVisuals.WarmUpStep())
			{
				warmupStep++;
			}
			return false;
		case 4:
			if (MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode)
			{
				CompleteLightsStartupWarmup();
				return false;
			}
			warmupQueued = false;
			return false;
		default:
			warmupQueued = false;
			return false;
		}
	}

	private static bool WarmUpLightsBundledFrameStep()
	{
		if (lightsBundledWarmupStep < 8)
		{
			MoDaoQianRenGreatswordFogVisuals.WarmUpBundledFrameStep(lightsBundledWarmupStep);
			lightsBundledWarmupStep++;
			return false;
		}
		int slashStep = lightsBundledWarmupStep - 8;
		if (slashStep < 12)
		{
			MoDaoQianRenGreatswordSlashVisuals.WarmUpBundledFrameStep(slashStep);
			lightsBundledWarmupStep++;
			return false;
		}
		int crimsonRiftStep = lightsBundledWarmupStep - 20;
		if (crimsonRiftStep < 16)
		{
			MoDaoQianRenCrimsonRiftArcVisuals.WarmUpStep();
			lightsBundledWarmupStep++;
			return false;
		}
		return true;
	}

	private static void QueueLightsStartupWarmup()
	{
		warmupQueued = true;
		warmupStep = 0;
		warmupDelay = 2;
		lightsRequestedAssetWarmupStep = 0;
		lightsBundledWarmupStep = 0;
	}

	private static void CompleteLightsStartupWarmup()
	{
		int bypassedFrames = lightsBridgeBypassFrames;
		warmupQueued = false;
		warmupStep = 0;
		warmupDelay = 0;
		lightsRequestedAssetWarmupStep = 0;
		lightsBundledWarmupStep = 0;
		lightsStartupResourcesReady = true;
		UpdateLightsStartupState();
		lightsBridgeBypassFrames = 0;
		LogLightsCompatibility($"Startup warmup complete after {bypassedFrames} bypassed calls; magic-blade visuals are enabled and Lights post-processing remains bypassed for stability.");
	}

	private static bool TryResolveLightsPostProcessingFields()
	{
		if ((object)lightsUseLightField != null && (object)lightsUseBloomField != null)
		{
			return true;
		}
		if (!ModLoader.TryGetMod("Lights", out var lightsMod))
		{
			return false;
		}
		Type lightsType = lightsMod.GetType().Assembly.GetType("Lights.Lights");
		if ((object)lightsType == null)
		{
			return false;
		}
		lightsUseLightField = lightsType.GetField("useLight", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		lightsUseBloomField = lightsType.GetField("useBloom", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if ((object)lightsUseLightField != null)
		{
			return (object)lightsUseBloomField != null;
		}
		return false;
	}

	private static void SetLightsPostProcessingSuppressed(bool suppressed)
	{
		if (Main.dedServ || !MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode || !TryResolveLightsPostProcessingFields())
		{
			if (suppressed)
			{
				LogLightsCompatibility("Unable to resolve Lights post-processing fields; bridge hook bypass remains the primary startup guard.");
			}
		}
		else if (suppressed)
		{
			if (!lightsOriginalPostProcessingCaptured)
			{
				object value = lightsUseLightField.GetValue(null);
				lightsOriginalUseLight = value is bool && (bool)value;
				value = lightsUseBloomField.GetValue(null);
				lightsOriginalUseBloom = value is bool && (bool)value;
				lightsOriginalPostProcessingCaptured = true;
			}
			lightsUseLightField.SetValue(null, false);
			lightsUseBloomField.SetValue(null, false);
			if (!lightsPostProcessingSuppressed)
			{
				LogLightsCompatibility("Temporarily disabled Lights light/bloom flags for startup.");
			}
			lightsPostProcessingSuppressed = true;
		}
		else if (!lightsOriginalPostProcessingCaptured)
		{
			lightsPostProcessingSuppressed = false;
		}
		else
		{
			lightsUseLightField.SetValue(null, lightsOriginalUseLight);
			lightsUseBloomField.SetValue(null, lightsOriginalUseBloom);
			lightsOriginalPostProcessingCaptured = false;
			lightsPostProcessingSuppressed = false;
			LogLightsCompatibility("Restored Lights light/bloom flags.");
		}
	}

	private static void EnsureLightsBridgeBypassHooksInstalled()
	{
		if (lightsBridgeBypassHooksInstalled || lightsBridgeBypassHookInstallFailed || Main.dedServ)
		{
			return;
		}
		try
		{
			if (!ModLoader.TryGetMod("Lights", out var lightsMod))
			{
				return;
			}
			Type lightsType = lightsMod.GetType().Assembly.GetType("Lights.Lights");
			if ((object)lightsType == null)
			{
				lightsBridgeBypassHookInstallFailed = true;
				LogLightsCompatibility("Unable to find Lights.Lights type; startup bridge bypass is unavailable.");
				return;
			}
			InstallLightsBridgeBypassHook(lightsType, "FilterManager_EndCapture");
			InstallLightsBridgeBypassHook(lightsType, "On_Main_InitTargets_int_int");
			lightsBridgeBypassHooksInstalled = lightsBridgeBypassHooks.Count > 0;
			if (lightsBridgeBypassHooksInstalled)
			{
				LogLightsCompatibility($"Installed {lightsBridgeBypassHooks.Count} Lights startup bridge bypass hook(s).");
			}
			else
			{
				lightsBridgeBypassHookInstallFailed = true;
				LogLightsCompatibility("No Lights startup bridge bypass hooks were installed.");
			}
		}
		catch (Exception ex)
		{
			lightsBridgeBypassHookInstallFailed = true;
			LogLightsCompatibility($"Failed to install Lights startup bridge bypass hooks: {ex}");
		}
	}

	private static void InstallLightsBridgeBypassHook(Type lightsType, string methodName)
	{
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Expected O, but got Unknown
		MethodInfo bridgeMethod = lightsType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if ((object)bridgeMethod == null)
		{
			LogLightsCompatibility("Unable to find Lights bridge method " + methodName + ".");
			return;
		}
		ParameterInfo[] bridgeParameters = bridgeMethod.GetParameters();
		if (bridgeParameters.Length == 0 || !typeof(Delegate).IsAssignableFrom(bridgeParameters[0].ParameterType))
		{
			LogLightsCompatibility("Lights bridge method " + methodName + " does not expose an orig delegate parameter.");
			return;
		}
		Type bridgeOrigDelegateType = bridgeParameters[0].ParameterType;
		MethodInfo bridgeOrigInvoke = bridgeOrigDelegateType.GetMethod("Invoke");
		if ((object)bridgeOrigInvoke == null)
		{
			LogLightsCompatibility("Unable to inspect orig delegate for Lights bridge method " + methodName + ".");
			return;
		}
		Type[] bridgeOrigInvokeParameters = (from parameter in bridgeOrigInvoke.GetParameters()
			select parameter.ParameterType).ToArray();
		Type[] bridgeOrigCallParameters = (from parameter in bridgeParameters.Skip(1)
			select parameter.ParameterType).ToArray();
		if (!CanForwardBridgeArgumentsToOrig(bridgeOrigCallParameters, bridgeOrigInvokeParameters))
		{
			LogLightsCompatibility("Lights bridge method " + methodName + " orig delegate signature did not match the method tail.");
			return;
		}
		Type[] originalMethodParameterTypes = new Type[1] { lightsType }.Concat(bridgeParameters.Select((ParameterInfo parameter) => parameter.ParameterType)).ToArray();
		Type originalMethodDelegateType = Expression.GetActionType(originalMethodParameterTypes);
		Type[] hookParameterTypes = new Type[1] { originalMethodDelegateType }.Concat(originalMethodParameterTypes).ToArray();
		Type hookDelegateType = Expression.GetActionType(hookParameterTypes);
		Delegate hookDelegate = BuildLightsBridgeBypassHookMethod(methodName, hookParameterTypes, bridgeOrigDelegateType, bridgeOrigCallParameters.Length).CreateDelegate(hookDelegateType);
		lightsBridgeBypassHooks.Add((IDisposable)new Hook((MethodBase)bridgeMethod, hookDelegate));
	}

	private static bool CanForwardBridgeArgumentsToOrig(Type[] sourceTypes, Type[] targetTypes)
	{
		if (sourceTypes.Length != targetTypes.Length)
		{
			return false;
		}
		for (int i = 0; i < sourceTypes.Length; i++)
		{
			Type sourceType = sourceTypes[i];
			Type targetType = targetTypes[i];
			if (!(sourceType == targetType) && (sourceType.IsValueType || !targetType.IsAssignableFrom(sourceType)))
			{
				return false;
			}
		}
		return true;
	}

	private static MethodInfo BuildLightsBridgeBypassHookMethod(string methodName, Type[] hookParameterTypes, Type bridgeOrigDelegateType, int bridgeOrigCallParameterCount)
	{
		AssemblyName assemblyName = new AssemblyName($"MoDaoQianRenLightsBridgeBypass_{methodName}_{Guid.NewGuid():N}");
		TypeBuilder typeBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(assemblyName.Name).DefineType("MoDaoQianRenLightsBridgeBypass_" + methodName, TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
		ILGenerator il = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.Static, typeof(void), hookParameterTypes).GetILGenerator();
		Label callLightsBridge = il.DefineLabel();
		MethodInfo shouldBypassMethod = typeof(MoDaoQianRenWarmupSystem).GetMethod("ShouldBypassLightsBridgeHooks", BindingFlags.Static | BindingFlags.Public);
		MethodInfo bridgeOrigInvoke = bridgeOrigDelegateType.GetMethod("Invoke");
		MethodInfo originalBridgeInvoke = hookParameterTypes[0].GetMethod("Invoke");
		il.Emit(OpCodes.Call, shouldBypassMethod);
		il.Emit(OpCodes.Brfalse_S, callLightsBridge);
		EmitLoadArg(il, 2);
		for (int i = 0; i < bridgeOrigCallParameterCount; i++)
		{
			EmitLoadArg(il, i + 3);
		}
		il.Emit(OpCodes.Callvirt, bridgeOrigInvoke);
		il.Emit(OpCodes.Ret);
		il.MarkLabel(callLightsBridge);
		for (int j = 0; j < hookParameterTypes.Length; j++)
		{
			EmitLoadArg(il, j);
		}
		il.Emit(OpCodes.Callvirt, originalBridgeInvoke);
		il.Emit(OpCodes.Ret);
		return typeBuilder.CreateType().GetMethod("Invoke");
	}

	private static void EmitLoadArg(ILGenerator il, int index)
	{
		switch (index)
		{
		case 0:
			il.Emit(OpCodes.Ldarg_0);
			return;
		case 1:
			il.Emit(OpCodes.Ldarg_1);
			return;
		case 2:
			il.Emit(OpCodes.Ldarg_2);
			return;
		case 3:
			il.Emit(OpCodes.Ldarg_3);
			return;
		}
		if (index <= 255)
		{
			il.Emit(OpCodes.Ldarg_S, (byte)index);
		}
		else
		{
			il.Emit(OpCodes.Ldarg, index);
		}
	}

	private static void DisposeLightsBridgeBypassHooks()
	{
		for (int i = lightsBridgeBypassHooks.Count - 1; i >= 0; i--)
		{
			lightsBridgeBypassHooks[i].Dispose();
		}
		lightsBridgeBypassHooks.Clear();
		lightsBridgeBypassHooksInstalled = false;
		lightsBridgeBypassHookInstallFailed = false;
	}

	private static void LogLightsCompatibility(string message)
	{
		try
		{
			ModContent.GetInstance<MoDaoQianRenMod>().Logger.Info((object)("[Lights compatibility] " + message));
		}
		catch
		{
		}
	}

	private static int GetWarmupStepSpacing()
	{
		if (!MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode)
		{
			return 0;
		}
		return 2;
	}

	private static void TryQueueLightsStartupWarmup()
	{
		if (MoDaoQianRenMod.UseLightsAndShadowsCompatibilityMode && lightsStartupWarmupActive && !lightsStartupResourcesReady && !warmupQueued && lightsStableDrawFrames >= 120 && lightsStableUpdateFrames >= 120 && Environment.TickCount64 - lightsStartupSuppressionStartedAtMs >= 2500)
		{
			QueueLightsStartupWarmup();
		}
	}

	private static void UpdateLightsStartupState()
	{
		if (lightsStartupWarmupActive && lightsStartupResourcesReady)
		{
			lightsStartupWarmupActive = false;
		}
	}

	private static bool WarmUpRequestedAssetStep()
	{
		switch (lightsRequestedAssetWarmupStep)
		{
		case 0:
			_ = ModContent.Request<Texture2D>(MoDaoQianRenMod.WeaponHiltTexture).Value;
			break;
		case 1:
			_ = ModContent.Request<Texture2D>("魔刀千刃/Content/Items/Weapons/MoDaoQianRen_weaponout_pulse").Value;
			break;
		case 2:
			_ = ModContent.Request<Texture2D>(MoDaoQianRenMod.InventoryIconPulseTexture).Value;
			break;
		case 3:
			_ = TextureAssets.Extra[98].Value;
			break;
		case 4:
			MoDaoQianRenShardVisuals.WarmUp();
			break;
		default:
			return true;
		}
		lightsRequestedAssetWarmupStep++;
		return lightsRequestedAssetWarmupStep >= 5;
	}
}
