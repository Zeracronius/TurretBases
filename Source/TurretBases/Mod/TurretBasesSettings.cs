using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurretBases.UserInterface.TreeBox;
using UnityEngine;
using Verse;

namespace TurretBases.Mod
{
	public class TurretBasesSettings : ModSettings
	{
		public bool WeaponsDegrade = true;
		public bool WeaponsCanBreak = true;
		public uint ShotsToBreakWeapon = 1000;

		internal List<TreeNode_FilterBox> GetSettingsNodes()
		{
			var degradationNode = new TreeNode_FilterBox("TurretBases_SettingsDegredation".Translate(), "TurretBases_SettingsDegredationTooltip".Translate(),
					(in Rect rect) => Widgets.Checkbox(rect.position, ref WeaponsDegrade));

			degradationNode.AddChild("TurretBases_SettingsCanBreak", "TurretBases_SettingsCanBreakTooltip",
					(in Rect rect) => Widgets.Checkbox(rect.position, ref WeaponsCanBreak));

			degradationNode.AddChild("TurretBases_SettingsBurstsToDestroy", "TurretBases_SettingsBurstsToDestroyTooltip",
					(in Rect rect) => ShotsToBreakWeapon = (uint)Widgets.HorizontalSlider(rect, ShotsToBreakWeapon, 100f, 10000f, 
						label: ShotsToBreakWeapon.ToString(), leftAlignedLabel: "100", rightAlignedLabel: "10000", roundTo: 1), splitRow: true);


			List<TreeNode_FilterBox> nodes = new List<TreeNode_FilterBox>
			{
				degradationNode
			};
			return nodes;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref WeaponsDegrade, nameof(WeaponsDegrade));
			Scribe_Values.Look(ref WeaponsCanBreak, nameof(WeaponsCanBreak));
			Scribe_Values.Look(ref ShotsToBreakWeapon, nameof(ShotsToBreakWeapon));
			base.ExposeData();
		}
	}
}
