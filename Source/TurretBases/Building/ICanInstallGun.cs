using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TurretBases.Building
{
	public interface ICanInstallGun
	{
		Thing? GunToInstall { get; set; }
		void InstallGun(Thing gun);
	}
}
