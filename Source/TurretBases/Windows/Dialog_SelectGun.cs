using RimWorld;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurretBases.UserInterface;
using TurretBases.UserInterface.TableBox;
using TurretBases.Utilities.Collections;
using UnityEngine;
using Verse;

namespace TurretBases.Interfaces
{
	internal class Dialog_SelectGun : Window
	{
		Action<Thing?>? _callback;
		Table<TableRow<Thing>>? _table;
		Map _map;

		public Thing? SelectedGun;

        public Dialog_SelectGun(Map map, Action<Thing?>? callback)
        {
			_map = map;
			_callback = callback;
		}

        public override void DoWindowContents(Rect inRect)
		{
			inRect.SplitHorizontally(inRect.height - 70, out Rect top, out Rect bottom);
			_table?.Draw(top);

			if (Widgets.ButtonText(bottom, "Select"))
			{
				SelectedGun = _table?.SelectedRows.FirstOrDefault()?.Tag as Thing;
				this.Close();
			}
		}

		private void DoWeaponTile(Rect inRect)
		{

		}

		public override void PreOpen()
		{
			base.PreOpen();


			_table = new Table<TableRow<Thing>>((row, value) => row.SearchString.Contains(value));
			_table.MultiSelect = false;
			_table.DrawBorder = true;

			TableColumn<TableRow<Thing>> colDef = _table.AddColumn("", 30f, DrawIcon);
			colDef.IsFixedWidth = true;

			TableColumn<TableRow<Thing>> colLabel = _table.AddColumn("Caption", 1f);
			colLabel.IsFixedWidth = false;

			foreach (var item in _map.listerThings.AllThings)
			{
				if (item.def.IsRangedWeapon && item.Spawned && (item as ThingWithComps)?.GetComp<CompForbiddable>()?.Forbidden != true)
				{
					var row = new TableRow<Thing>(item, item.Label.ToLower());
					row[colLabel] = item.LabelCap;
					row.Tag = item;
					row.Tooltip = item.DescriptionDetailed;
					_table.AddRow(row);
				}
			}
			_table.Refresh();
		}

		private void DrawIcon(ref Rect boundingBox, TableRow<Thing> item)
		{
			Widgets.ThingIcon(boundingBox, item.Tag as Thing);
		}

		public override void PreClose()
		{
			base.PreClose();
		}

		public override void PostClose()
		{
			base.PostClose();
			_callback?.Invoke(SelectedGun);
		}
	}
}
