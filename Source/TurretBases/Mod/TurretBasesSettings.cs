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
			var degradationNode = new TreeNode_FilterBox("Degredation", "Weapons degrades when shot by turret platform.",
					(in Rect rect) => Widgets.Checkbox(rect.position, ref WeaponsDegrade));

			degradationNode.AddChild("Weapon can break", "If enabled, mounted guns will keep shooting until they break, otherwise it will stop firing when too damaged.",
					(in Rect rect) => Widgets.Checkbox(rect.position, ref WeaponsCanBreak));

			degradationNode.AddChild("Bursts to destroy", "Number of bursts a weapon will last before breaking.",
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
			base.ExposeData();
		}
	}
}
