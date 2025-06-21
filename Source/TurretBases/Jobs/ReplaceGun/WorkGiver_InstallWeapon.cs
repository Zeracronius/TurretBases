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
	internal class WorkGiver_InstallWeapon : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation item in pawn.Map.designationManager.designationsByDef[TB_DesignationDefOf.InstallWeapon])
			{
				yield return item.target.Thing;
			}
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(TB_DesignationDefOf.InstallWeapon);
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (pawn.Map.designationManager.DesignationOn(t, TB_DesignationDefOf.InstallWeapon) == null)
				return false;

			if (pawn.CanReserve(t, 1, -1, null, forced) == false)
				return false;

			if (t is Building_TurretBase turretBase)
			{
				if (pawn.CanReserve(turretBase.GunToInstall, 1, 1, null, forced) == false ||
					turretBase.GunToInstall.IsForbidden(pawn))
				{
					// Gun is no longer available. Cancel job.
					turretBase.Map.designationManager.DesignationOn(turretBase, TB_DesignationDefOf.InstallWeapon)?.Delete();
					Messages.Message("Install job cancelled as gun is no longer available.", turretBase, MessageTypeDefOf.TaskCompletion, historical: false);
					return false;
				}

				if (pawn.CanReserveAndReach(turretBase.GunToInstall, PathEndMode.Touch, DangerUtility.NormalMaxDanger(pawn), 1, 1, null, forced) == false)
					return false;
			}
			else
				return false;

			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Job job = JobMaker.MakeJob(TB_JobDefOf.InstallWeapon, t, ((Building_TurretBase)t).GunToInstall);
			job.count = 1;
			return job;
		}
	}
}
