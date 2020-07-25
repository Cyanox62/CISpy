using Exiled.API.Interfaces;
using System.Collections.Generic;

namespace CISpy
{
	public class Config : IConfig
	{
		public bool IsEnabled { get; set; } = true;

		public List<int> SpyRoles { get; set; } = new List<int>() { 11, 13 };

		public bool SpawnWithGrenade { get; set; } = true;

		public int SpawnChance { get; set; } = 40;
		public int GuardSpawnChance { get; set; } = 50;
		public int MinimumSquadSize { get; set; } = 6;
	}
}
