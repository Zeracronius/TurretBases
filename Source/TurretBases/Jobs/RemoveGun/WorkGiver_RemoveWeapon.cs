using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using TurretBases.DefOf;
using TurretBases.Building;

namespace TurretBases
{
	internal class WorkGiver_RemoveWeapon : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation item in pawn.Map.designationManager.designationsByDef[TB_DesignationDefOf.RemoveWeapon])
			{
				yield return item.target.Thing;
			}
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			if (!pawn.Map.designationManager.AnySpawnedDesignationOfDef(TB_DesignationDefOf.RemoveWeapon))
				return true;

			if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) == false)
				return true;

			return false;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (pawn.Map.designationManager.DesignationOn(t, TB_DesignationDefOf.RemoveWeapon) == null)
				return false;

			if (t is Building_MountedTurret turret)
			{
				if (pawn.CanReserveAndReach(turret, PathEndMode.Touch, DangerUtility.NormalMaxDanger(pawn), 1, 1, null, forced) == false)
					return false;

				return true;
			}

			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(TB_JobDefOf.RemoveWeapon, t);
		}
	}
}
