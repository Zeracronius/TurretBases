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
		float _maxMass;

		public Thing? SelectedGun;

        public Dialog_SelectGun(Map map, float maxMass, Action<Thing?>? callback)
        {
			_map = map;
			_callback = callback;
			_maxMass = maxMass;
			this.resizeable = true;
			this.draggable = true;
			this.doCloseX = true;
		}

        public override void DoWindowContents(Rect inRect)
		{
			inRect.SplitHorizontally(inRect.height - 40, out Rect top, out Rect bottom);
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
			_table.AllowSorting = true;

			TableColumn<TableRow<Thing>> colDef = _table.AddColumn("", 30f, DrawIcon);
			colDef.IsFixedWidth = true;
			colDef.OnClicked += (sender, e) =>
			{
				if (e.RowObject is Thing thing)
					Find.WindowStack.Add(new Dialog_InfoCard(thing));
			};


			TableColumn<TableRow<Thing>> colMass = _table.AddColumn("Mass", 60f);
			colDef.IsFixedWidth = true;

			TableColumn<TableRow<Thing>> colLabel = _table.AddColumn("Caption", 1f);
			colLabel.IsFixedWidth = false;

			foreach (var item in _map.listerThings.AllThings)
			{
				if (item.def.IsRangedWeapon && item.Spawned)
				{
					var row = new TableRow<Thing>(item, item.Label.ToLower());
					row[colLabel] = item.LabelCap;
					row.Tag = item;
					
					List<string> tooltips = new List<string>();

					float gunMass = item.def.BaseMass;
					row[colMass] = gunMass.ToStringMass();


					// Is the gun forbidden?
					if ((item as ThingWithComps)?.GetComp<CompForbiddable>()?.Forbidden == true)
					{
						row.Enabled = false;
						tooltips.Insert(0, "Marked as forbidden!".Colorize(Color.red));
					}

					// Is the gun too big?
					if (gunMass > _maxMass)
					{
						row[colMass] = row[colMass].Colorize(Color.red);
						row.Enabled = false;
						tooltips.Insert(0, "Too heavy!".Colorize(Color.red));
					}

					if (row.Enabled == false)
					{
						row[colLabel] = row[colLabel].Colorize(Color.gray);
					}

					tooltips.Add(item.DescriptionDetailed);
					row.Tooltip = tooltips.ToArray();
					_table.AddRow(row);
				}
			}
			_table.Sort(colMass, SortDirection.None, true);
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
