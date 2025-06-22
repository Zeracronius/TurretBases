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
	internal class TB_ThingDefOf
	{
		static TB_ThingDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TB_ThingDefOf));
		}

	}
}
