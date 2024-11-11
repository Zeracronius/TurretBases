using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurretBases.DefOf;
using TurretBases.Interfaces;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace TurretBases
{
    public class Building_TurretBase : Building_TurretGun
    {
		public Thing? GunToInstall;

		private Gizmo _selectWeapon;

        public Building_TurretBase()
			: base()
        {
			_selectWeapon = new Command_Action()
			{
				action = OpenGunSelection,
				defaultLabel = "Select gun"
			};
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

		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			if (def.drawerType == DrawerType.RealtimeOnly || !Spawned)
				Graphic.Draw(drawLoc, flip ? Rotation.Opposite : Rotation, this);

			if (gun != null)
				PawnRenderUtility.DrawEquipmentAiming(gun, drawLoc, top.CurRotation);

			SilhouetteUtility.DrawGraphicSilhouette(this, drawLoc);
			Comps_PostDraw();
		}

		public override string GetInspectString()
		{
			if (gun != null)
				return base.GetInspectString();
			else
				return "No gun installed";
		}

		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_References.Look(ref GunToInstall, "GunToInstall");
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
