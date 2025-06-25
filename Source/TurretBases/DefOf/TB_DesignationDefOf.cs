using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TurretBases.DefOf
{
	[RimWorld.DefOf]
	public static class TB_DesignationDefOf
	{
		static TB_DesignationDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TB_DesignationDefOf));
		}

		public static DesignationDef? InstallWeapon;
		public static DesignationDef? RemoveWeapon;
	}
}
