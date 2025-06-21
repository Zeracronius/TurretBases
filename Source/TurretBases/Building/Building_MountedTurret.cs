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

namespace TurretBases.Building
{
    public class Building_MountedTurret : Building_TurretGun
    {
		private Gizmo _removeWeapon;

		public Building_MountedTurret()
			: base()
        {
			_removeWeapon = new Command_Action()
			{
				action = RemoveGun,
				defaultLabel = "Uninstall gun"
			};
		}

		public void SetGun(Thing gun)
		{
			base.gun = gun;
			List<Verb> allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				Verb verb = allVerbs[i];
				verb.caster = this;
				verb.castCompleteCallback = BurstComplete;
			}
			if (gun.Spawned)
				gun.DeSpawn();
			gun.ForceSetStateToUnspawned();
		}

		private void RemoveGun()
		{
			IntVec3 position = Position;
			Map map = Map;
			DeSpawn();

			Building_TurretBase turretBase = (Building_TurretBase)ThingMaker.MakeThing(TB_ThingDefOf.Turret_Base);
			turretBase.SetFactionDirect(factionInt);
			GenPlace.TryPlaceThing(turretBase, position, map, ThingPlaceMode.Direct);
		}


		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			// Drop mounted gun.
			gun.stackCount = 1;
			GenSpawn.Spawn(gun, Position, Map);
			base.DeSpawn(mode);
		}

		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			if (gun != null)
				PawnRenderUtility.DrawEquipmentAiming(gun, drawLoc + Altitudes.AltIncVect, top.CurRotation);

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
			yield return _removeWeapon;
		}
	}
}
