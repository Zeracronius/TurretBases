using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurretBases.UserInterface;
using UnityEngine;
using Verse;

namespace TurretBases.Mod
{
	internal class TurretBasesMod : Verse.Mod
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
		public static TurretBasesSettings Settings { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

		private FilterTreeBox? _treeBox;

		public TurretBasesMod(ModContentPack content) 
			: base(content)
		{
			Settings = this.GetSettings<TurretBasesSettings>();
		}


		public override string SettingsCategory()
		{
			return "Turret bases";
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			if (_treeBox == null)
				_treeBox = new FilterTreeBox(Settings.GetSettingsNodes());

			_treeBox.Draw(inRect);
		}
	}
}
