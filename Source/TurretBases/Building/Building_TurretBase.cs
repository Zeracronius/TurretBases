using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurretBases.DefOf;
using TurretBases.Defs;
using TurretBases.Interfaces;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace TurretBases.Building
{
    public class Building_TurretBase : Verse.Building, ICanInstallGun
	{
		private Gizmo _selectWeapon;
		private TurretBaseDef _turretBaseDef;
		private Thing? _gunToInstall;

		public Thing? GunToInstall
		{
			get => _gunToInstall;
			set => _gunToInstall = value;
		}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
		public Building_TurretBase()
			: base()
		{
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

		
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			_turretBaseDef = ((TurretBaseDef)def).relatedTurretDef!;
			_selectWeapon = new Command_Action()
			{
				action = OpenGunSelection,
				icon = _turretBaseDef.uiIcon,
				defaultLabel = "TurretBases_SelectWeapon".TranslateSimple()
			};
		}

		public void InstallGun(Thing gun)
		{
			base.Map.designationManager.DesignationOn(this, TB_DesignationDefOf.InstallWeapon)?.Delete();

			IntVec3 position = Position;
			Map map = Map;
			this.DeSpawn();

			Building_MountedTurret mountedTurret = (Building_MountedTurret)ThingMaker.MakeThing(_turretBaseDef, Stuff);
			mountedTurret.SetGun(gun);
			mountedTurret.SetFactionDirect(factionInt);
			GenPlace.TryPlaceThing(mountedTurret, position, map, ThingPlaceMode.Direct);
		}

		private void OpenGunSelection()
		{
			Dialog_SelectGun dialog = new Dialog_SelectGun(Map, def.BaseMass, GunSelected);
			Find.WindowStack.Add(dialog);
		}

		private void GunSelected(Thing? selectedGun)
		{
			if (selectedGun != null)
			{
				GunToInstall = selectedGun;
				Map.designationManager.AddDesignation(new Designation(this, TB_DesignationDefOf.InstallWeapon));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref _gunToInstall, "GunToInstall");
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return _selectWeapon;
		}
	}
}
