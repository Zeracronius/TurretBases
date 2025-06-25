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
	internal class TB_JobDefOf
	{
		static TB_JobDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TB_JobDefOf));
		}

		public static JobDef? InstallWeapon;
		public static JobDef? RemoveWeapon;
	}
}
