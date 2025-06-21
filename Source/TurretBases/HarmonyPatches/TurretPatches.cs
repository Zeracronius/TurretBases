using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurretBases.Building;

namespace TurretBases.HarmonyPatches
{
	[HarmonyPatch]
	internal static class TurretPatches
	{

		//[HarmonyPatch(typeof(Building_TurretGun), nameof(Building_TurretGun.MakeGun)), HarmonyPrefix]
		//static bool MakeGunPrefix(Building_TurretGun __instance)
		//{
		//	// Block MakeGun method for TurretBase
		//	if (__instance is Building_TurretBase)
		//		return false;

		//	return true;
		//}
	}
}
