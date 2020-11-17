using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Studio;

namespace StudioCharaSort
{
	[BepInPlugin(GUID, PluginName, Version)]
	[BepInProcess(Constants.StudioProcessName)]
	public class StudioCharaSort : BaseUnityPlugin
	{
#if AI
		public const string GUID = "kky.ai.studiocharasort";
#elif HS2
		public const string GUID = "kky.hs2.studiocharasort";
#elif KK
		public const string GUID = "kky.kk.studiocharasort";
#endif
		public const string PluginName = "Studio Character Sort";
		internal const string Version = "1.0.2.0";

		public static ConfigEntry<sortTypes> ConfigSType;
		public static ConfigEntry<sortOrders> ConfigSOrder;
		public enum sortTypes { Name, Date }
		public enum sortOrders { Descending, Ascending }
		public static bool sortAscend;

		private void Awake()
		{
			ConfigSType = Config.Bind("Character cards default sort values. Changes take effect at next startup.", "Sort By", sortTypes.Name, "Set custom default sort type. Game default is Name");
			ConfigSOrder = Config.Bind("Character cards default sort values. Changes take effect at next startup.", "Sort Order", sortOrders.Descending, "Set custom default sort order. Game default is Descending");
			sortAscend = (ConfigSOrder.Value != sortOrders.Descending);
			Harmony HarmonyInstance = new Harmony("StudioCharaSort");
			HarmonyInstance.PatchAll(typeof(StudioCharaSort));

			Type CostumeInfoType = typeof(MPCharCtrl).GetNestedType("CostumeInfo", BindingFlags.NonPublic);
			HarmonyInstance.Patch(CostumeInfoType.GetMethod("InitList", AccessTools.all), postfix : new HarmonyMethod(typeof(Patches), nameof(Patches.InitListPostfix)));
		}

		[HarmonyPostfix, HarmonyPatch(typeof(CharaList), "InitCharaList")]
		public static void InitCharaList(CharaFileSort ___charaFileSort)
		{
			bool flag = !(ConfigSType.Value == sortTypes.Name & !sortAscend);
			if (flag)
				___charaFileSort.Sort((int) ConfigSType.Value, sortAscend);
		}
	}

	public class Patches
	{
		public static void InitListPostfix(object __instance)
		{
			Traverse.Create(__instance).Field("fileSort").GetValue<CharaFileSort>().Sort((int) StudioCharaSort.ConfigSType.Value, StudioCharaSort.sortAscend);
		}
	}
}