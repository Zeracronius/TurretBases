using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using TurretBases.DefOf;
using TurretBases.Building;
using RimWorld;

namespace TurretBases
{
	/// <summary>
	/// Job driver for installing a weapon on turret base. TargetA is turret and TargetB is weapon.
	/// Giver already verified that both are available.
	/// </summary>
	/// <seealso cref="Verse.AI.JobDriver" />
	internal class JobDriver_InstallWeapon : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed) == false)
				return false;

			if (pawn.Reserve(job.targetB, job, 1, 1, null, errorOnFailed) == false)
				return false;

			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOnDestroyedOrNull(TargetIndex.B);
			this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

			this.FailOn(() => base.Map.designationManager.DesignationOn(base.TargetThingA, TB_DesignationDefOf.InstallWeapon) == null);

			// Go to B (New gun)
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch)
				.FailOnDespawnedNullOrForbidden(TargetIndex.B);

			yield return Toils_Haul.StartCarryThing(TargetIndex.B);

			// Go to A (Turret base)
			yield return Toils_Goto.GotoBuild(TargetIndex.A);
			yield return Toils_General.Wait(15).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);

			Toil finalize = ToilMaker.MakeToil("InstallWeaponToils");
			finalize.initAction = delegate
			{
				pawn.carryTracker.DestroyCarriedThing();
				Building_TurretBase turret = (Building_TurretBase)TargetThingA;
				turret?.InstallGun(TargetThingB);
			};
			finalize.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return finalize;
		}
	}
}
