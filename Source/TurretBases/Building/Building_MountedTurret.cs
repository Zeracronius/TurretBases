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

namespace TurretBases.Building
{
    public class Building_MountedTurret : Building_TurretGun, ICanInstallGun
    {
		private Gizmo _removeWeapon;
		private Gizmo _selectWeapon;
		private TurretBaseDef _turretBaseDef;
		private Thing? _gunToInstall;

		public Thing? GunToInstall
		{
			get => _gunToInstall;
			set => _gunToInstall = value;
		}

		public Building_MountedTurret()
			: base()
        {
		}

		public override string Label => string.Format(base.Label, gun.LabelNoParenthesisCap);
		public override void PostMake()
		{
			base.PostMake();
			_removeWeapon = new Command_Action()
			{
				action = ConvertToPlatform,
				defaultLabel = "Eject weapon"
			};
			_selectWeapon = new Command_Action()
			{
				action = OpenGunSelection,
				defaultLabel = "Replace weapon"
			};
			_turretBaseDef = ((TurretBaseDef)def).relatedTurretDef;
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
				// Add designation if not already assigned.
				if (Map.designationManager.HasMapDesignationOn(this) == false)
					Map.designationManager.AddDesignation(new Designation(this, TB_DesignationDefOf.InstallWeapon));
			}
		}

		public void SetGun(Thing gun)
		{
			base.gun = gun;
			List<Verb> allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				Verb verb = allVerbs[i];
				verb.caster = this;
				verb.castCompleteCallback = OnBurstCompleted;
			}
			if (gun.Spawned)
				gun.DeSpawn();
			gun.ForceSetStateToUnspawned();
		}

		private void OnBurstCompleted()
		{
			base.BurstComplete();

			// When salvo is finished, check if attached weapon is dead, and convert to platform.
			if (gun.Destroyed)
				ConvertToPlatform();
		}
		

		private void ConvertToPlatform()
		{
			IntVec3 position = Position;
			Map map = Map;
			DeSpawn();

			Building_TurretBase turretBase = (Building_TurretBase)ThingMaker.MakeThing(_turretBaseDef);
			turretBase.SetFactionDirect(factionInt);
			GenPlace.TryPlaceThing(turretBase, position, map, ThingPlaceMode.Direct);
		}

		
		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			// Before vanishing, destroyed or despawning, drop any attached weapon.
			EjectGun();
			base.DeSpawn(mode);
		}

		private void EjectGun()
		{
			// Drop mounted gun unless broken (like for consumables).
			if (gun.Destroyed == false)
			{
				gun.stackCount = 1;
				GenSpawn.Spawn(gun, Position, Map);
			}
		}

		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			// Draw attached gun if any, and make sure to draw it above the turret.
			if (gun != null)
				PawnRenderUtility.DrawEquipmentAiming(gun, drawLoc + Altitudes.AltIncVect, top.CurRotation);

			// Draw turret frame and shadows.
			if (def.drawerType == DrawerType.RealtimeOnly || !Spawned)
				Graphic.Draw(drawLoc, flip ? Rotation.Opposite : Rotation, this);
			SilhouetteUtility.DrawGraphicSilhouette(this, drawLoc);

			Comps_PostDraw();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return _selectWeapon;
			yield return _removeWeapon;
		}

		/// <summary>
		/// Drop current weapon on the ground and replace it with another.
		/// </summary>
		/// <param name="newGun"></param>
		public void InstallGun(Thing newGun)
		{
			base.Map.designationManager.DesignationOn(this, TB_DesignationDefOf.InstallWeapon)?.Delete();
			EjectGun();
			SetGun(newGun);
		}

		public override string GetInspectString()
		{
			return base.GetInspectString() + Environment.NewLine + gun.GetInspectString();
		}
	}
}
