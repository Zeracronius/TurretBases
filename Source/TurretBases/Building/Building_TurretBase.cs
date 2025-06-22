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

		public Building_TurretBase()
			: base()
        {
		}

		public override void PostMake()
		{
			base.PostMake();

			_selectWeapon = new Command_Action()
			{
				action = OpenGunSelection,
				defaultLabel = "Select weapon"
			};
			_turretBaseDef = ((TurretBaseDef)def).relatedTurretDef;
		}

		public void InstallGun(Thing gun)
		{
			base.Map.designationManager.DesignationOn(this, TB_DesignationDefOf.InstallWeapon)?.Delete();

			IntVec3 position = Position;
			Map map = Map;
			this.DeSpawn();

			Building_MountedTurret mountedTurret = (Building_MountedTurret)ThingMaker.MakeThing(_turretBaseDef);
			mountedTurret.SetGun(gun);
			mountedTurret.SetFactionDirect(factionInt);
			GenPlace.TryPlaceThing(mountedTurret, position, map, ThingPlaceMode.Direct);
		}

		private void OpenGunSelection()
		{
			Dialog_SelectGun dialog = new Dialog_SelectGun(Map, GunSelected);
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
