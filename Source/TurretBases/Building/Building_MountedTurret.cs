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
		private const int SMOKE_COOLDOWN = 15;

		private Gizmo _removeWeapon;
		private Gizmo _selectWeapon;
		private Gizmo _examineWeapon;
		private TurretBaseDef _turretBaseDef;
		private Graphic? _turretGraphics;
		private Thing? _gunToInstall;
		private Verb_LaunchProjectile? _recoiledProjectile;
		private float _burstCooldownFactor;
		private bool _tooDamaged;
		private int _smokeTicks;
		private float _partialDamage;
		private bool _loadedTexture;

		public Thing? GunToInstall
		{
			get => _gunToInstall;
			set => _gunToInstall = value;
		}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
		public Building_MountedTurret()
			: base()
		{
		}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

		public override string LabelNoCount => $"{base.LabelNoCount}: {gun.LabelNoParenthesisCap}";
		public override string LabelNoParenthesis => $"{base.LabelNoParenthesis}: {gun.LabelNoParenthesisCap}";


		protected override void Tick()
		{
			if (_tooDamaged == false)
				base.Tick();
		}

		protected override void TickInterval(int delta)
		{
			base.TickInterval(delta);

			if (_tooDamaged && Spawned)
			{
				if (_smokeTicks < 0)
				{
					FleckMaker.ThrowSmoke(DrawPos, base.Map, 0.1f);
					_smokeTicks = SMOKE_COOLDOWN;
				}
				_smokeTicks -= delta;
			}
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);

			_removeWeapon = new Command_Action()
			{
				action = ScheduleUninstall,
				defaultLabel = "TurretBases_UninstallWeapon".TranslateSimple(),
				
			};
			_selectWeapon = new Command_Action()
			{
				action = OpenGunSelection,
				defaultLabel = "TurretBases_ReplaceWeapon".TranslateSimple(),
			};
			_examineWeapon = new Command_Action()
			{
				action = () => Find.WindowStack.Add(new Dialog_InfoCard(gun)),
				defaultLabel = gun.LabelCap,
				icon = gun.def.uiIcon
			};

			_turretBaseDef = ((TurretBaseDef)def).relatedTurretDef!;
			_burstCooldownFactor = ((TurretBaseDef)def).burstCooldownFactor!;
			if (_burstCooldownFactor == 0)
				_burstCooldownFactor = 1;

			_smokeTicks = SMOKE_COOLDOWN;

			if (gun != null)
				SetGun(gun);

		}

		private void ScheduleUninstall()
		{
			// Add designation if not already assigned.
			if (Map.designationManager.DesignationOn(this, TB_DesignationDefOf.RemoveWeapon) == null)
				Map.designationManager.AddDesignation(new Designation(this, TB_DesignationDefOf.RemoveWeapon));
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
				// Add designation if not already assigned.
				if (Map.designationManager.DesignationOn(this, TB_DesignationDefOf.InstallWeapon) == null)
					Map.designationManager.AddDesignation(new Designation(this, TB_DesignationDefOf.InstallWeapon));
			}
		}

		public void SetGun(Thing gun)
		{
			base.gun = gun;
			List<Verb> allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
			_recoiledProjectile = EquipmentUtility.GetRecoilVerb(allVerbs);
			for (int i = 0; i < allVerbs.Count; i++)
			{
				Verb verb = allVerbs[i];
				verb.caster = this;
				verb.castCompleteCallback = OnBurstCompleted;
			}
			if (gun.Spawned)
				gun.DeSpawn();
			gun.ForceSetStateToUnspawned();

			if (Mod.TurretBasesMod.Settings.WeaponsCanBreak && _partialDamage > gun.HitPoints)
				_tooDamaged = true;
		}

		protected override float BurstCooldownTime()
		{
			return AttackVerb.verbProps.defaultCooldownTime * _burstCooldownFactor;
		}

		private void OnBurstCompleted()
		{
			base.BurstComplete();

			Mod.TurretBasesSettings settings = Mod.TurretBasesMod.Settings;

			if (settings.WeaponsDegrade && _tooDamaged == false)
			{
				_partialDamage += (float)gun.MaxHitPoints / settings.ShotsToBreakWeapon;

				if (_partialDamage > 1)
				{
					// If guns can break or damage is less than needed to break it.
					int damage = (int)_partialDamage;
					if (settings.WeaponsCanBreak || damage < gun.HitPoints)
					{
						gun.HitPoints -= damage;
						if (gun.HitPoints < 1)
							gun.Destroy();
					}
					else
						_tooDamaged = true;
				}
			}

			// When salvo is finished, check if attached weapon is dead, and convert to platform.
			if (gun.Destroyed)
				ConvertToPlatform();
		}

		public void ConvertToPlatform()
		{
			IntVec3 position = Position;
			Map map = Map;
			DeSpawn();

			Building_TurretBase turretBase = (Building_TurretBase)ThingMaker.MakeThing(_turretBaseDef, Stuff);
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
			if (_loadedTexture == false)
			{
				var modExtension = gun.def.GetModExtension<ModExtensions.TurretBaseExtension>();
				_turretGraphics = modExtension?.turretGraphicData?.Graphic;
				_loadedTexture = true;
			}

			// Draw attached gun if any, and make sure to draw it above the turret.
			float aimRotation = top.CurRotation - 90f;
			if (_turretGraphics != null)
			{
				_turretGraphics.Draw(drawLoc + Altitudes.AltIncVect, Rotation, gun, aimRotation);
			}
			else
				DrawEquipment(gun, drawLoc + Altitudes.AltIncVect, aimRotation);

			// Draw turret frame and shadows.
			if (def.drawerType == DrawerType.RealtimeOnly || !Spawned)
				Graphic.Draw(drawLoc, flip ? Rotation.Opposite : Rotation, this);
			SilhouetteUtility.DrawGraphicSilhouette(this, drawLoc);

			Comps_PostDraw();
		}

		private void DrawEquipment(Thing eq, Vector3 drawLoc, float aimAngle)
		{
			aimAngle += eq.def.equippedAngleOffset;
			aimAngle %= 360f;

			EquipmentUtility.Recoil(eq.def, _recoiledProjectile, out Vector3 drawOffset, out float angleOffset, aimAngle);
			drawLoc += drawOffset;
			aimAngle += angleOffset;

			Material material = ((!(eq.Graphic is Graphic_StackCount graphic_StackCount)) ? eq.Graphic.MatSingleFor(eq) : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq));
			Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(eq.Graphic.drawSize.x, 0f, eq.Graphic.drawSize.y), pos: drawLoc, q: Quaternion.AngleAxis(aimAngle, Vector3.up));
			Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return _selectWeapon;
			yield return _removeWeapon;
			yield return _examineWeapon;
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
			string description = base.GetInspectString();
			string gunDescription = gun.GetInspectString();

			if (String.IsNullOrWhiteSpace(gunDescription) == false)
				description += Environment.NewLine + gunDescription;

			if (_tooDamaged)
				description += Environment.NewLine + "CannotFire".TranslateSimple() + ": " + "TurretBases_TooDamaged".TranslateSimple();

			return description;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref _gunToInstall, "GunToInstall");
			Scribe_Values.Look(ref _partialDamage, "PartialDamage");
		}
	}
}
